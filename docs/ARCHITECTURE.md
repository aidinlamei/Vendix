# Vendix - Architecture Document

> **Version:** 2.1.0
> **Last Updated:** 2025-12-28
> **Status:** Phase 2 - Core Catalog
> **.NET Version:** 10.0 LTS (Supported until November 2028)

---

## 1. Project Overview

**Vendix** is a global-ready, scalable e-commerce platform built for small to medium retailers.

### Key Characteristics
- 🌍 **Global-ready:** Multi-language (FA/EN), configurable currency & date
- 🏗️ **Scalable:** Clean Architecture + DDD
- 🔌 **Configurable:** Payment/Shipping via admin panel
- 📦 **Product Types:** Physical and digital
- 🧪 **Test-driven:** Unit and integration tests
- ⚡ **Performance:** Built-in caching
- 📝 **Auditable:** Full audit trail

---

## 2. Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| **Runtime** | .NET | 10.0 LTS |
| **Language** | C# | 14 |
| **Frontend** | Blazor Web App (Interactive Auto) | .NET 10 |
| **Backend** | ASP.NET Core Web API | 10.0 |
| **ORM** | Entity Framework Core | 10.0 |
| **Database** | PostgreSQL | 16+ |
| **Caching** | IMemoryCache → Redis | - |
| **Auth** | ASP.NET Core Identity + JWT | 10.0 |
| **Validation** | FluentValidation | 11.x |
| **Mediator** | MediatR | 12.x |
| **Mapping** | Mapster | 7.x |
| **Testing** | xUnit + FluentAssertions + NSubstitute + Testcontainers | Latest |

### Why These Choices?

| Choice | Reason |
|--------|--------|
| **.NET 10 LTS** | Stable until 2028, major performance gains |
| **Blazor** | Single C# stack, mature in .NET 10 |
| **Mapster** | Faster than AutoMapper, compile-time safe |
| **PostgreSQL** | Open source, great JSON support |

---

## 3. Architecture Layers

```
┌─────────────────────────────────────────┐
│            Presentation                 │
│      (Blazor Web App, API Controllers)  │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│            Application                  │
│   (Commands, Queries, DTOs, Interfaces) │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│              Domain                     │
│  (Entities, Value Objects, Aggregates)  │
└─────────────────────────────────────────┘
                  ▲
┌─────────────────┴───────────────────────┐
│           Infrastructure                │
│   (EF Core, Repos, External Services)   │
└─────────────────────────────────────────┘
```

**Dependency Rule:**
- Domain = ZERO dependencies
- Application → Domain only
- Infrastructure implements Application interfaces
- Presentation → Application

---

## 4. Solution Structure

