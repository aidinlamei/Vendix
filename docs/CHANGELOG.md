# Changelog

All notable changes to the Vendix project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Phase 2 - Task 4: Brand Admin Pages (Date: 2025-12-28)

**Added:**

Blazor Admin Pages (src/Vendix.Web/Components/Pages/Admin/Brands/):
- `Index.razor` - Brand list with table view, logo preview
- `Create.razor` - Create new brand form with logo URL preview
- `Edit.razor` - Edit existing brand form

**Features:**
- Table view with brand logo, name, slug, product count
- Logo preview from URL (with fallback to initial letter)
- Slug auto-generation option
- Delete confirmation dialog
- Loading states and error handling
- Empty state with call-to-action
- Form validation

**Technical Decisions:**
- Simpler structure than Category (no hierarchy/tree view needed)
- Logo displayed from external URL (no file upload yet - Task 5)
- Reused ConfirmDialog and LoadingSpinner shared components

**Notes:**
- TODO: Add error toast notifications for better UX
- TODO: Replace LogoUrl text input with file upload component (Task 7)

---

### Phase 2 - Task 4: Code Review (Date: 2025-12-28)

**Reviewed By:** Claude Opus (AI Code Review)

**Summary:** Brand Admin Pages implementation reviewed against prompt specifications. All components correctly implemented with minor adaptations for existing component APIs.

---

#### Index.razor Review ✅

**Status:** PASS

**Components Verified:**
- Route: `/admin/brands` ✅
- Layout: `AdminLayout` ✅
- MediatR integration with proper queries/commands ✅
- Header with title and "Add Brand" button ✅
- Loading state with `LoadingSpinner` component ✅
- Empty state with call-to-action ✅
- Table with Brand, Slug, Products, Actions columns ✅
- Logo preview with fallback to initial letter ✅
- Edit/Delete action buttons ✅
- Delete confirmation dialog ✅

**Adaptation:** ConfirmDialog usage adapted from conditional rendering (`@if`) to `IsVisible` parameter binding. This correctly matches the existing ConfirmDialog component API which has internal `@if (IsVisible)` check.

---

#### Create.razor Review ✅

**Status:** PASS

**Components Verified:**
- Route: `/admin/brands/create` ✅
- Layout: `AdminLayout` ✅
- Back button navigation ✅
- EditForm with DataAnnotationsValidator ✅
- Name field (required) ✅
- Slug field with prefix display and auto-generation hint ✅
- Logo URL field with validation message ✅
- Logo preview with @onerror handler ✅
- Cancel/Submit buttons with loading state ✅
- BrandFormModel inner class ✅
- CreateBrandCommand integration ✅

---

#### Edit.razor Review ✅

**Status:** PASS

**Components Verified:**
- Route: `/admin/brands/edit/{Id:guid}` ✅
- Layout: `AdminLayout` ✅
- Id parameter binding ✅
- Loading state while fetching brand ✅
- "Brand not found" error state ✅
- Back button navigation ✅
- EditForm pre-populated with brand data ✅
- Name, Slug, Logo URL fields ✅
- Logo preview ✅
- Cancel/Save buttons with loading state ✅
- UpdateBrandCommand integration ✅

---

#### Documentation Review

**CHANGELOG.md:** ✅
- Task 4 entry present with correct format
- Date included (2025-12-28)
- Added, Features, Technical Decisions, Notes sections populated

**ARCHITECTURE.md:** ✅ (Fixed)
- Task 4 checklist updated from ⬜ to ✅

---

#### Issues Found & Fixed

| Issue | File | Status |
|-------|------|--------|
| Task 4 not marked complete in checklist | ARCHITECTURE.md | ✅ Fixed |

---

#### Recommendations

1. **Error Handling:** Both Create and Edit pages have TODO comments for error toast. Consider implementing Toast notification for better UX.

2. **Form Validation:** BrandFormModel classes don't have DataAnnotations. Validation relies on server-side FluentValidation. Consider adding client-side validation attributes for immediate feedback.

3. **Shared Form Component:** Create.razor and Edit.razor share similar form markup. Consider extracting to `_BrandForm.razor` for DRY principle (optional, low priority).

---

**Verification Status:**
- [x] Index.razor matches prompt specification
- [x] Create.razor matches prompt specification
- [x] Edit.razor matches prompt specification
- [x] CHANGELOG.md updated correctly
- [x] ARCHITECTURE.md checklist fixed
- [ ] `dotnet build` - Not tested in review environment
- [ ] Manual UI testing - Requires runtime

**Recommendation:** Run `dotnet build` and manual testing before merge.

---

### Phase 2 - Task 3: Brand Commands & Queries (Date: 2025-12-28)

**Added:**

Application Layer - Brand DTOs:
- `BrandDto.cs` - Detail view DTO
- `BrandListDto.cs` - Lightweight DTO for lists
- `BrandSelectDto.cs` - DTO for dropdown selection

Application Layer - Brand Commands:
- `CreateBrandCommand.cs` - Create brand with logo URL
- `CreateBrandCommandValidator.cs` - Validation with URL check
- `UpdateBrandCommand.cs` - Update brand details
- `UpdateBrandCommandValidator.cs` - Validation rules
- `DeleteBrandCommand.cs` - Soft delete with product check