```
Vendix/
├── src/
│   ├── Vendix.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── IAuditableEntity.cs
│   │   │   ├── ISoftDelete.cs
│   │   │   └── IRepository.cs
│   │   ├── Catalog/
│   │   │   ├── Entities/ (Product, Category, Brand, ProductVariant, ProductSpec, ProductImage, Translations)
│   │   │   ├── ValueObjects/ (Money, Sku, Slug)
│   │   │   ├── Enums/ (ProductType)
│   │   │   └── Repositories/
│   │   ├── Ordering/
│   │   │   ├── Entities/ (Order, OrderItem, DigitalDownload)
│   │   │   ├── ValueObjects/ (Address, OrderNumber)
│   │   │   ├── Enums/ (OrderStatus)
│   │   │   └── Repositories/
│   │   ├── Basket/
│   │   │   ├── Entities/ (Basket, BasketItem)
│   │   │   └── Repositories/
│   │   ├── Identity/
│   │   │   ├── Entities/ (ApplicationUser)
│   │   │   ├── Enums/ (UserRole)
│   │   │   └── Repositories/
│   │   ├── Payment/
│   │   │   ├── Entities/ (Payment)
│   │   │   ├── Enums/ (PaymentStatus)
│   │   │   └── Repositories/
│   │   ├── Inventory/
│   │   │   ├── Entities/ (StockItem)
│   │   │   └── Repositories/
│   │   ├── Discounts/
│   │   │   ├── Entities/ (Coupon, ProductDiscount, CategoryDiscount)
│   │   │   ├── Enums/ (DiscountType)
│   │   │   └── Repositories/
│   │   └── Settings/
│   │       ├── Entities/ (StoreSettings)
│   │       └── ValueObjects/ (CurrencySettings, DateTimeSettings)
│   │
│   ├── Vendix.Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/ (IUnitOfWork, ICurrentUser, ICacheService, IPaymentGateway, IPaymentGatewayFactory, IShippingProvider, IFileStorage, IEmailService, ISmsService)
│   │   │   ├── Behaviors/ (Validation, Logging, Caching, Transaction)
│   │   │   ├── Exceptions/ (NotFoundException, ValidationException, BusinessRuleException)
│   │   │   ├── Models/ (Result, PaginatedList)
│   │   │   └── Attributes/ (CacheableQuery)
│   │   ├── Catalog/
│   │   │   ├── Commands/ (CreateProduct, UpdateProduct, DeleteProduct, CreateCategory, UpdateCategory)
│   │   │   ├── Queries/ (GetProducts, GetProductById, GetCategories)
│   │   │   └── Mappings/
│   │   ├── Ordering/
│   │   │   ├── Commands/ (PlaceOrder, CancelOrder, UpdateOrderStatus)
│   │   │   ├── Queries/ (GetOrders, GetOrderById, GetMyOrders)
│   │   │   └── Mappings/
│   │   ├── Basket/
│   │   │   ├── Commands/ (AddToBasket, UpdateBasketItem, RemoveFromBasket, ClearBasket)
│   │   │   └── Queries/ (GetBasket)
│   │   ├── Payment/
│   │   │   ├── Commands/ (InitiatePayment, VerifyPayment)
│   │   │   └── Queries/
│   │   ├── Identity/
│   │   │   ├── Commands/ (Register, Login, SendOtp, VerifyOtp, UpdateProfile)
│   │   │   └── Queries/ (GetUserProfile)
│   │   ├── Discounts/
│   │   │   ├── Commands/ (CreateCoupon, UpdateCoupon, DeleteCoupon, ApplyCoupon)
│   │   │   └── Queries/ (GetCoupons, ValidateCoupon)
│   │   └── Admin/
│   │       └── Settings/
│   │           ├── Commands/ (UpdateStoreSettings, ConfigurePaymentGateway, ConfigureShippingProvider)
│   │           └── Queries/ (GetStoreSettings)
│   │
│   ├── Vendix.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── VendixDbContext.cs
│   │   │   ├── Configurations/
│   │   │   ├── Repositories/
│   │   │   ├── Interceptors/ (AuditableEntityInterceptor, SoftDeleteInterceptor)
│   │   │   ├── Migrations/
│   │   │   └── UnitOfWork.cs
│   │   ├── Identity/ (IdentityService, JwtTokenService, OtpService, CurrentUserService)
│   │   ├── Caching/ (MemoryCacheService, CacheKeys)
│   │   ├── Payment/
│   │   │   ├── PaymentGatewayFactory.cs
│   │   │   └── Gateways/ (Zarinpal, Stripe, PayPal)
│   │   ├── Shipping/
│   │   │   ├── ShippingProviderFactory.cs
│   │   │   └── Providers/
│   │   ├── FileStorage/ (LocalFileStorage)
│   │   ├── Notifications/ (EmailService, SmsService)
│   │   ├── Audit/ (AuditLog, AuditLogService)
│   │   └── DependencyInjection.cs
│   │
│   ├── Vendix.Web/
│   │   ├── Program.cs
│   │   ├── Components/
│   │   │   ├── App.razor
│   │   │   ├── Routes.razor
│   │   │   ├── Layout/ (MainLayout, AdminLayout, NavMenu, Footer)
│   │   │   ├── Pages/
│   │   │   │   ├── Home.razor
│   │   │   │   ├── Catalog/ (Products, ProductDetail, Category)
│   │   │   │   ├── Basket/ (Cart)
│   │   │   │   ├── Checkout/ (Checkout, PaymentResult, DownloadDigital)
│   │   │   │   ├── Account/ (Login, Register, VerifyOtp, Profile, MyOrders)
│   │   │   │   └── Admin/
│   │   │   │       ├── Dashboard.razor
│   │   │   │       ├── Products/, Categories/, Orders/, Customers/, Discounts/, Inventory/
│   │   │   │       ├── AuditLogs/
│   │   │   │       └── Settings/ (General, Payment, Shipping, Localization)
│   │   │   └── Shared/ (ProductCard, Pagination, LoadingSpinner, ConfirmDialog, LanguageSwitcher, Toast)
│   │   ├── Services/
│   │   └── wwwroot/
│   │
│   └── Vendix.Api/
│       ├── Program.cs
│       ├── Controllers/
│       │   ├── v1/ (Catalog, Basket, Order, Payment, Auth, User, Discount)
│       │   └── Admin/ (AdminProduct, AdminOrder, AdminSettings, AuditLog)
│       ├── Middleware/ (ExceptionHandling, RequestLogging, Localization)
│       ├── Filters/
│       └── OpenApi/
│
├── tests/
│   ├── Vendix.Domain.Tests/
│   ├── Vendix.Application.Tests/
│   └── Vendix.Integration.Tests/
│
├── docs/
│   └── ARCHITECTURE.md
│
├── Vendix.sln
├── Directory.Build.props
├── global.json
├── docker-compose.yml
├── .gitignore
├── .editorconfig
└── README.md
```

---

## 5. Bounded Contexts

| Context | Aggregate Root | Child Entities |
|---------|---------------|----------------|
| Catalog | Product | ProductVariant, ProductSpec, ProductImage, ProductTranslation |
| Catalog | Category | CategoryTranslation |
| Ordering | Order | OrderItem, DigitalDownload |
| Basket | Basket | BasketItem |
| Payment | Payment | - |
| Identity | ApplicationUser | - |
| Inventory | StockItem | - |
| Discounts | Coupon | - |
| Settings | StoreSettings | - |

---

## 6. Key Design Patterns

### 6.1 Simplified CQRS
```csharp
// Command → Result<T>
public record CreateProductCommand(string Name, decimal Price) : IRequest<Result<Guid>>;

// Query → DTO
public record GetProductsQuery(int Page, int Size) : IRequest<PaginatedList<ProductDto>>;
```

**Upgrade Path:** Same model now → Separate read models later → Separate read DB if needed

### 6.2 Repository + Unit of Work
```csharp
public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### 6.3 Factory Pattern (Payment/Shipping)
```csharp
public interface IPaymentGatewayFactory
{
    IPaymentGateway Create(string gatewayName);
    IEnumerable<string> GetAvailableGateways();
}
```

### 6.4 Result Pattern
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

---

## 7. Localization (Simplified)

**Languages:** English (en), Persian (fa)

```csharp
public class Product : AggregateRoot
{
    public ICollection<ProductTranslation> Translations { get; }
    
    public string GetTitle(string lang) =>
        Translations.FirstOrDefault(t => t.LanguageCode == lang)?.Title
        ?? Translations.First().Title;
}
```

**Admin Settings:**
```csharp
public class StoreSettings
{
    public string DefaultLanguage { get; set; } = "en";
    public string DefaultCurrency { get; set; } = "USD";
    public string CurrencySymbol { get; set; } = "$";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeZone { get; set; } = "UTC";
}
```

---

## 8. Caching Strategy

| Data | TTL | Invalidation |
|------|-----|--------------|
| Products | 5 min | On change |
| Categories | 30 min | On change |
| Settings | 1 hour | On change |

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}

[CacheableQuery(Key = "products", ExpiryMinutes = 5)]
public record GetProductsQuery(int Page, int Size) : IRequest<PaginatedList<ProductDto>>;
```

---

## 9. Audit Trail

```csharp
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? ModifiedAt { get; set; }
    string? ModifiedBy { get; set; }
}

public class AuditLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }      // Create, Update, Delete
    public string EntityName { get; set; }
    public string EntityId { get; set; }
    public string? OldValues { get; set; }  // JSON
    public string? NewValues { get; set; }  // JSON
    public DateTime Timestamp { get; set; }
}
```