Application Layer - Brand Queries:
- `GetBrandByIdQuery.cs` - Get single brand by ID
- `GetBrandBySlugQuery.cs` - Get single brand by slug
- `GetBrandsQuery.cs` - Get all brands (cached)

Application Layer - Mappings:
- `BrandMappingConfig.cs` - Mapster configuration for Brand

Domain/Infrastructure:
- `IProductRepository.GetByBrandAsync` - Get products by brand ID
- `ProductRepository.GetByBrandAsync` - Implementation

**Technical Decisions:**
- Used URL validation in validators to ensure LogoUrl is a valid HTTP/HTTPS URL
- DeleteBrandCommand checks for associated products before allowing deletion
- ProductCount in DTOs set to 0 (TODO: implement via repository)
- Used string literal for cache key "brands" to avoid Application->Infrastructure dependency

**Notes:**
- TODO: Implement ProductCount calculation in BrandMappingConfig
- Brand entity uses UpdateLogoUrl method which accepts null to remove logo
- IProductRepository.GetByBrandAsync added for brand deletion validation

---

### Phase 2 - Task 2: Category Admin Pages (Date: 2025-12-28)

**Added:**

Blazor Admin Pages (src/Vendix.Web/Components/Pages/Admin/Categories/):
- `Index.razor` - Category list with hierarchical tree view
- `CategoryTreeNode.razor` - Recursive tree node component
- `Create.razor` - Create new category form
- `Edit.razor` - Edit existing category form

**Features:**
- Hierarchical tree view with expand/collapse functionality
- Parent category selection (prevents self-reference)
- Multi-language translations support (EN/FA)
- Slug auto-generation option
- Delete confirmation dialog
- Loading states and error handling
- Empty state with call-to-action

**Technical Decisions:**
- Used recursive CategoryTreeNode component for tree rendering
- FlattenTree helper method for parent category dropdown
- ConfirmDialog component for delete confirmation
- LoadingSpinner component for async operations
- Form validation using DataAnnotationsValidator

**Notes:**
- TODO: Add error toast notifications for better UX
- Category description stored in translations, not directly on entity
- Parent category dropdown excludes current category in edit mode

---

### Phase 2 - Task 1: Category Commands & Queries (Date: 2025-12-28)

**Added:**

Application Layer - Category DTOs:
- `CategoryDto.cs` - Detail view DTO with translations and children
- `CategoryListDto.cs` - Lightweight DTO for lists
- `CategoryTranslationDto.cs` - Translation DTO
- `CategoryTreeDto.cs` - Hierarchical tree structure DTO

Application Layer - Category Commands:
- `CreateCategoryCommand.cs` - Create category with translations
- `CreateCategoryCommandValidator.cs` - Validation rules
- `UpdateCategoryCommand.cs` - Update category details
- `UpdateCategoryCommandValidator.cs` - Validation rules
- `DeleteCategoryCommand.cs` - Soft delete category

Application Layer - Category Queries:
- `GetCategoryByIdQuery.cs` - Get single category by ID
- `GetCategoryBySlugQuery.cs` - Get single category by slug
- `GetCategoriesQuery.cs` - Get root categories list (cached)
- `GetCategoryTreeQuery.cs` - Get full category hierarchy (cached)

Application Layer - Mappings:
- `CategoryMappingConfig.cs` - Mapster configuration for Category

**Technical Decisions:**
- Used string literals for cache keys instead of CacheKeys class to avoid Application layer dependency on Infrastructure
- Category entity doesn't have Description property, so Description is stored in CategoryTranslation
- Translations are replaced entirely on update (remove all, add new ones)
- If Description is provided but no translations, it's added to default "en" translation

**Notes:**
- TODO: Implement circular reference check in UpdateCategoryCommand
- Category entity uses SetParentCategory method which already prevents self-reference
- ICategoryRepository.GetBySlugAsync accepts string (not Slug value object)
### Phase 2 - Core Catalog Started (Date: 2025-12-28)

**Goals:**
- Categories CRUD (Admin)
- Brands CRUD (Admin)
- Products CRUD (Admin) with image upload
- Public catalog pages
- Caching activation

**Status:** In Progress ⏳

---

### Phase 1 - CLOSED ✅ (Date: 2025-12-28)

**Summary:** Foundation phase completed successfully.

**Delivered:**
- ✅ Clean Architecture solution structure
- ✅ Domain layer (Common, Catalog entities, Value Objects)
- ✅ Application layer (CQRS, Pipeline Behaviors, Exceptions)
- ✅ Infrastructure layer (EF Core, Repositories, Caching)
- ✅ Basic Blazor layouts (Main, Admin)
- ✅ Shared components (Toast, Pagination, LoadingSpinner, ConfirmDialog)
- ✅ Unit tests foundation
- ✅ 4 Critical bug fixes

**Statistics:**
- Files created: 113+
- Domain entities: 10
- Value objects: 3
- Repository implementations: 3
- Pipeline behaviors: 3 (Logging, Validation, Caching)
- Shared components: 4

**Next:** Phase 2 - Core Catalog

---

### Phase 1 - Critical Bug Fixes Review (Date: 2025-12-28)

**Reviewed By:** Claude Code (AI Code Review)

**Summary:** All 4 critical fixes requested in the Phase 1 bug report were successfully applied and verified.

---

#### Fix 1: Variable Name Bug in CreateProductCommand.cs ✅

**File:** `src/Vendix.Application/Catalog/Commands/CreateProductCommand.cs:47-52`

**Problem:** Variable named `existingSku` but actually checks for Slug duplicate - misleading name.

**Before:**
```csharp
var existingSku = await productRepository.GetBySlugAsync(
    new Slug(request.Slug), cancellationToken);
if (existingSku is not null)
{
    throw new ConflictException("Product", "Slug", request.Slug);
}
```

**After:**
```csharp
var existingBySlug = await productRepository.GetBySlugAsync(
    new Slug(request.Slug), cancellationToken);
if (existingBySlug is not null)
{
    throw new ConflictException("Product", "Slug", request.Slug);
}
```

**Review:** Variable correctly renamed to `existingBySlug` to match its purpose (checking for slug duplicates).

---

#### Fix 2: Division by Zero in PaginatedList.cs ✅

**File:** `src/Vendix.Application/Common/Models/PaginatedList.cs:54-71`

**Problem:** If `pageSize` is 0, division by zero occurs in TotalPages calculation.

**Before:**
```csharp
public PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
{
    Items = items;
    TotalCount = totalCount;
    PageNumber = pageNumber;
    PageSize = pageSize;
    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize); // DivideByZero!
}
```

**After:**
```csharp
public PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
{
    if (pageSize <= 0)
    {
        throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
    }

    if (pageNumber <= 0)
    {
        throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
    }

    Items = items;
    TotalCount = totalCount;
    PageNumber = pageNumber;
    PageSize = pageSize;
    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
}
```

**Review:** Guard clauses added for both `pageSize` and `pageNumber` with descriptive `ArgumentException` messages. Follows fail-fast principle.

---

#### Fix 3: MarkAsDeleted() Not Setting DeletedBy ✅

**Files:**
- `src/Vendix.Domain/Catalog/Entities/Brand.cs:116-121`
- `src/Vendix.Domain/Catalog/Entities/Category.cs:207-212`
- `src/Vendix.Domain/Catalog/Entities/Product.cs:470-475`

**Problem:** `MarkAsDeleted()` only sets `IsDeleted` and `DeletedAt`, but `DeletedBy` was left for interceptor. This creates inconsistency if called outside EF context.

**Before (all 3 files):**
```csharp
public void MarkAsDeleted()
{
    IsDeleted = true;
    DeletedAt = DateTime.UtcNow;
}
```

**After (all 3 files):**
```csharp
public void MarkAsDeleted(string? deletedBy = null)
{
    IsDeleted = true;
    DeletedAt = DateTime.UtcNow;
    DeletedBy = deletedBy;
}
```

**Review:** Optional `deletedBy` parameter added to all 3 entities. Maintains backward compatibility (parameter is optional) while allowing explicit user tracking when called outside EF context.

---

#### Fix 4: RowVersion Empty Array Initialization ✅

**File:** `src/Vendix.Domain/Common/AggregateRoot.cs:32`

**Problem:** `RowVersion` initialized with empty array `[]`, but EF Core expects `null` for new entities.

**Before:**
```csharp
public byte[] RowVersion { get; set; } = [];
```

**After:**
```csharp
public byte[] RowVersion { get; set; } = null!;
```

**Review:** Changed to `null!` (null-forgiving operator) to let EF Core handle RowVersion initialization. The `!` suppresses nullable warning since EF Core will always populate this value.

---

**Verification Status:**
- [x] All 4 fixes verified in source code
- [ ] `dotnet build` - Not tested (dotnet not available in review environment)
- [ ] `dotnet test` - Not tested (dotnet not available in review environment)

**Recommendation:** Run `dotnet build && dotnet test` locally to confirm all tests pass.

---

### Phase 1 - Core Models and CRUD Implementation (Date: 2025-12-27)

**Added:**

Application Layer - Core Models (src/Vendix.Application/Common/Models/):
- `Result.cs` - Result pattern implementation:
  - Generic `Result<T>` class with IsSuccess, IsFailure, Value, Error properties
  - Static factory methods: Success(T value), Failure(string error)
  - Implicit conversion from T to Result<T>
  - Map and Bind methods for functional composition
  - Match method for pattern matching
  - Non-generic `Result` class for commands without return value

- `PaginatedList.cs` - Paginated list for query results:
  - Items, PageNumber, PageSize, TotalCount, TotalPages properties
  - HasPreviousPage, HasNextPage computed properties
  - Static CreateAsync method that takes IQueryable<T> and applies Skip/Take
  - Static Create method for in-memory collections
  - Static Empty method for empty paginated results

Application Layer - Custom Exceptions (src/Vendix.Application/Common/Exceptions/):
- `NotFoundException.cs` - For entity not found scenarios:
  - EntityName and Key properties
  - Static ForEntity<T> factory method