Automatic via EF Core `SaveChangesInterceptor`.

---

## 10. File Storage

```csharp
public interface IFileStorage
{
    Task<string> UploadAsync(Stream file, string fileName, string folder, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    string GetPublicUrl(string path);
    Task<string> GetSignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default);
}
```

**Phase 1:** LocalFileStorage  
**Phase 2+:** S3FileStorage with CDN

---

## 11. API Versioning

```
/api/v1/products
/api/v2/products  (future)
```

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase { }
```

---

## 12. Authentication

**Flow:**
1. Register: Email/Phone → OTP → Verify → Set Password
2. Login: Email/Phone + Password → JWT Token
3. Reset: Email/Phone → OTP → Verify → New Password

**Roles:** Customer, Admin

---

## 13. Testing

| Layer | Target | Focus |
|-------|--------|-------|
| Domain | 90%+ | Business logic, Value Objects |
| Application | 80%+ | Handlers, Validators |
| Infrastructure | 70%+ | Repositories (integration) |

**Tools:** xUnit, FluentAssertions, NSubstitute, Testcontainers, Bogus

**Naming:** `Method_Scenario_ExpectedResult`

---

## 14. Implementation Phases

### Phase 1: Foundation ✅ (Completed: 2025-12-28)
- [x] Architecture document
- [x] Solution structure
- [x] Domain entities (Common, Catalog basics)
- [x] Base infrastructure (DbContext, Identity)
- [x] Basic Blazor layout
- [x] Unit tests setup
- [x] Critical bug fixes (4 items)

### Phase 2: Core Catalog ⏳ (Current)
- [ ] Products CRUD (with translations)
- [x] Category Commands & Queries
- [x] Category Admin Pages
- [x] Brand Commands & Queries
- [ ] Variants & Specs
- [ ] Image upload
- [ ] Caching

### Phase 3: Shopping Flow
- [ ] Basket
- [ ] Checkout
- [ ] Orders

### Phase 4: Payment
- [ ] Gateway abstraction
- [ ] First gateway
- [ ] Admin config

### Phase 5: Users
- [ ] Registration (OTP)
- [ ] Login (JWT)
- [ ] Profile & Orders
- [ ] Comments

### Phase 6: Discounts
- [ ] Coupons
- [ ] Product/Category discounts
- [ ] Time sales

### Phase 7: Inventory & Shipping
- [ ] Stock management
- [ ] Shipping abstraction

### Phase 8: Digital Products
- [ ] Secure downloads

### Phase 9: Admin
- [ ] Audit logs viewer

### Phase 10: Production
- [ ] Redis
- [ ] Performance
- [ ] SEO

---

## 15. Commit Convention

```
feat: add product creation
fix: resolve basket calculation bug
refactor: extract payment logic
test: add order placement tests
docs: update API documentation
```

**Branches:** main → develop → feature/xxx

---

## 16. AI Context

> **For Claude Code, Cursor, etc.:**

| Rule | Detail |
|------|--------|
| Stack | .NET 10, C# 14, Blazor, EF Core 10, PostgreSQL |
| Architecture | Clean. Domain = ZERO deps |
| CQRS | MediatR. Commands → Result<T> |
| Mapping | Mapster (NOT AutoMapper) |
| Validation | FluentValidation |
| Caching | ICacheService |
| Audit | Via EF Interceptor |
| Phase | Check section 14 |

---

## 17. Phase 2: Core Catalog - Detailed Architecture

### 17.1 Products CRUD (Admin Panel)

**Pages to Create:**
```
src/Vendix.Web/Components/Pages/Admin/Products/
├── Index.razor          # Product list with search, filter, pagination
├── Create.razor         # Create new product form
├── Edit.razor           # Edit existing product
└── _ProductForm.razor   # Shared form component
```

**Features:**
- DataGrid with sorting, filtering, search
- Multi-language support (FA/EN) for title & description
- Image upload with drag & drop
- Variant management inline
- Specification key-value editor
- SEO fields (slug, meta)

**API Endpoints:**
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/v1/admin/products | List with pagination |
| GET | /api/v1/admin/products/{id} | Get by ID |
| POST | /api/v1/admin/products | Create |
| PUT | /api/v1/admin/products/{id} | Update |
| DELETE | /api/v1/admin/products/{id} | Soft delete |

---

### 17.2 Categories CRUD

**Pages:**
```
src/Vendix.Web/Components/Pages/Admin/Categories/
├── Index.razor          # Tree view of categories
├── Create.razor         # Create category
└── Edit.razor           # Edit category
```

**Features:**
- Hierarchical tree view
- Drag & drop reordering (future)
- Parent category selection
- Multi-language name & description
- Slug auto-generation

**Commands & Queries to Add:**
- CreateCategoryCommand
- UpdateCategoryCommand
- DeleteCategoryCommand
- GetCategoriesQuery (tree structure)
- GetCategoryByIdQuery

---

### 17.3 Brands CRUD

**Pages:**
```
src/Vendix.Web/Components/Pages/Admin/Brands/
├── Index.razor
├── Create.razor
└── Edit.razor
```

**Features:**
- Simple list view
- Logo upload
- Slug auto-generation

**Commands & Queries to Add:**
- CreateBrandCommand
- UpdateBrandCommand
- DeleteBrandCommand
- GetBrandsQuery
- GetBrandByIdQuery

---

### 17.4 Image Upload

**Interface:**
```csharp
public interface IFileStorage
{
    Task<string> UploadAsync(Stream file, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    string GetPublicUrl(string path);
}
```

**Implementation:** LocalFileStorage (Phase 2)

**Location:** `wwwroot/uploads/{folder}/{guid}_{filename}`

**Validation:**
- Max size: 5MB
- Allowed types: jpg, jpeg, png, webp
- Image optimization (resize to max 1200px)

---

### 17.5 Caching Activation

**Queries to Mark Cacheable:**
```csharp
[CacheableQuery(Key = CacheKeys.Products, ExpiryMinutes = 5)]
public record GetProductsQuery(...) : IRequest<PaginatedList<ProductListDto>>;

[CacheableQuery(Key = CacheKeys.Categories, ExpiryMinutes = 30)]
public record GetCategoriesQuery() : IRequest<List<CategoryDto>>;

[CacheableQuery(Key = CacheKeys.Brands, ExpiryMinutes = 30)]
public record GetBrandsQuery() : IRequest<List<BrandDto>>;
```

**Cache Invalidation:**
- On Create/Update/Delete → RemoveByPrefixAsync(prefix)
- Product change → Invalidate Products cache
- Category change → Invalidate Categories cache
- Brand change → Invalidate Brands cache

---

### 17.6 Public Catalog Pages

**Pages:**
```
src/Vendix.Web/Components/Pages/Catalog/
├── Products.razor       # Product listing with filters
├── ProductDetail.razor  # Single product view
└── Category.razor       # Products by category
```

**Features:**
- Responsive product grid
- Filter by category, brand, price range
- Sort by price, name, newest
- Product detail with image gallery
- Variant selection
- Add to cart button (UI only, Phase 3)

---

### Phase 2 Checklist

| # | Task | Status |
|---|------|--------|
| 1 | Category Commands & Queries | ⬜ |
| 2 | Category Admin Pages | ⬜ |
| 3 | Brand Commands & Queries | ⬜ |
| 4 | Brand Admin Pages | ⬜ |
| 5 | LocalFileStorage Implementation | ⬜ |
| 6 | Product Admin Pages (Index, Create, Edit) | ⬜ |
| 7 | Image Upload Component | ⬜ |
| 8 | Cache Activation on Queries | ⬜ |
| 9 | Public Products Page | ⬜ |
| 10 | Public Product Detail Page | ⬜ |
| 11 | Public Category Page | ⬜ |
| 12 | Unit & Integration Tests | ⬜ |

---

## Appendix: Config Files

**global.json:**
```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor"
  }
}
```

**Directory.Build.props:**
```xml
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

---

*Single source of truth for Vendix architecture.*
*Last Updated: 2025-12-28 by Claude Opus*