- `ValidationException.cs` - For FluentValidation failures:
  - Dictionary<string, string[]> Errors property
  - Constructor accepting IEnumerable<ValidationFailure>

- `BusinessRuleException.cs` - For domain rule violations:
  - RuleName and Details properties

- `ConflictException.cs` - For duplicate entries:
  - EntityName, PropertyName, ConflictingValue properties
  - Static ForDuplicate<T> factory method

Application Layer - Pipeline Behaviors (src/Vendix.Application/Common/Behaviors/):
- `ValidationBehavior.cs` - MediatR pipeline behavior:
  - Injects IEnumerable<IValidator<TRequest>>
  - Runs all validators before handler
  - Throws ValidationException with all errors if validation fails
  - Uses async validation

- `LoggingBehavior.cs` - MediatR pipeline behavior:
  - Injects ILogger<LoggingBehavior<TRequest, TResponse>>
  - Logs request name, user info (if available), timestamp
  - Uses Stopwatch to measure response time
  - Logs warnings for slow requests (>500ms)

Application Layer - Services (src/Vendix.Application/Common/Interfaces/):
- `ICurrentUserService.cs` - Interface for current user information:
  - UserId, UserName, IsAuthenticated properties
  - IsInRole(string role) method

Application Layer - Product CRUD (src/Vendix.Application/Catalog/):
- DTOs (DTOs/ProductDto.cs):
  - ProductDto for detail views
  - ProductListDto for list views
  - ProductVariantDto, ProductSpecificationDto, ProductImageDto
  - CreateProductDto, UpdateProductDto for inputs

- Commands:
  - CreateProductCommand with handler and validator
  - UpdateProductCommand with handler and validator
  - DeleteProductCommand with handler (soft delete)

- Queries:
  - GetProductByIdQuery with handler
  - GetProductBySlugQuery with handler
  - GetProductsQuery with pagination and filters

- Mappings (Mappings/ProductMappingConfig.cs):
  - Mapster IRegister implementation
  - Product to ProductDto/ProductListDto mappings
  - Variant, Specification, Image mappings
  - Money value object to decimal conversion

Infrastructure Layer - Identity (src/Vendix.Infrastructure/Identity/):
- `CurrentUserService.cs` - ICurrentUserService implementation:
  - Uses IHttpContextAccessor
  - Gets user info from HttpContext.User claims
  - Returns null for UserId/UserName if not authenticated

Domain Layer - Concurrency:
- `AggregateRoot.cs` - Added RowVersion property for optimistic concurrency

**Updated:**

- `AuditableEntityInterceptor.cs` - Inject ICurrentUserService (optional):
  - Sets CreatedBy, ModifiedBy, DeletedBy from current user

- `DependencyInjection.cs` (Application) - Register behaviors:
  - Added LoggingBehavior and ValidationBehavior to MediatR pipeline

- `DependencyInjection.cs` (Infrastructure) - Register services:
  - Added HttpContextAccessor registration
  - Added ICurrentUserService -> CurrentUserService as Scoped

- EF Configurations - Added RowVersion:
  - ProductConfiguration: Added RowVersion as concurrency token
  - CategoryConfiguration: Added RowVersion as concurrency token
  - BrandConfiguration: Added RowVersion as concurrency token

- ProductVariant Entity - Fixed PriceAdjustment:
  - Changed from Money to PriceAdjustmentAmount (decimal) and PriceAdjustmentCurrency (string)
  - Added GetFinalPrice(Money basePrice) method
  - Now allows negative amounts for discounts

- ProductVariantConfiguration - Updated for new properties:
  - Removed OwnsOne for PriceAdjustment
  - Configured PriceAdjustmentAmount with precision (18, 4)
  - Configured PriceAdjustmentCurrency with max length 3

- Program.cs (Web and Api) - Added service registration:
  - Added AddApplication() and AddInfrastructure() calls
  - Configured connection string from configuration

- appsettings.json (Web and Api) - Added connection string:
  - DefaultConnection for PostgreSQL

Tests:
- `CreateProductCommandValidatorTests.cs` - Validator tests:
  - Valid command passes
  - Empty name fails
  - Invalid SKU format fails
  - Negative price fails
  - Currency length validation

- `ValidationBehaviorTests.cs` - Behavior tests:
  - Valid request passes through
  - Invalid request throws ValidationException
  - No validators passes through
  - Multiple validators combine errors

- `BrandTests.cs` - Brand aggregate tests:
  - Creation with valid inputs
  - Name and slug updates
  - Logo URL update
  - IAuditableEntity and ISoftDelete implementation

- `README.md` - Updated with:
  - Technology stack
  - Getting started guide
  - Project structure
  - Key features
  - Development instructions

**Technical Decisions:**
- Result pattern provides functional error handling without exceptions for expected failures
- Pipeline behaviors centralize cross-cutting concerns (validation, logging)
- ICurrentUserService is optional in interceptor to support non-web scenarios
- RowVersion uses byte[] for database-agnostic concurrency tokens
- ProductVariant PriceAdjustment split into separate properties to allow negative values
- Commands use record types for immutability
- Validators use FluentValidation with async validation
- Mapster used for object mapping with IRegister pattern

---

### Phase 1 Fixes and Completions (Date: 2025-12-27)

**Fixed:**

Critical Fixes:
- `Toast.razor` - Added `@implements IDisposable` directive after comment block for proper timer cleanup
- `LoadingSpinner.razor` - Changed invalid `border-3` to `border-2` in medium spinner size (border-3 doesn't exist in Tailwind CSS)
- `CategoryConfiguration.cs` - Changed parent category relationship from `DeleteBehavior.Restrict` to `DeleteBehavior.SetNull` to properly handle category deletion

**Added:**

Domain Layer:
- `IBrandRepository.cs` - Repository interface for Brand aggregate with GetBySlugAsync and GetAllAsync methods
- Updated `Brand.cs` to extend AggregateRoot (was BaseEntity) to support repository pattern
- Added ISoftDelete interface to Brand entity (IsDeleted, DeletedAt, DeletedBy properties)

Infrastructure Layer - Repositories (src/Vendix.Infrastructure/Persistence/Repositories/):
- `ProductRepository.cs` - Full implementation of IProductRepository with:
  - GetByIdAsync, GetBySlugAsync with eager loading of Variants, Specifications, Images, Translations
  - GetByCategoryAsync for filtering products by category
  - SearchAsync with support for search term, category, brand, and price range filters
  - CRUD operations (AddAsync, Update, Delete)

- `CategoryRepository.cs` - Full implementation of ICategoryRepository with:
  - GetByIdAsync, GetBySlugAsync with Translations eager loading
  - GetRootCategoriesAsync for top-level categories
  - GetWithChildrenAsync with nested subcategory loading
  - CRUD operations (AddAsync, Update, Delete)

- `BrandRepository.cs` - Full implementation of IBrandRepository with:
  - GetByIdAsync, GetBySlugAsync
  - GetAllAsync for retrieving all brands
  - CRUD operations (AddAsync, Update, Delete)

Infrastructure Layer - Configuration Updates:
- `BrandConfiguration.cs` - Added soft delete properties configuration (IsDeleted default, DeletedBy max length, query filter)
- `DependencyInjection.cs` - Registered all repository implementations (IProductRepository, ICategoryRepository, IBrandRepository)

Application Layer:
- `DependencyInjection.cs` - Extension method to register application services:
  - MediatR with assembly scanning
  - FluentValidation validators from assembly
  - Mapster TypeAdapterConfig and ServiceMapper

Tests (tests/Vendix.Domain.Tests/Catalog/):
- `SkuTests.cs` - Comprehensive tests for SKU value object:
  - Valid SKU creation and uppercase normalization
  - Invalid format rejection (empty, too short, too long, invalid chars)
  - Min/max length boundary tests
  - IsValid static method tests
  - Equality and hash code tests

- `SlugTests.cs` - Comprehensive tests for Slug value object:
  - Valid slug creation and lowercase normalization
  - FromText generation from various inputs
  - Invalid format rejection (consecutive hyphens, start/end hyphen, invalid chars)
  - Min/max length boundary tests
  - IsValid static method tests
  - Equality and hash code tests

- `CategoryTests.cs` - Comprehensive tests for Category aggregate:
  - Category creation with and without parent
  - Name and slug updates
  - Self-parent validation
  - Translation add/remove/get operations
  - GetName with language fallback
  - IAuditableEntity and ISoftDelete implementation

**Technical Decisions:**
- Brand promoted from BaseEntity to AggregateRoot to support repository pattern (IRepository requires AggregateRoot)
- All read-only repository methods use AsNoTracking() for better query performance
- Repository implementations use constructor injection for VendixDbContext
- Product searches use case-insensitive matching via ToLower()
- Category hierarchy loaded with ThenInclude for subcategory translations
- Tests follow existing patterns with FluentAssertions and Theory/InlineData for parameterized tests

**Next Steps:**
- Step 1.6: Set up unit test project structure (completed)
- Step 2.1: Implement Product CRUD with Application layer

---

### Step 1.5 - Blazor Layout Components (Date: 2025-12-27)

**Added:**

Tailwind CSS Integration:
- Added Tailwind CSS CDN to `App.razor` with custom Vendix brand color palette
- Configured primary (blue), secondary (slate), and accent (purple) color schemes
- Added utility colors for success, warning, error, and info states

Layout Components (src/Vendix.Web/Components/Layout/):
- `MainLayout.razor` - Public site layout with:
  - Responsive header with logo, navigation links (Home, Products, Categories)
  - Search bar with icon
  - Cart icon with badge counter
  - Login button
  - Mobile hamburger menu with full navigation
  - Footer with brand info, quick links, support links, social media icons
  - Top promotional bar ("Free shipping on orders over $50")

- `AdminLayout.razor` - Admin panel layout with:
  - Collapsible sidebar with navigation (Dashboard, Products, Categories, Orders, Customers, Settings)
  - Top bar with notifications and user dropdown menu
  - Back to Store link
  - Mobile-responsive with slide-out sidebar
  - Active link highlighting with NavLink component

Shared Components (src/Vendix.Web/Components/Shared/):
- `LoadingSpinner.razor` - Loading indicator with size options (small, medium, large) and optional text
- `Toast.razor` - Toast notification component with:
  - Support for success, error, warning, info types
  - Auto-dismiss with configurable duration
  - Close button and animated slide-in
- `ConfirmDialog.razor` - Confirmation modal with:
  - Support for info, warning, danger types
  - Customizable title, message, and button text
  - Backdrop click to cancel
- `Pagination.razor` - Pagination component with:
  - Page numbers with ellipsis for large page counts
  - Previous/Next navigation
  - Items count display ("Showing X to Y of Z results")
  - Configurable max visible pages

Page Updates:
- `Home.razor` - Complete redesign with:
  - Hero section with gradient background and CTA buttons
  - Features section (Free Shipping, Secure Payment, 24/7 Support)
  - Featured Products grid with placeholder cards
  - Shop by Category section with icons
  - Newsletter subscription section

- `Admin/Dashboard.razor` - New admin dashboard with:
  - Stats cards (Total Revenue, Orders, Products, Customers) with trends
  - Recent Orders table with status badges
  - Quick Actions panel (Add Product, Add Category, View Orders, Settings)
  - Top Products list

Import Updates:
- Updated `_Imports.razor` with Layout and Shared component namespaces

**Technical Decisions:**
- Used Tailwind CSS via CDN for rapid prototyping (production should use build pipeline)
- Mobile-first responsive design with md: and lg: breakpoints
- Component parameters use standard Blazor patterns (Parameter, EventCallback)
- AdminLayout uses NavLink for automatic active state management
- Toast component implements IDisposable for timer cleanup
- Pagination uses 1-based page indexing for user-friendly display
- All components include XML documentation comments
- SVG icons used throughout for scalability and performance

**Brand Colors:**
- Primary: Blue (#3b82f6) - Main brand color
- Secondary: Slate (#64748b) - Neutral elements
- Accent: Purple (#d946ef) - Highlights
- Success: Green (#10b981)
- Warning: Amber (#f59e0b)
- Error: Red (#ef4444)
- Info: Blue (#3b82f6)

**Next Steps:**
- Step 1.6: Set up unit test project structure
- Step 2.1: Implement Product CRUD with Application layer

---

### Step 1.4 - Infrastructure Layer (Date: 2025-12-27)

**Added:**

NuGet Packages:
- `Microsoft.EntityFrameworkCore` (10.*) - Core EF functionality
- `Microsoft.EntityFrameworkCore.Design` (10.*) - Design-time tools for migrations
- `Npgsql.EntityFrameworkCore.PostgreSQL` (10.*) - PostgreSQL provider
- `MediatR` (12.*) - Mediator pattern for CQRS
- `FluentValidation` (11.*) - Validation library
- `FluentValidation.DependencyInjectionExtensions` (11.*) - DI integration for FluentValidation
- `Mapster` (7.*) - Fast object mapping
- `Mapster.DependencyInjection` (1.*) - DI integration for Mapster

Application Layer:
- `IUnitOfWork.cs` - Interface for unit of work pattern with SaveChangesAsync method
- `IDateTimeProvider.cs` - Interface for abstracting DateTime.UtcNow for testability

Infrastructure/Persistence:
- `VendixDbContext.cs` - Main DbContext with DbSets for all Catalog entities
- `UnitOfWork.cs` - Implementation of IUnitOfWork wrapping DbContext.SaveChangesAsync

Infrastructure/Persistence/Configurations:
- `ProductConfiguration.cs` - EF Core configuration for Product aggregate and child entities
- `CategoryConfiguration.cs` - EF Core configuration for Category aggregate and translations
- `BrandConfiguration.cs` - EF Core configuration for Brand entity

Infrastructure/Persistence/Interceptors:
- `AuditableEntityInterceptor.cs` - Automatically sets CreatedAt, ModifiedAt, DeletedAt fields

Infrastructure/Services:
- `DateTimeProvider.cs` - Default implementation of IDateTimeProvider using DateTime.UtcNow

Infrastructure:
- `DependencyInjection.cs` - Extension method to register all infrastructure services

**Technical Decisions:**
- Value objects (Money, Sku, Slug) use HasConversion for EF Core persistence
- Money is configured as an owned entity with separate Amount and Currency columns
- Soft delete implemented via global query filter on ISoftDelete entities
- Slug fields have unique indexes for URL routing and SEO
- All collections use PropertyAccessMode.Field to access backing fields
- AuditableEntityInterceptor handles automatic audit field population
- PostgreSQL configured with retry-on-failure (3 retries, 30s max delay)
- Debug builds enable sensitive data logging and detailed errors

**Next Steps:**
- Step 1.5: Create basic Blazor layout components
- Step 1.6: Set up unit test project structure

---

### Step 1.3 - Catalog Domain Entities (Date: 2025-12-27)

**Added:**

Enums:
- `ProductType.cs` - Enumeration for product types (Physical, Digital)

Value Objects:
- `Money.cs` - Immutable monetary value with Amount and Currency, including validation and arithmetic operations
- `Sku.cs` - Stock Keeping Unit value object with alphanumeric validation (3-50 chars)
- `Slug.cs` - URL-friendly slug value object with format validation and generation from text

Entities:
- `Product.cs` - Aggregate root with full product management (variants, specs, images, translations)
- `Category.cs` - Aggregate root for hierarchical product categorization with translations
- `Brand.cs` - Entity for product brands with name, slug, and logo
- `ProductVariant.cs` - Entity for product variants with SKU, price adjustment, and stock
- `ProductSpecification.cs` - Entity for product specifications (key-value pairs)
- `ProductImage.cs` - Entity for product images with URL, alt text, sort order, and main flag
- `ProductTranslation.cs` - Entity for product translations (title, description by language)
- `CategoryTranslation.cs` - Entity for category translations (name, description by language)

Repository Interfaces:
- `IProductRepository.cs` - Repository interface with GetBySlugAsync, GetByCategoryAsync, SearchAsync
- `ICategoryRepository.cs` - Repository interface with GetBySlugAsync, GetRootCategoriesAsync, GetWithChildrenAsync

Tests:
- `MoneyTests.cs` - Comprehensive tests for Money value object (creation, arithmetic, equality)
- `ProductTests.cs` - Tests for Product aggregate root (creation, variants, specs, images, translations)

**Technical Decisions:**
- Product and Category are aggregate roots; other entities belong to their aggregates
- Value objects (Money, Sku, Slug) are immutable with validation in constructor
- Rich domain model with business logic in entities (not anemic)
- Entities validate invariants in constructors and methods
- Used C# 14 partial classes with [GeneratedRegex] for SKU and Slug validation patterns
- Money supports arithmetic operations (+, -, *) with currency matching enforcement
- Slug.FromText() provides automatic slug generation from arbitrary text
- Product automatically sets first image as main; maintains main image invariant
- Translations use ISO 639-1 language codes normalized to lowercase
- All entities use file-scoped namespaces and XML documentation

**Next Steps:**
- Step 1.4: Set up DbContext and base infrastructure
- Step 1.5: Create EF Core configurations for Catalog entities

---

### Step 1.2 - Domain/Common (Date: 2025-12-27)

**Added:**
- `BaseEntity.cs` - Base class for all domain entities with Guid Id and equality by identity
- `AggregateRoot.cs` - Base class for aggregate roots with domain events collection
- `ValueObject.cs` - Abstract base class for value objects with equality by values
- `IAuditableEntity.cs` - Interface for entities with audit trail (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy)
- `ISoftDelete.cs` - Interface for soft-deletable entities (IsDeleted, DeletedAt, DeletedBy)
- `IRepository.cs` - Generic repository interface for aggregate roots
- `IDomainEvent.cs` - Marker interface for domain events (in AggregateRoot.cs)
- `DomainEventBase.cs` - Base record for domain events with OccurredOn timestamp (in AggregateRoot.cs)
- `BaseEntityTests.cs` - Unit tests for BaseEntity equality by Id
- `ValueObjectTests.cs` - Unit tests for ValueObject equality by values

**Technical Decisions:**
- Used file-scoped namespaces for cleaner code structure
- BaseEntity uses Guid for Id to ensure uniqueness across distributed systems
- Transient entities (with empty Guid) are never considered equal
- Domain events are stored as a list in AggregateRoot and cleared after dispatch
- ValueObject uses GetEqualityComponents() pattern for flexible equality comparison
- IRepository is constrained to AggregateRoot types following DDD principles
- Used C# 14 collection expressions (`[]`) for list initialization
- DomainEventBase is a record for immutability and built-in equality

**Next Steps:**
- Step 1.3: Create Catalog domain entities (Product, Category, Brand)
- Step 1.4: Create Catalog value objects (Money, Sku, Slug)
- Step 1.5: Set up DbContext and base infrastructure

---
### Phase 1 - Remaining Fixes (Date: 2025-12-28)

**Added:**

Caching Infrastructure:
- `ICacheService.cs` - Cache service interface with methods:
  - GetAsync<T>, SetAsync<T>, RemoveAsync, RemoveByPrefixAsync
  - GetOrCreateAsync<T> for cache-aside pattern

- `MemoryCacheService.cs` - IMemoryCache implementation:
  - Configurable default expiration
  - Key tracking for prefix-based invalidation
  - Logging of cache hits/misses

- `CacheKeys.cs` - Static class with cache key constants:
  - Products, Categories, Brands prefixes
  - Helper methods for generating consistent keys
  - Default TTL constants (Products: 5min, Categories: 30min)

- `CacheableQueryAttribute.cs` - Attribute for marking cacheable queries:
  - Key property for custom cache keys
  - ExpiryMinutes for custom expiration
  - BypassCache for runtime cache bypass

- `CachingBehavior.cs` - MediatR pipeline behavior:
  - Auto-caches queries with CacheableQuery attribute
  - Generates cache keys from request properties
  - Logs cache hits/misses

API Rate Limiting:
- Global rate limiter (100 requests/minute per user/IP)
- Auth-specific policy (10 requests/minute)
- Search-specific policy (30 requests/minute)
- Custom 429 response with retry-after header

Domain Improvements:
- `FinalPriceResult.cs` - Record type for price calculation results:
  - Price: The calculated final price
  - WasClampedToZero: Indicates negative price was clamped

Tests:
- `ProductVariantPriceTests.cs` - Tests for GetFinalPriceWithInfo:
  - Positive/negative/zero adjustments
  - Clamping behavior when result is negative
  - Currency mismatch handling
  - Backward compatibility with GetFinalPrice

**Fixed:**

ProductVariant.GetFinalPrice Warning Issue:
- Added `GetFinalPriceWithInfo()` method returning `FinalPriceResult`
- Result includes `WasClampedToZero` flag for callers to log warnings
- Original `GetFinalPrice()` maintained for backward compatibility

N+1 Query in ProductRepository.SearchAsync:
- Removed unnecessary Includes (Variants, Specifications, Translations)
- Now only includes Images.Where(i => i.IsMain) for list view
- Added Category and Brand includes for display
- Added AsSplitQuery() for better performance
- Added OrderByDescending(CreatedAt) for consistent ordering

**Updated:**

DependencyInjection (Application):
- Added CachingBehavior to MediatR pipeline
- Pipeline order: Logging → Validation → Caching → Handler

DependencyInjection (Infrastructure):
- Added IMemoryCache registration
- Added CacheSettings configuration
- Added ICacheService → MemoryCacheService registration

Program.cs (API):
- Added Rate Limiting middleware
- Configured global and policy-specific rate limits
- Added custom 429 response handler

**Technical Decisions:**
- FinalPriceResult uses record type for immutability and value semantics
- Cache key generation uses JSON serialization with hash for uniqueness
- Rate limiting uses FixedWindowLimiter for simplicity and predictability
- Search query only loads main image for performance (N+1 fix)
- CachingBehavior checks for null response before caching

---

### Phase 1 - Core Models and CRUD Implementation (Date: 2025-12-27)

**Added:**

Application Layer - Core Models (src/Vendix.Application/Common/Models/):
- `Result.cs` - Result pattern implementation
- `PaginatedList.cs` - Paginated list for query results

Application Layer - Custom Exceptions (src/Vendix.Application/Common/Exceptions/):
- `NotFoundException.cs` - For entity not found scenarios
- `ValidationException.cs` - For FluentValidation failures
- `BusinessRuleException.cs` - For domain rule violations
- `ConflictException.cs` - For duplicate entries

Application Layer - Pipeline Behaviors (src/Vendix.Application/Common/Behaviors/):
- `ValidationBehavior.cs` - MediatR pipeline behavior for validation
- `LoggingBehavior.cs` - MediatR pipeline behavior for logging

Application Layer - Services (src/Vendix.Application/Common/Interfaces/):
- `ICurrentUserService.cs` - Interface for current user information

Application Layer - Product CRUD (src/Vendix.Application/Catalog/):
- DTOs, Commands, Queries, Mappings for Product management

Infrastructure Layer - Identity:
- `CurrentUserService.cs` - ICurrentUserService implementation

Domain Layer - Concurrency:
- `AggregateRoot.cs` - Added RowVersion property

**Technical Decisions:**
- Result pattern provides functional error handling
- Pipeline behaviors centralize cross-cutting concerns
- RowVersion uses byte[] for database-agnostic concurrency tokens

---

### Phase 1 Fixes and Completions (Date: 2025-12-27)

**Fixed:**
- `Toast.razor` - Added `@implements IDisposable` for proper timer cleanup
- `LoadingSpinner.razor` - Changed invalid `border-3` to `border-2`
- `CategoryConfiguration.cs` - Changed DeleteBehavior to SetNull

**Added:**
- `IBrandRepository.cs` - Repository interface for Brand
- Repository implementations (Product, Category, Brand)
- Domain tests (Sku, Slug, Category)

---

### Step 1.5 - Blazor Layout Components (Date: 2025-12-27)

**Added:**
- Tailwind CSS integration with custom brand colors
- Layout Components (MainLayout, AdminLayout)
- Shared Components (LoadingSpinner, Toast, ConfirmDialog, Pagination)
- Home page with hero, features, products sections
- Admin Dashboard with stats, orders, quick actions

---

### Step 1.4 - Infrastructure Layer (Date: 2025-12-27)

**Added:**
- VendixDbContext with all entity configurations
- UnitOfWork implementation
- AuditableEntityInterceptor
- DateTimeProvider service

---

### Step 1.3 - Catalog Domain Entities (Date: 2025-12-27)

**Added:**
- Value Objects (Money, Sku, Slug)
- Entities (Product, Category, Brand, ProductVariant, etc.)
- Repository interfaces

---

### Step 1.2 - Domain/Common (Date: 2025-12-27)

**Added:**
- BaseEntity, AggregateRoot, ValueObject base classes
- IAuditableEntity, ISoftDelete interfaces
- IRepository generic interface
- Domain event infrastructure
