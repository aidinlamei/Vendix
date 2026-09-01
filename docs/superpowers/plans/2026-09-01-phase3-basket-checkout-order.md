# Phase 3: Basket, Checkout &amp; Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the guest, localStorage-only cart with a real server-persisted `Basket` bounded context (matching `docs/ARCHITECTURE.md` §5), and add the missing `Order` bounded context plus a Checkout flow and Admin Orders pages, so a visitor can go from "add to cart" to a placed, admin-visible order.

**Architecture:** Two new bounded contexts, built end-to-end per context (Domain → Infrastructure → Application → Web), following the exact patterns already established by the Catalog context (`AggregateRoot`, `IRepository<T>`, MediatR CQRS returning `Result<T>`, Mapster `IRegister` mappings, FluentValidation validators, EF Core owned-entity conversions for `Money`). Since there is no authentication yet (Phase 5 is still 0%), baskets and orders are keyed by an anonymous `BuyerId` — a GUID persisted in the browser's `localStorage`, mirroring the exact JS-interop pattern the current `CartService` already uses for cart data. Because `Vendix.Web` runs in pure Interactive Server mode (confirmed: `Program.cs` calls `.AddInteractiveServerRenderMode()` only, no WebAssembly), Blazor services can inject `IMediator` directly and call Application-layer commands/queries in-process — no REST API hop needed, consistent with how existing admin pages already `@inject IMediator Mediator`.

**Tech Stack:** .NET 10 / C# 14, EF Core 10 + PostgreSQL, MediatR 12, FluentValidation, Mapster, Blazor Interactive Server, existing `ICacheService`/`IUnitOfWork` abstractions. No new NuGet packages required.

**Spec:** `docs/ARCHITECTURE.md` §4 (`Ordering/`, `Basket/` folders), §5 (Basket/Ordering bounded contexts table), §14 Phase 3 (`Basket`, `Checkout`, `Orders`). `docs/superpowers/plans/2026-09-01-phase2-task12-catalog-integration-tests.md` Task 1 (`DatabaseFixture`) — **this plan's Task 14 depends on that file existing**; if Phase 2 Task 12 hasn't been executed yet, do Task 14's fixture step first (copy the `DatabaseFixture`/`DatabaseCollection` code from that plan's Task 1 verbatim) before writing Basket/Order integration tests.

## Global Constraints

- .NET 10 / C# 14, file-scoped namespaces, XML doc comments on all public members (project-wide convention, verified in every existing entity/command/query file).
- Domain layer has ZERO dependencies — never reference `Vendix.Application` types (e.g. `BusinessRuleException`) from `Vendix.Domain`; use plain `ArgumentException`/`InvalidOperationException` instead, exactly like `Money.Subtract` does.
- Commands return `Result<T>` (or `Result` for no payload); queries that represent "this might not exist" return `Result<T>`/throw `NotFoundException`, queries that always have a sensible empty answer (like "get my basket") return the DTO directly — matches the existing `GetProductByIdQuery` (Result) vs. `GetProductsQuery` (plain `PaginatedList`) split.
- Mapping is Mapster only (`IRegister` classes scanned via `config.Scan(assembly)` in `Vendix.Application/DependencyInjection.cs`) — never AutoMapper.
- Validation is FluentValidation, registered automatically via `services.AddValidatorsFromAssembly(assembly)` — just add the validator class, no manual registration.
- Money is always an EF Core owned entity (`builder.OwnsOne(...)`) with `Amount` (`precision 18,4`) and `Currency` (`maxlength 3`) columns, exactly like `ProductConfiguration`.
- Test naming: `Method_Scenario_ExpectedResult`.
- This plan assumes a single-currency store per basket/order (the existing `StoreSettings.DefaultCurrency` concept from `docs/ARCHITECTURE.md` §7) — a basket is not designed to hold items priced in different currencies. Note this as a known simplification, not a bug to fix here.

---

## File Structure

```
src/Vendix.Domain/
├── Basket/
│   ├── Entities/
│   │   ├── Basket.cs                      # NEW
│   │   └── BasketItem.cs                  # NEW
│   └── Repositories/
│       └── IBasketRepository.cs           # NEW
├── Ordering/
│   ├── Entities/
│   │   ├── Order.cs                       # NEW
│   │   └── OrderItem.cs                   # NEW
│   ├── Enums/
│   │   └── OrderStatus.cs                 # NEW
│   ├── ValueObjects/
│   │   └── OrderNumber.cs                 # NEW
│   └── Repositories/
│       └── IOrderRepository.cs            # NEW

src/Vendix.Infrastructure/
├── Persistence/
│   ├── Configurations/
│   │   ├── BasketConfiguration.cs         # NEW (Basket + BasketItem configs)
│   │   └── OrderConfiguration.cs          # NEW (Order + OrderItem configs)
│   ├── Repositories/
│   │   ├── BasketRepository.cs            # NEW
│   │   └── OrderRepository.cs             # NEW
│   ├── VendixDbContext.cs                 # MODIFY - add 4 DbSets
│   └── Migrations/                        # NEW migration via `dotnet ef migrations add`
└── DependencyInjection.cs                 # MODIFY - register 2 repositories

src/Vendix.Application/
├── Basket/
│   ├── DTOs/BasketDto.cs                  # NEW (BasketDto + BasketItemDto)
│   ├── Mappings/BasketMappingConfig.cs    # NEW
│   ├── Commands/
│   │   ├── AddToBasketCommand.cs          # NEW
│   │   ├── UpdateBasketItemQuantityCommand.cs  # NEW
│   │   ├── RemoveFromBasketCommand.cs     # NEW
│   │   └── ClearBasketCommand.cs          # NEW
│   └── Queries/GetBasketQuery.cs          # NEW
├── Ordering/
│   ├── DTOs/OrderDto.cs                   # NEW (OrderDto, OrderItemDto, OrderListDto, PlaceOrderResultDto)
│   ├── Mappings/OrderMappingConfig.cs     # NEW
│   ├── Commands/
│   │   ├── PlaceOrderCommand.cs           # NEW
│   │   ├── CancelOrderCommand.cs          # NEW
│   │   └── UpdateOrderStatusCommand.cs    # NEW
│   └── Queries/
│       ├── GetOrderByIdQuery.cs           # NEW
│       ├── GetMyOrdersQuery.cs            # NEW
│       └── GetOrdersQuery.cs              # NEW (admin, all buyers)

src/Vendix.Web/
├── Services/
│   ├── BuyerIdProvider.cs                 # NEW
│   └── CartService.cs                     # MODIFY - MediatR-backed instead of localStorage
├── wwwroot/js/storefront.js               # MODIFY - add vendix.buyer interop
├── Program.cs                             # MODIFY - register BuyerIdProvider
├── Components/Pages/
│   ├── Cart/Cart.razor                    # MODIFY - wire Checkout() to navigate
│   ├── Checkout/
│   │   ├── Checkout.razor                 # NEW
│   │   └── OrderConfirmation.razor        # NEW
│   └── Admin/Orders/
│       ├── Index.razor                    # NEW
│       └── Detail.razor                   # NEW

tests/Vendix.Domain.Tests/
├── Basket/BasketTests.cs                  # NEW
└── Ordering/
    ├── OrderTests.cs                      # NEW
    └── OrderNumberTests.cs                # NEW

tests/Vendix.Application.Tests/Ordering/
├── PlaceOrderCommandValidatorTests.cs     # NEW
└── PlaceOrderCommandHandlerTests.cs       # NEW

tests/Vendix.Integration.Tests/Persistence/
├── BasketRepositoryTests.cs               # NEW
└── OrderRepositoryTests.cs                # NEW

docs/
├── ARCHITECTURE.md                        # MODIFY - Phase 3 checklist
└── CHANGELOG.md                           # MODIFY - Phase 3 entries
```

---

### Task 1: Domain — Basket &amp; BasketItem

**Files:**
- Create: `src/Vendix.Domain/Basket/Entities/Basket.cs`
- Create: `src/Vendix.Domain/Basket/Entities/BasketItem.cs`
- Create: `src/Vendix.Domain/Basket/Repositories/IBasketRepository.cs`

**Interfaces:**
- Consumes: `AggregateRoot`, `BaseEntity` (`Vendix.Domain.Common`), `Money` (`Vendix.Domain.Catalog.ValueObjects`), `IRepository<T>`.
- Produces: `Basket(string buyerId)` ctor; `Basket.AddItem(Guid productId, string productName, string productSlug, string sku, Money unitPrice, int quantity, string? imageUrl)`; `Basket.SetItemQuantity(Guid productId, int quantity)`; `Basket.RemoveItem(Guid productId)`; `Basket.Clear()`; `Basket.Items` (`IReadOnlyCollection<BasketItem>`); `Basket.BuyerId`. `IBasketRepository.GetByBuyerIdAsync(string, CancellationToken)`. These exact names/signatures are relied on by Tasks 3-6.

- [ ] **Step 1: Write `BasketItem.cs`**

```csharp
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Basket.Entities;

/// <summary>
/// Represents a single product line within a <see cref="Basket"/>.
/// </summary>
/// <remarks>
/// Product details (name, slug, SKU, price, image) are snapshotted at the time the item is
/// added so the basket keeps displaying correctly even if the product is later renamed,
/// repriced, or removed. The snapshot is refreshed whenever the quantity is increased via
/// <see cref="Basket.AddItem"/> re-adding the same product.
/// </remarks>
public class BasketItem : BaseEntity
{
    /// <summary>
    /// Gets the ID of the owning basket.
    /// </summary>
    public Guid BasketId { get; private set; }

    /// <summary>
    /// Gets the ID of the product this line represents.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the product name snapshot.
    /// </summary>
    public string ProductName { get; private set; } = null!;

    /// <summary>
    /// Gets the product slug snapshot, used to link back to the product detail page.
    /// </summary>
    public string ProductSlug { get; private set; } = null!;

    /// <summary>
    /// Gets the product SKU snapshot.
    /// </summary>
    public string Sku { get; private set; } = null!;

    /// <summary>
    /// Gets the unit price snapshot.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    /// <summary>
    /// Gets the quantity of this product in the basket.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the product image URL snapshot, if any.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Gets the line total (unit price x quantity).
    /// </summary>
    public decimal LineTotal => UnitPrice.Amount * Quantity;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private BasketItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasketItem"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive.</exception>
    /// <exception cref="ArgumentNullException">Thrown when unitPrice is null.</exception>
    public BasketItem(
        Guid basketId,
        Guid productId,
        string productName,
        string productSlug,
        string sku,
        Money unitPrice,
        int quantity,
        string? imageUrl)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        BasketId = basketId;
        ProductId = productId;
        ProductName = productName;
        ProductSlug = productSlug;
        Sku = sku;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Quantity = quantity;
        ImageUrl = imageUrl;
    }

    /// <summary>
    /// Increases the quantity by the given positive amount.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when amount is not positive.</exception>
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        Quantity += amount;
    }

    /// <summary>
    /// Sets the quantity to an exact positive value.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive.</exception>
    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Quantity = quantity;
    }

    /// <summary>
    /// Refreshes the price/name/image snapshot from the current product state.
    /// </summary>
    public void RefreshSnapshot(string productName, string productSlug, Money unitPrice, string? imageUrl)
    {
        ProductName = productName;
        ProductSlug = productSlug;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        ImageUrl = imageUrl;
    }
}
```

- [ ] **Step 2: Write `Basket.cs`**

```csharp
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Basket.Entities;

/// <summary>
/// Represents a shopping basket owned by a single buyer (guest or, from Phase 5 onward,
/// an authenticated user).
/// </summary>
/// <remarks>
/// Basket is an aggregate root. One basket exists per <see cref="BuyerId"/> — see
/// <see cref="Repositories.IBasketRepository.GetByBuyerIdAsync"/>. A basket is not deleted
/// after checkout; it is emptied via <see cref="Clear"/> so the same buyer can keep shopping.
/// </remarks>
public class Basket : AggregateRoot
{
    private readonly List<BasketItem> _items = [];

    /// <summary>
    /// Gets the identifier of the buyer who owns this basket.
    /// </summary>
    public string BuyerId { get; private set; } = null!;

    /// <summary>
    /// Gets the line items in this basket.
    /// </summary>
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Basket() { }

    /// <summary>
    /// Initializes a new, empty basket for the given buyer.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when buyerId is null or whitespace.</exception>
    public Basket(string buyerId)
    {
        if (string.IsNullOrWhiteSpace(buyerId))
        {
            throw new ArgumentException("Buyer id is required.", nameof(buyerId));
        }

        BuyerId = buyerId;
    }

    /// <summary>
    /// Adds a product to the basket. If the product is already present, the quantities are
    /// summed and the snapshot (name/slug/price/image) is refreshed to the latest values.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive.</exception>
    public void AddItem(
        Guid productId,
        string productName,
        string productSlug,
        string sku,
        Money unitPrice,
        int quantity,
        string? imageUrl)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            existing.RefreshSnapshot(productName, productSlug, unitPrice, imageUrl);
            return;
        }

        _items.Add(new BasketItem(Id, productId, productName, productSlug, sku, unitPrice, quantity, imageUrl));
    }

    /// <summary>
    /// Sets the quantity of an existing item. A quantity of zero or less removes the item.
    /// Does nothing if the product is not in the basket.
    /// </summary>
    public void SetItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            _items.Remove(item);
            return;
        }

        item.SetQuantity(quantity);
    }

    /// <summary>
    /// Removes a product from the basket entirely. Does nothing if not present.
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        _items.RemoveAll(i => i.ProductId == productId);
    }

    /// <summary>
    /// Removes all items from the basket, leaving it empty (but not deleted).
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }
}
```

- [ ] **Step 3: Write `IBasketRepository.cs`**

```csharp
using Vendix.Domain.Basket.Entities;
using Vendix.Domain.Common;

namespace Vendix.Domain.Basket.Repositories;

/// <summary>
/// Repository interface for managing Basket aggregates.
/// </summary>
public interface IBasketRepository : IRepository<Entities.Basket>
{
    /// <summary>
    /// Gets the basket owned by the given buyer, or null if they don't have one yet.
    /// </summary>
    Task<Entities.Basket?> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default);
}
```

> Note: the entity is named `Basket` in the `Vendix.Domain.Basket.Entities` namespace, so `IRepository<T>` and the return type must be fully qualified as `Entities.Basket` inside `Vendix.Domain.Basket.Repositories` to avoid clashing with the enclosing `Basket` namespace segment — this mirrors no existing ambiguity in the codebase (Catalog has no type named the same as its own namespace segment), so double-check this compiles; if the compiler still complains, add `using BasketAggregate = Vendix.Domain.Basket.Entities.Basket;` and use the alias instead.

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Vendix.Domain`
Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/Vendix.Domain/Basket/
git commit -m "feat: add Basket and BasketItem domain entities"
```

---

### Task 2: Domain — Order, OrderItem, OrderNumber, OrderStatus

**Files:**
- Create: `src/Vendix.Domain/Ordering/Enums/OrderStatus.cs`
- Create: `src/Vendix.Domain/Ordering/ValueObjects/OrderNumber.cs`
- Create: `src/Vendix.Domain/Ordering/Entities/OrderItem.cs`
- Create: `src/Vendix.Domain/Ordering/Entities/Order.cs`
- Create: `src/Vendix.Domain/Ordering/Repositories/IOrderRepository.cs`

**Interfaces:**
- Consumes: `AggregateRoot`, `BaseEntity`, `IAuditableEntity`, `ValueObject`, `IRepository<T>` (`Vendix.Domain.Common`).
- Produces: `OrderNumber.Generate()`, `OrderNumber(string value)`, `OrderNumber.Value`, `OrderNumber.Length` (const). `OrderStatus` enum: `Pending, Processing, Shipped, Delivered, Cancelled`. `Order(string buyerId, string buyerEmail, string shippingAddress, string currency, decimal shippingCost)` ctor; `Order.AddItem(Guid productId, string productName, string sku, decimal unitPrice, int quantity, string? imageUrl)`; `Order.Cancel()`; `Order.UpdateStatus(OrderStatus status)`; `Order.Subtotal`/`Order.Total` (computed); `Order.Items`. `IOrderRepository.GetByBuyerIdAsync(string, CancellationToken)`, `IOrderRepository.SearchAsync(string? buyerId, OrderStatus? status, int pageNumber, int pageSize, CancellationToken)`. Relied on by Tasks 3-6.

- [ ] **Step 1: Write `OrderStatus.cs`**

```csharp
namespace Vendix.Domain.Ordering.Enums;

/// <summary>
/// Represents the lifecycle status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>The order was placed and is awaiting processing.</summary>
    Pending = 0,

    /// <summary>The order is being prepared/packed.</summary>
    Processing = 1,

    /// <summary>The order has been handed to a carrier.</summary>
    Shipped = 2,

    /// <summary>The order has been delivered to the buyer.</summary>
    Delivered = 3,

    /// <summary>The order was cancelled and will not be fulfilled.</summary>
    Cancelled = 4
}
```

- [ ] **Step 2: Write `OrderNumber.cs`**

```csharp
using System.Text.RegularExpressions;
using Vendix.Domain.Common;

namespace Vendix.Domain.Ordering.ValueObjects;

/// <summary>
/// Represents a human-readable, unique order number in the format "ORD-yyyyMMdd-XXXXXX".
/// </summary>
/// <remarks>
/// The random 6-character suffix (uppercase hex, from a GUID fragment) makes collisions
/// astronomically unlikely for a small/medium store; if this store grows to a volume where
/// that risk becomes real, replace <see cref="Generate"/> with a database sequence instead
/// of changing this value object's shape.
/// </remarks>
public sealed partial class OrderNumber : ValueObject
{
    /// <summary>
    /// The fixed length of a valid order number: "ORD-" (4) + 8 date digits + "-" (1) + 6 suffix chars = 19.
    /// </summary>
    public const int Length = 19;

    /// <summary>
    /// The regex pattern for validating order number format.
    /// </summary>
    public const string Pattern = @"^ORD-\d{8}-[A-F0-9]{6}$";

    /// <summary>
    /// Gets the order number value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderNumber"/> class from an existing value
    /// (used when reading from the database).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when value is null, empty, or invalid format.</exception>
    public OrderNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !OrderNumberPattern().IsMatch(value))
        {
            throw new ArgumentException("Invalid order number format.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Generates a new, unique order number based on the current UTC date.
    /// </summary>
    public static OrderNumber Generate()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return new OrderNumber($"ORD-{datePart}-{suffix}");
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the order number value as a string.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts an OrderNumber to its string value.
    /// </summary>
    public static implicit operator string(OrderNumber orderNumber) => orderNumber.Value;

    [GeneratedRegex(@"^ORD-\d{8}-[A-F0-9]{6}$", RegexOptions.Compiled)]
    private static partial Regex OrderNumberPattern();
}
```

- [ ] **Step 3: Write `OrderItem.cs`**

```csharp
using Vendix.Domain.Common;

namespace Vendix.Domain.Ordering.Entities;

/// <summary>
/// Represents a single product line within a placed <see cref="Order"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="Basket.Entities.BasketItem"/>, this snapshot is permanent — an order line
/// must never change after checkout, even if the product is later repriced or deleted.
/// </remarks>
public class OrderItem : BaseEntity
{
    /// <summary>Gets the ID of the owning order.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>Gets the ID of the product this line represents.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Gets the product name at the time of purchase.</summary>
    public string ProductName { get; private set; } = null!;

    /// <summary>Gets the product SKU at the time of purchase.</summary>
    public string Sku { get; private set; } = null!;

    /// <summary>Gets the unit price at the time of purchase.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Gets the purchased quantity.</summary>
    public int Quantity { get; private set; }

    /// <summary>Gets the product image URL at the time of purchase, if any.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Gets the line total (unit price x quantity).</summary>
    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private OrderItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderItem"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive or unitPrice is negative.</exception>
    public OrderItem(Guid orderId, Guid productId, string productName, string sku, decimal unitPrice, int quantity, string? imageUrl)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        UnitPrice = unitPrice;
        Quantity = quantity;
        ImageUrl = imageUrl;
    }
}
```

- [ ] **Step 4: Write `Order.cs`**

```csharp
using Vendix.Domain.Common;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.ValueObjects;

namespace Vendix.Domain.Ordering.Entities;

/// <summary>
/// Represents a placed customer order.
/// </summary>
/// <remarks>
/// Order is an aggregate root. It is created from a <see cref="Basket.Entities.Basket"/> at
/// checkout time (see the Application layer's PlaceOrderCommand) and is immutable except for
/// its <see cref="Status"/>.
/// </remarks>
public class Order : AggregateRoot, IAuditableEntity
{
    private readonly List<OrderItem> _items = [];

    /// <summary>Gets the human-readable order number.</summary>
    public OrderNumber OrderNumber { get; private set; } = null!;

    /// <summary>Gets the identifier of the buyer who placed this order.</summary>
    public string BuyerId { get; private set; } = null!;

    /// <summary>Gets the buyer's contact email for order updates.</summary>
    public string BuyerEmail { get; private set; } = null!;

    /// <summary>
    /// Gets the shipping address as free text. A structured <c>Address</c> value object
    /// (street/city/postal code/country) is deferred to Phase 7 (Inventory &amp; Shipping)
    /// per <c>docs/ARCHITECTURE.md</c> §4 — this is an intentional, documented simplification.
    /// </summary>
    public string ShippingAddress { get; private set; } = null!;

    /// <summary>Gets the current order status.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Gets the currency code for all monetary amounts on this order.</summary>
    public string Currency { get; private set; } = null!;

    /// <summary>Gets the shipping cost.</summary>
    public decimal ShippingCost { get; private set; }

    /// <summary>Gets the order's line items.</summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    /// <summary>Gets the sum of all line totals, excluding shipping.</summary>
    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    /// <summary>Gets the grand total including shipping.</summary>
    public decimal Total => Subtotal + ShippingCost;

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public string? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTime? ModifiedAt { get; set; }

    /// <inheritdoc />
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Order() { }

    /// <summary>
    /// Initializes a new order in <see cref="OrderStatus.Pending"/> status with a freshly
    /// generated order number.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when buyerId, buyerEmail, shippingAddress, or currency is null/whitespace,
    /// or when shippingCost is negative.
    /// </exception>
    public Order(string buyerId, string buyerEmail, string shippingAddress, string currency, decimal shippingCost)
    {
        if (string.IsNullOrWhiteSpace(buyerId))
        {
            throw new ArgumentException("Buyer id is required.", nameof(buyerId));
        }

        if (string.IsNullOrWhiteSpace(buyerEmail))
        {
            throw new ArgumentException("Buyer email is required.", nameof(buyerEmail));
        }

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            throw new ArgumentException("Shipping address is required.", nameof(shippingAddress));
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-character code.", nameof(currency));
        }

        if (shippingCost < 0)
        {
            throw new ArgumentException("Shipping cost cannot be negative.", nameof(shippingCost));
        }

        OrderNumber = OrderNumber.Generate();
        BuyerId = buyerId;
        BuyerEmail = buyerEmail;
        ShippingAddress = shippingAddress;
        Currency = currency.ToUpperInvariant();
        ShippingCost = shippingCost;
        Status = OrderStatus.Pending;
    }

    /// <summary>
    /// Adds a line item to the order. Should only be called while building the order at
    /// checkout time, before it is persisted.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when quantity is not positive or unitPrice is negative.</exception>
    public void AddItem(Guid productId, string productName, string sku, decimal unitPrice, int quantity, string? imageUrl)
    {
        _items.Add(new OrderItem(Id, productId, productName, sku, unitPrice, quantity, imageUrl));
    }

    /// <summary>
    /// Cancels the order.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the order is already cancelled, or has already shipped/been delivered.
    /// </exception>
    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Order is already cancelled.");
        }

        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
        {
            throw new InvalidOperationException($"Cannot cancel an order with status {Status}.");
        }

        Status = OrderStatus.Cancelled;
    }

    /// <summary>
    /// Updates the order status (admin action).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the order is already cancelled.</exception>
    public void UpdateStatus(OrderStatus status)
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot change the status of a cancelled order.");
        }

        Status = status;
    }
}
```

- [ ] **Step 5: Write `IOrderRepository.cs`**

```csharp
using Vendix.Domain.Common;
using Vendix.Domain.Ordering.Enums;

namespace Vendix.Domain.Ordering.Repositories;

/// <summary>
/// Repository interface for managing Order aggregates.
/// </summary>
public interface IOrderRepository : IRepository<Entities.Order>
{
    /// <summary>
    /// Gets all orders placed by the given buyer, most recent first.
    /// </summary>
    Task<IReadOnlyList<Entities.Order>> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches orders with optional buyer and status filters, paginated, most recent first.
    /// </summary>
    Task<(IReadOnlyList<Entities.Order> Items, int TotalCount)> SearchAsync(
        string? buyerId = null,
        OrderStatus? status = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
```

- [ ] **Step 6: Build to verify it compiles**

Run: `dotnet build src/Vendix.Domain`
Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add src/Vendix.Domain/Ordering/
git commit -m "feat: add Order, OrderItem, OrderNumber, and OrderStatus domain types"
```

---

### Task 3: Infrastructure — EF Core Configurations, DbContext, Migration

**Files:**
- Create: `src/Vendix.Infrastructure/Persistence/Configurations/BasketConfiguration.cs`
- Create: `src/Vendix.Infrastructure/Persistence/Configurations/OrderConfiguration.cs`
- Modify: `src/Vendix.Infrastructure/Persistence/VendixDbContext.cs`

**Interfaces:**
- Consumes: `Basket`, `BasketItem` (Task 1), `Order`, `OrderItem`, `OrderNumber` (Task 2).
- Produces: `VendixDbContext.Baskets`, `.BasketItems`, `.Orders`, `.OrderItems` (`DbSet<T>`), a new EF Core migration file.

- [ ] **Step 1: Write `BasketConfiguration.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Basket.Entities;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Basket"/> aggregate root.
/// </summary>
public class BasketConfiguration : IEntityTypeConfiguration<Basket>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Basket> builder)
    {
        builder.ToTable("Baskets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BuyerId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(b => b.BuyerId)
            .IsUnique();

        builder.HasMany(b => b.Items)
            .WithOne()
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// EF Core configuration for the <see cref="BasketItem"/> entity.
/// </summary>
public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BasketItem> builder)
    {
        builder.ToTable("BasketItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.ProductSlug).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Sku).HasMaxLength(50).IsRequired();
        builder.Property(i => i.ImageUrl).HasMaxLength(1000);
        builder.Property(i => i.Quantity).IsRequired();

        builder.OwnsOne(i => i.UnitPrice, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 4)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasIndex(i => new { i.BasketId, i.ProductId })
            .IsUnique();
    }
}
```

- [ ] **Step 2: Write `OrderConfiguration.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.ValueObjects;

namespace Vendix.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Order"/> aggregate root.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(OrderNumber.Length)
            .IsRequired()
            .HasConversion(
                number => number.Value,
                value => new OrderNumber(value));

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.BuyerId).HasMaxLength(100).IsRequired();
        builder.Property(o => o.BuyerEmail).HasMaxLength(256).IsRequired();
        builder.Property(o => o.ShippingAddress).HasMaxLength(500).IsRequired();
        builder.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        builder.Property(o => o.ShippingCost).HasPrecision(18, 4).IsRequired();
        builder.Property(o => o.Status).IsRequired();

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.CreatedBy).HasMaxLength(100);
        builder.Property(o => o.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(o => o.BuyerId);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// EF Core configuration for the <see cref="OrderItem"/> entity.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Sku).HasMaxLength(50).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(i => i.ImageUrl).HasMaxLength(1000);
        builder.Property(i => i.Quantity).IsRequired();
    }
}
```

- [ ] **Step 3: Add DbSets to `VendixDbContext.cs`**

Add inside the class, after the existing `#region Catalog Entities` block (new region, same file):

```csharp
    #region Basket Entities

    /// <summary>
    /// Gets or sets the baskets DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Basket.Entities.Basket> Baskets => Set<Vendix.Domain.Basket.Entities.Basket>();

    /// <summary>
    /// Gets or sets the basket items DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Basket.Entities.BasketItem> BasketItems => Set<Vendix.Domain.Basket.Entities.BasketItem>();

    #endregion

    #region Ordering Entities

    /// <summary>
    /// Gets or sets the orders DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Ordering.Entities.Order> Orders => Set<Vendix.Domain.Ordering.Entities.Order>();

    /// <summary>
    /// Gets or sets the order items DbSet.
    /// </summary>
    public DbSet<Vendix.Domain.Ordering.Entities.OrderItem> OrderItems => Set<Vendix.Domain.Ordering.Entities.OrderItem>();

    #endregion
```

Fully-qualified names are used here instead of new `using` statements because `Vendix.Domain.Basket.Entities.Basket` would otherwise require a `using Vendix.Domain.Basket.Entities;` that shadows nothing today but reads ambiguously next to `Vendix.Domain.Catalog.Entities`'s already-imported `Product`/`Category`/etc. — keep the explicit qualification for these two new DbSets only.

- [ ] **Step 4: Generate the EF Core migration**

Run: `dotnet ef migrations add AddBasketAndOrder -p src/Vendix.Infrastructure -s src/Vendix.Web`
Expected: A new migration file is generated under `src/Vendix.Infrastructure/Migrations/` creating `Baskets`, `BasketItems`, `Orders`, `OrderItems` tables with the indexes/constraints from Step 1-2.

- [ ] **Step 5: Apply the migration and verify**

Run: `dotnet ef database update -p src/Vendix.Infrastructure -s src/Vendix.Web`
Expected: Migration applies cleanly against your local PostgreSQL instance with no errors.

- [ ] **Step 6: Build to verify everything compiles**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 7: Commit**

```bash
git add src/Vendix.Infrastructure/Persistence/Configurations/BasketConfiguration.cs \
        src/Vendix.Infrastructure/Persistence/Configurations/OrderConfiguration.cs \
        src/Vendix.Infrastructure/Persistence/VendixDbContext.cs \
        src/Vendix.Infrastructure/Migrations/
git commit -m "feat: add EF Core configurations and migration for Basket and Order"
```

---

### Task 4: Infrastructure — Repositories &amp; DI Registration

**Files:**
- Create: `src/Vendix.Infrastructure/Persistence/Repositories/BasketRepository.cs`
- Create: `src/Vendix.Infrastructure/Persistence/Repositories/OrderRepository.cs`
- Modify: `src/Vendix.Infrastructure/DependencyInjection.cs`

**Interfaces:**
- Consumes: `IBasketRepository`, `IOrderRepository` (Tasks 1-2), `VendixDbContext.Baskets`/`Orders` (Task 3).
- Produces: `BasketRepository`, `OrderRepository` registered as `IBasketRepository`/`IOrderRepository` in DI — relied on by Task 5-6's handlers.

- [ ] **Step 1: Write `BasketRepository.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Basket.Entities;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing Basket aggregates.
/// </summary>
public class BasketRepository : IBasketRepository
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasketRepository"/> class.
    /// </summary>
    public BasketRepository(VendixDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Basket?> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        return await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.BuyerId == buyerId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Basket entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Baskets.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Basket entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var local = _context.Set<Basket>().Local.FirstOrDefault(b => b.Id == entity.Id);
        if (local is not null && !ReferenceEquals(local, entity))
        {
            _context.Entry(local).State = EntityState.Detached;
        }

        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void Delete(Basket entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Baskets.Remove(entity);
    }
}
```

- [ ] **Step 2: Write `OrderRepository.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing Order aggregates.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    public OrderRepository(VendixDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchAsync(
        string? buyerId = null,
        OrderStatus? status = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(buyerId))
        {
            query = query.Where(o => o.BuyerId == buyerId);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Orders.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Order entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var local = _context.Set<Order>().Local.FirstOrDefault(o => o.Id == entity.Id);
        if (local is not null && !ReferenceEquals(local, entity))
        {
            _context.Entry(local).State = EntityState.Detached;
        }

        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void Delete(Order entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Orders.Remove(entity);
    }
}
```

- [ ] **Step 3: Register both repositories in `DependencyInjection.cs`**

In `src/Vendix.Infrastructure/DependencyInjection.cs`, add two `using` statements at the top:

```csharp
using Vendix.Domain.Basket.Repositories;
using Vendix.Domain.Ordering.Repositories;
```

And add two lines right after the existing `services.AddScoped<IBrandRepository, BrandRepository>();`:

```csharp
        services.AddScoped<IBasketRepository, BasketRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Vendix.Infrastructure`
Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/Vendix.Infrastructure/Persistence/Repositories/BasketRepository.cs \
        src/Vendix.Infrastructure/Persistence/Repositories/OrderRepository.cs \
        src/Vendix.Infrastructure/DependencyInjection.cs
git commit -m "feat: add Basket and Order repository implementations"
```

---

### Task 5: Application — Basket DTOs, Mapping, Commands, Query

**Files:**
- Create: `src/Vendix.Application/Basket/DTOs/BasketDto.cs`
- Create: `src/Vendix.Application/Basket/Mappings/BasketMappingConfig.cs`
- Create: `src/Vendix.Application/Basket/Commands/AddToBasketCommand.cs`
- Create: `src/Vendix.Application/Basket/Commands/UpdateBasketItemQuantityCommand.cs`
- Create: `src/Vendix.Application/Basket/Commands/RemoveFromBasketCommand.cs`
- Create: `src/Vendix.Application/Basket/Commands/ClearBasketCommand.cs`
- Create: `src/Vendix.Application/Basket/Queries/GetBasketQuery.cs`

**Interfaces:**
- Consumes: `IBasketRepository` (Task 1/4), `IProductRepository` (existing Catalog), `IUnitOfWork`, `Result<T>`.
- Produces: `BasketDto { Guid Id; string BuyerId; List<BasketItemDto> Items; decimal Subtotal; string? Currency; int ItemCount; }`, `AddToBasketCommand(string BuyerId, Guid ProductId, int Quantity = 1) : IRequest<Result<BasketDto>>`, `UpdateBasketItemQuantityCommand(string BuyerId, Guid ProductId, int Quantity) : IRequest<Result<BasketDto>>`, `RemoveFromBasketCommand(string BuyerId, Guid ProductId) : IRequest<Result<BasketDto>>`, `ClearBasketCommand(string BuyerId) : IRequest<Result>`, `GetBasketQuery(string BuyerId) : IRequest<BasketDto>`. Task 8's `CartService` depends on these exact types/signatures.

- [ ] **Step 1: Write `BasketDto.cs`**

```csharp
namespace Vendix.Application.Basket.DTOs;

/// <summary>
/// DTO for a single basket line item.
/// </summary>
public sealed class BasketItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// DTO for a buyer's basket.
/// </summary>
public sealed class BasketDto
{
    public Guid Id { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public string? Currency { get; set; }
    public int ItemCount { get; set; }
}
```

- [ ] **Step 2: Write `BasketMappingConfig.cs`**

```csharp
using Mapster;
using Vendix.Application.Basket.DTOs;
using BasketEntity = Vendix.Domain.Basket.Entities.Basket;
using Vendix.Domain.Basket.Entities;

namespace Vendix.Application.Basket.Mappings;

public class BasketMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BasketItem, BasketItemDto>()
            .Map(dest => dest.UnitPrice, src => src.UnitPrice.Amount)
            .Map(dest => dest.Currency, src => src.UnitPrice.Currency)
            .Map(dest => dest.LineTotal, src => src.LineTotal);

        config.NewConfig<BasketEntity, BasketDto>()
            .Map(dest => dest.Items, src => src.Items)
            .Map(dest => dest.Subtotal, src => src.Items.Sum(i => i.LineTotal))
            .Map(dest => dest.Currency, src => src.Items.Select(i => i.UnitPrice.Currency).FirstOrDefault())
            .Map(dest => dest.ItemCount, src => src.Items.Sum(i => i.Quantity));
    }
}
```

- [ ] **Step 3: Write `AddToBasketCommand.cs`**

```csharp
using FluentValidation;
using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;
using Vendix.Domain.Catalog.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to add a product to a buyer's basket, or increase its quantity if already present.
/// </summary>
/// <remarks>
/// Product name/slug/SKU/price/image are always re-read from the authoritative
/// <see cref="Domain.Catalog.Entities.Product"/> on the server — the client never supplies
/// pricing, so a tampered client request can't put an arbitrary price into the basket.
/// </remarks>
public sealed record AddToBasketCommand(string BuyerId, Guid ProductId, int Quantity = 1) : IRequest<Result<BasketDto>>;

/// <summary>
/// Handler for <see cref="AddToBasketCommand"/>.
/// </summary>
public sealed class AddToBasketCommandHandler(
    IBasketRepository basketRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<AddToBasketCommand, Result<BasketDto>>
{
    /// <inheritdoc />
    public async Task<Result<BasketDto>> Handle(AddToBasketCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<BasketDto>.Failure("Product not found.");
        }

        var imageUrl = product.Images.FirstOrDefault(i => i.IsMain)?.Url
            ?? product.Images.FirstOrDefault()?.Url;

        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            basket = new Domain.Basket.Entities.Basket(request.BuyerId);
            basket.AddItem(product.Id, product.Name, product.Slug.Value, product.Sku.Value, product.Price, request.Quantity, imageUrl);
            await basketRepository.AddAsync(basket, cancellationToken);
        }
        else
        {
            basket.AddItem(product.Id, product.Name, product.Slug.Value, product.Sku.Value, product.Price, request.Quantity, imageUrl);
            basketRepository.Update(basket);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BasketDto>.Success(mapper.Map<BasketDto>(basket));
    }
}

/// <summary>
/// Validator for <see cref="AddToBasketCommand"/>.
/// </summary>
public sealed class AddToBasketCommandValidator : AbstractValidator<AddToBasketCommand>
{
    public AddToBasketCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
```

- [ ] **Step 4: Write `UpdateBasketItemQuantityCommand.cs`**

```csharp
using FluentValidation;
using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to set the exact quantity of a basket item. A quantity of zero removes the item.
/// </summary>
public sealed record UpdateBasketItemQuantityCommand(string BuyerId, Guid ProductId, int Quantity) : IRequest<Result<BasketDto>>;

/// <summary>
/// Handler for <see cref="UpdateBasketItemQuantityCommand"/>.
/// </summary>
public sealed class UpdateBasketItemQuantityCommandHandler(
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateBasketItemQuantityCommand, Result<BasketDto>>
{
    /// <inheritdoc />
    public async Task<Result<BasketDto>> Handle(UpdateBasketItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            return Result<BasketDto>.Failure("Basket not found.");
        }

        basket.SetItemQuantity(request.ProductId, request.Quantity);
        basketRepository.Update(basket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BasketDto>.Success(mapper.Map<BasketDto>(basket));
    }
}

/// <summary>
/// Validator for <see cref="UpdateBasketItemQuantityCommand"/>.
/// </summary>
public sealed class UpdateBasketItemQuantityCommandValidator : AbstractValidator<UpdateBasketItemQuantityCommand>
{
    public UpdateBasketItemQuantityCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
    }
}
```

- [ ] **Step 5: Write `RemoveFromBasketCommand.cs`**

```csharp
using FluentValidation;
using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to remove a product from a buyer's basket entirely.
/// </summary>
public sealed record RemoveFromBasketCommand(string BuyerId, Guid ProductId) : IRequest<Result<BasketDto>>;

/// <summary>
/// Handler for <see cref="RemoveFromBasketCommand"/>.
/// </summary>
public sealed class RemoveFromBasketCommandHandler(
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<RemoveFromBasketCommand, Result<BasketDto>>
{
    /// <inheritdoc />
    public async Task<Result<BasketDto>> Handle(RemoveFromBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            return Result<BasketDto>.Failure("Basket not found.");
        }

        basket.RemoveItem(request.ProductId);
        basketRepository.Update(basket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BasketDto>.Success(mapper.Map<BasketDto>(basket));
    }
}

/// <summary>
/// Validator for <see cref="RemoveFromBasketCommand"/>.
/// </summary>
public sealed class RemoveFromBasketCommandValidator : AbstractValidator<RemoveFromBasketCommand>
{
    public RemoveFromBasketCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
    }
}
```

- [ ] **Step 6: Write `ClearBasketCommand.cs`**

```csharp
using FluentValidation;
using MediatR;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Commands;

/// <summary>
/// Command to empty a buyer's basket.
/// </summary>
public sealed record ClearBasketCommand(string BuyerId) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="ClearBasketCommand"/>.
/// </summary>
public sealed class ClearBasketCommandHandler(
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClearBasketCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null)
        {
            return Result.Success();
        }

        basket.Clear();
        basketRepository.Update(basket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Validator for <see cref="ClearBasketCommand"/>.
/// </summary>
public sealed class ClearBasketCommandValidator : AbstractValidator<ClearBasketCommand>
{
    public ClearBasketCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");
    }
}
```

- [ ] **Step 7: Write `GetBasketQuery.cs`**

```csharp
using MapsterMapper;
using MediatR;
using Vendix.Application.Basket.DTOs;
using Vendix.Domain.Basket.Repositories;

namespace Vendix.Application.Basket.Queries;

/// <summary>
/// Query to get a buyer's basket. Returns an empty basket DTO (not an error) if the buyer
/// doesn't have one yet — a fresh visitor with no basket is a normal state, not a failure.
/// </summary>
public sealed record GetBasketQuery(string BuyerId) : IRequest<BasketDto>;

/// <summary>
/// Handler for <see cref="GetBasketQuery"/>.
/// </summary>
public sealed class GetBasketQueryHandler(
    IBasketRepository basketRepository,
    IMapper mapper) : IRequestHandler<GetBasketQuery, BasketDto>
{
    /// <inheritdoc />
    public async Task<BasketDto> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        return basket is null
            ? new BasketDto { BuyerId = request.BuyerId }
            : mapper.Map<BasketDto>(basket);
    }
}
```

- [ ] **Step 8: Build to verify it compiles**

Run: `dotnet build src/Vendix.Application`
Expected: 0 errors.

- [ ] **Step 9: Commit**

```bash
git add src/Vendix.Application/Basket/
git commit -m "feat: add Basket application layer (commands, query, DTOs, mapping)"
```

---

### Task 6: Application — Order DTOs, Mapping, Commands, Queries

**Files:**
- Create: `src/Vendix.Application/Ordering/DTOs/OrderDto.cs`
- Create: `src/Vendix.Application/Ordering/Mappings/OrderMappingConfig.cs`
- Create: `src/Vendix.Application/Ordering/Commands/PlaceOrderCommand.cs`
- Create: `src/Vendix.Application/Ordering/Commands/CancelOrderCommand.cs`
- Create: `src/Vendix.Application/Ordering/Commands/UpdateOrderStatusCommand.cs`
- Create: `src/Vendix.Application/Ordering/Queries/GetOrderByIdQuery.cs`
- Create: `src/Vendix.Application/Ordering/Queries/GetMyOrdersQuery.cs`
- Create: `src/Vendix.Application/Ordering/Queries/GetOrdersQuery.cs`

**Interfaces:**
- Consumes: `IOrderRepository`, `IBasketRepository` (for `PlaceOrderCommand`), `IUnitOfWork`, `Result<T>`, `NotFoundException`, `PaginatedList<T>`.
- Produces: `PlaceOrderResultDto(Guid OrderId, string OrderNumber, decimal Total, string Currency)`, `OrderDto`, `OrderListDto`, `PlaceOrderCommand(string BuyerId, string BuyerEmail, string ShippingAddress, decimal ShippingCost = 0m) : IRequest<Result<PlaceOrderResultDto>>`, `GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDto>>`. Web Tasks 9-11 depend on these exact types.

- [ ] **Step 1: Write `OrderDto.cs`**

```csharp
namespace Vendix.Application.Ordering.DTOs;

/// <summary>
/// DTO for a single order line item.
/// </summary>
public sealed class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// DTO for a full order (detail view).
/// </summary>
public sealed class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

/// <summary>
/// Lightweight DTO for order lists (admin index, "my orders").
/// </summary>
public sealed class OrderListDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Result returned immediately after successfully placing an order.
/// </summary>
public sealed record PlaceOrderResultDto(Guid OrderId, string OrderNumber, decimal Total, string Currency);
```

- [ ] **Step 2: Write `OrderMappingConfig.cs`**

```csharp
using Mapster;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Entities;

namespace Vendix.Application.Ordering.Mappings;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderItem, OrderItemDto>();

        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.OrderNumber, src => src.OrderNumber.Value)
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<Order, OrderListDto>()
            .Map(dest => dest.OrderNumber, src => src.OrderNumber.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ItemCount, src => src.Items.Sum(i => i.Quantity));
    }
}
```

- [ ] **Step 3: Write `PlaceOrderCommand.cs`**

```csharp
using FluentValidation;
using MediatR;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Basket.Repositories;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Commands;

/// <summary>
/// Command to place an order from the buyer's current basket. The basket is emptied
/// (not deleted) on success so the buyer can keep shopping with the same basket row.
/// </summary>
public sealed record PlaceOrderCommand(
    string BuyerId,
    string BuyerEmail,
    string ShippingAddress,
    decimal ShippingCost = 0m) : IRequest<Result<PlaceOrderResultDto>>;

/// <summary>
/// Handler for <see cref="PlaceOrderCommand"/>.
/// </summary>
public sealed class PlaceOrderCommandHandler(
    IBasketRepository basketRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResultDto>>
{
    /// <inheritdoc />
    public async Task<Result<PlaceOrderResultDto>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket is null || basket.Items.Count == 0)
        {
            return Result<PlaceOrderResultDto>.Failure("Your basket is empty.");
        }

        var currency = basket.Items.First().UnitPrice.Currency;

        var order = new Order(request.BuyerId, request.BuyerEmail, request.ShippingAddress, currency, request.ShippingCost);
        foreach (var item in basket.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Sku, item.UnitPrice.Amount, item.Quantity, item.ImageUrl);
        }

        await orderRepository.AddAsync(order, cancellationToken);

        basket.Clear();
        basketRepository.Update(basket);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PlaceOrderResultDto>.Success(
            new PlaceOrderResultDto(order.Id, order.OrderNumber.Value, order.Total, order.Currency));
    }
}

/// <summary>
/// Validator for <see cref="PlaceOrderCommand"/>.
/// </summary>
public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer id is required.");

        RuleFor(x => x.BuyerEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters.");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping cost cannot be negative.");
    }
}
```

- [ ] **Step 4: Write `CancelOrderCommand.cs`**

```csharp
using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Commands;

/// <summary>
/// Command to cancel an order (buyer or admin action).
/// </summary>
public sealed record CancelOrderCommand(Guid OrderId) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="CancelOrderCommand"/>.
/// </summary>
public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw NotFoundException.ForEntity<Order>(request.OrderId);
        }

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 5: Write `UpdateOrderStatusCommand.cs`**

```csharp
using FluentValidation;
using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Commands;

/// <summary>
/// Command to update an order's status (admin action).
/// </summary>
public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<Result>;

/// <summary>
/// Handler for <see cref="UpdateOrderStatusCommand"/>.
/// </summary>
public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw NotFoundException.ForEntity<Order>(request.OrderId);
        }

        try
        {
            order.UpdateStatus(request.Status);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Validator for <see cref="UpdateOrderStatusCommand"/>.
/// </summary>
public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order id is required.");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Invalid order status.");
    }
}
```

- [ ] **Step 6: Write `GetOrderByIdQuery.cs`**

```csharp
using MapsterMapper;
using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Models;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Queries;

/// <summary>
/// Query to get a single order by its ID.
/// </summary>
public sealed record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDto>>;

/// <summary>
/// Handler for <see cref="GetOrderByIdQuery"/>.
/// </summary>
public sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    /// <inheritdoc />
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            throw NotFoundException.ForEntity<Order>(request.Id);
        }

        return Result<OrderDto>.Success(mapper.Map<OrderDto>(order));
    }
}
```

- [ ] **Step 7: Write `GetMyOrdersQuery.cs`**

```csharp
using MapsterMapper;
using MediatR;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Queries;

/// <summary>
/// Query to get all orders placed by a specific buyer, most recent first.
/// </summary>
public sealed record GetMyOrdersQuery(string BuyerId) : IRequest<List<OrderListDto>>;

/// <summary>
/// Handler for <see cref="GetMyOrdersQuery"/>.
/// </summary>
public sealed class GetMyOrdersQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetMyOrdersQuery, List<OrderListDto>>
{
    /// <inheritdoc />
    public async Task<List<OrderListDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        return orders.Select(mapper.Map<OrderListDto>).ToList();
    }
}
```

- [ ] **Step 8: Write `GetOrdersQuery.cs`**

```csharp
using MapsterMapper;
using MediatR;
using Vendix.Application.Common.Models;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Enums;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Ordering.Queries;

/// <summary>
/// Query to search/paginate all orders across all buyers (admin index page).
/// </summary>
public sealed record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    OrderStatus? Status = null) : IRequest<PaginatedList<OrderListDto>>;

/// <summary>
/// Handler for <see cref="GetOrdersQuery"/>.
/// </summary>
public sealed class GetOrdersQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetOrdersQuery, PaginatedList<OrderListDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<OrderListDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await orderRepository.SearchAsync(
            status: request.Status,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var dtos = orders.Select(mapper.Map<OrderListDto>).ToList();

        return new PaginatedList<OrderListDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
```

- [ ] **Step 9: Build to verify it compiles**

Run: `dotnet build src/Vendix.Application`
Expected: 0 errors.

- [ ] **Step 10: Commit**

```bash
git add src/Vendix.Application/Ordering/
git commit -m "feat: add Order application layer (commands, queries, DTOs, mapping)"
```

---

### Task 7: Web — BuyerIdProvider (Anonymous Buyer Identity)

**Files:**
- Create: `src/Vendix.Web/Services/BuyerIdProvider.cs`
- Modify: `src/Vendix.Web/wwwroot/js/storefront.js`
- Modify: `src/Vendix.Web/Program.cs`

**Interfaces:**
- Produces: `BuyerIdProvider.GetOrCreateAsync() : Task<string>` — Task 8's `CartService` and Task 9's Checkout page depend on this exact method.

- [ ] **Step 1: Add localStorage interop to `storefront.js`**

Append to the existing `window.vendix` object (do not remove the existing `vendix.cart` block):

```javascript
window.vendix.buyer = {
    load: function () {
        try {
            return window.localStorage.getItem('vendix.buyerId') || '';
        } catch (e) {
            return '';
        }
    },
    save: function (id) {
        try {
            window.localStorage.setItem('vendix.buyerId', id);
        } catch (e) {
        }
    }
};
```

- [ ] **Step 2: Write `BuyerIdProvider.cs`**

```csharp
using Microsoft.JSInterop;

namespace Vendix.Web.Services;

/// <summary>
/// Resolves a stable anonymous buyer identifier for the current browser, persisted in
/// localStorage. Used to associate a guest's basket and orders across visits until a full
/// authentication system (Phase 5, currently 0% per docs/ARCHITECTURE.md) replaces it with
/// the logged-in user's ID.
/// </summary>
public class BuyerIdProvider
{
    private const string JsNamespace = "vendix.buyer";

    private readonly IJSRuntime _js;
    private string? _buyerId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuyerIdProvider"/> class.
    /// </summary>
    public BuyerIdProvider(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Gets the current buyer ID, creating and persisting a new one on first call.
    /// Cached in-memory for the lifetime of this scoped service (one Blazor circuit).
    /// </summary>
    public async Task<string> GetOrCreateAsync()
    {
        if (_buyerId is not null)
        {
            return _buyerId;
        }

        var existing = await _js.InvokeAsync<string>($"{JsNamespace}.load");
        if (!string.IsNullOrWhiteSpace(existing))
        {
            _buyerId = existing;
            return _buyerId;
        }

        _buyerId = Guid.NewGuid().ToString("N");
        await _js.InvokeVoidAsync($"{JsNamespace}.save", _buyerId);
        return _buyerId;
    }
}
```

- [ ] **Step 3: Register in `Program.cs`**

Add next to the existing `builder.Services.AddScoped<CartService>();` line:

```csharp
builder.Services.AddScoped<BuyerIdProvider>();
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Vendix.Web`
Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/Vendix.Web/Services/BuyerIdProvider.cs src/Vendix.Web/wwwroot/js/storefront.js src/Vendix.Web/Program.cs
git commit -m "feat: add anonymous BuyerIdProvider for guest basket/order ownership"
```

---

### Task 8: Web — Rewrite CartService to Use the Server-Side Basket

**Files:**
- Modify: `src/Vendix.Web/Services/CartService.cs`

**Interfaces:**
- Consumes: `IMediator` (already registered via `AddApplication()`), `BuyerIdProvider` (Task 7), `AddToBasketCommand`/`UpdateBasketItemQuantityCommand`/`RemoveFromBasketCommand`/`ClearBasketCommand`/`GetBasketQuery`/`BasketDto` (Task 5).
- Produces: Same public surface as before — `Items`, `ItemCount`, `Subtotal`, `Currency`, `Changed` event, `InitializeAsync()`, `AddAsync(CartItem)`, `SetQuantityAsync(Guid, int)`, `RemoveAsync(Guid)`, `ClearAsync()`, `GetQuantity(Guid)` — **plus one new method** `GetBuyerIdAsync() : Task<string>` that Task 9's Checkout page needs. `CartItem.cs` is NOT modified; `Cart.razor`, `MainLayout.razor` need NO changes for this task (they only call the public surface, which is unchanged).

- [ ] **Step 1: Replace `CartService.cs` entirely**

```csharp
using MediatR;
using Vendix.Application.Basket.Commands;
using Vendix.Application.Basket.DTOs;
using Vendix.Application.Basket.Queries;

namespace Vendix.Web.Services;

/// <summary>
/// Client-facing cart service backed by the server-side Basket aggregate (see
/// <c>Vendix.Application.Basket</c>). Keeps the same public surface the UI already depends on
/// (Items, ItemCount, Subtotal, Currency, Changed) while persisting through MediatR
/// commands/queries instead of localStorage, so a basket now survives across devices/tabs for
/// the same buyer ID (see <see cref="BuyerIdProvider"/>).
/// </summary>
public class CartService
{
    private readonly IMediator _mediator;
    private readonly BuyerIdProvider _buyerIdProvider;
    private readonly ILogger<CartService> _logger;
    private List<CartItem> _items = [];
    private bool _initialized;

    /// <summary>
    /// Occurs whenever the cart contents change (add, update, remove, clear, load).
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CartService"/> class.
    /// </summary>
    public CartService(IMediator mediator, BuyerIdProvider buyerIdProvider, ILogger<CartService> logger)
    {
        _mediator = mediator;
        _buyerIdProvider = buyerIdProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current cart items.
    /// </summary>
    public IReadOnlyList<CartItem> Items => _items;

    /// <summary>
    /// Gets the total number of items (sum of quantities).
    /// </summary>
    public int ItemCount => _items.Sum(i => i.Quantity);

    /// <summary>
    /// Gets the cart subtotal (sum of line totals).
    /// </summary>
    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    /// <summary>
    /// Gets the currency of the first item, if any.
    /// </summary>
    public string? Currency => _items.FirstOrDefault()?.Currency;

    /// <summary>
    /// Loads the basket from the server. Safe to call multiple times.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            var buyerId = await _buyerIdProvider.GetOrCreateAsync();
            var basket = await _mediator.Send(new GetBasketQuery(buyerId));
            _items = MapToCartItems(basket);
            Changed?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load basket.");
        }
    }

    /// <summary>
    /// Adds a product to the basket, merging quantities if it's already present. Only
    /// <see cref="CartItem.ProductId"/> and <see cref="CartItem.Quantity"/> are sent to the
    /// server — price/name/etc. are re-resolved server-side from the authoritative product.
    /// </summary>
    public async Task AddAsync(CartItem item)
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        var result = await _mediator.Send(new AddToBasketCommand(buyerId, item.ProductId, item.Quantity));

        if (result.IsSuccess)
        {
            _items = MapToCartItems(result.Value);
            Changed?.Invoke();
        }
        else
        {
            _logger.LogWarning("Could not add product {ProductId} to basket: {Error}", item.ProductId, result.Error);
        }
    }

    /// <summary>
    /// Sets the quantity of an item. A quantity of zero or less removes the item.
    /// </summary>
    public async Task SetQuantityAsync(Guid productId, int quantity)
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        var result = await _mediator.Send(new UpdateBasketItemQuantityCommand(buyerId, productId, quantity));

        if (result.IsSuccess)
        {
            _items = MapToCartItems(result.Value);
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public async Task RemoveAsync(Guid productId)
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        var result = await _mediator.Send(new RemoveFromBasketCommand(buyerId, productId));

        if (result.IsSuccess)
        {
            _items = MapToCartItems(result.Value);
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Removes all items from the cart.
    /// </summary>
    public async Task ClearAsync()
    {
        var buyerId = await _buyerIdProvider.GetOrCreateAsync();
        await _mediator.Send(new ClearBasketCommand(buyerId));
        _items = [];
        Changed?.Invoke();
    }

    /// <summary>
    /// Gets the current quantity of a product in the cart.
    /// </summary>
    public int GetQuantity(Guid productId)
        => _items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;

    /// <summary>
    /// Gets the current buyer ID. Used by the Checkout page to place an order against the
    /// same basket this service manages.
    /// </summary>
    public Task<string> GetBuyerIdAsync() => _buyerIdProvider.GetOrCreateAsync();

    private static List<CartItem> MapToCartItems(BasketDto basket) =>
        basket.Items.Select(i => new CartItem
        {
            ProductId = i.ProductId,
            Slug = i.ProductSlug,
            Name = i.ProductName,
            Sku = i.Sku,
            Price = i.UnitPrice,
            Currency = i.Currency,
            ImageUrl = i.ImageUrl,
            Quantity = i.Quantity
        }).ToList();
}
```

- [ ] **Step 2: Build to verify it compiles**

Run: `dotnet build src/Vendix.Web`
Expected: 0 errors. `Cart.razor`, `MainLayout.razor`, `ProductDetail.razor`, `ProductCard.razor` should all compile unchanged since `CartItem` and `CartService`'s public method signatures are untouched.

- [ ] **Step 3: Manual smoke test**

Run: `dotnet run --project src/Vendix.Web`, open the storefront, add a product to the cart from a product card and from the product detail page, open `/cart`, change quantity, remove an item, refresh the page — the basket should still be there (now server-persisted, not just localStorage).

- [ ] **Step 4: Commit**

```bash
git add src/Vendix.Web/Services/CartService.cs
git commit -m "feat: back CartService with the server-side Basket aggregate via MediatR"
```

---

### Task 9: Web — Checkout Page

**Files:**
- Create: `src/Vendix.Web/Components/Pages/Checkout/Checkout.razor`
- Modify: `src/Vendix.Web/Components/Pages/Cart/Cart.razor`

**Interfaces:**
- Consumes: `CartService` (Task 8, including new `GetBuyerIdAsync()`), `PlaceOrderCommand`/`PlaceOrderResultDto` (Task 6), `IMediator`.

- [ ] **Step 1: Wire up `Cart.razor`'s Checkout button**

In `src/Vendix.Web/Components/Pages/Cart/Cart.razor`, add `@inject NavigationManager Navigation` near the other `@inject` lines, and replace the existing `Checkout()` method:

```csharp
    private void Checkout()
    {
        Navigation.NavigateTo("/checkout");
    }
```

(This replaces the old body that just showed a "Coming Soon" toast — remove the `await Task.CompletedTask;` and `ToastService.ShowInfo(...)` lines, and drop `async`/`Task` since the method is now synchronous.)

- [ ] **Step 2: Write `Checkout.razor`**

```razor
@page "/checkout"

@*
    Checkout Page - Collects buyer email and shipping address, then places the order
    from the current basket.
*@

<PageTitle>Checkout - Vendix</PageTitle>

@implements IDisposable

@inject CartService CartService
@inject IMediator Mediator
@inject NavigationManager Navigation
@inject ToastService ToastService

@if (_isLoading)
{
    <div class="flex justify-center py-24">
        <LoadingSpinner Size="large" />
    </div>
}
else
{
    <div class="bg-gray-50 min-h-screen">
        <div class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <h1 class="text-2xl font-bold text-gray-900 mb-6">Checkout</h1>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
                <div class="lg:col-span-2">
                    <EditForm Model="_model" OnValidSubmit="PlaceOrder" class="bg-white rounded-lg shadow-sm p-6 space-y-4">
                        <DataAnnotationsValidator />

                        <div>
                            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
                            <InputText @bind-Value="_model.Email" class="w-full rounded-lg border-gray-300" placeholder="you@example.com" />
                            <ValidationMessage For="@(() => _model.Email)" class="text-sm text-red-600" />
                        </div>

                        <div>
                            <label class="block text-sm font-medium text-gray-700 mb-1">Shipping Address</label>
                            <InputTextArea @bind-Value="_model.ShippingAddress" rows="4" class="w-full rounded-lg border-gray-300" placeholder="Street, city, postal code, country" />
                            <ValidationMessage For="@(() => _model.ShippingAddress)" class="text-sm text-red-600" />
                        </div>

                        <button type="submit" class="w-full bg-primary-600 text-white py-3 rounded-lg font-medium hover:bg-primary-700 transition disabled:opacity-50" disabled="@_isSubmitting">
                            @(_isSubmitting ? "Placing order..." : "Place Order")
                        </button>
                    </EditForm>
                </div>

                <div class="lg:col-span-1">
                    <div class="bg-white rounded-lg shadow-sm p-6 sticky top-20">
                        <h2 class="text-lg font-bold text-gray-900 mb-4">Order Summary</h2>
                        <dl class="space-y-3 text-sm">
                            <div class="flex justify-between">
                                <dt class="text-gray-600">Subtotal (@CartService.ItemCount @(CartService.ItemCount == 1 ? "item" : "items"))</dt>
                                <dd class="font-medium text-gray-900"><PriceDisplay Price="@CartService.Subtotal" Currency="@(CartService.Currency ?? "USD")" /></dd>
                            </div>
                            <div class="border-t pt-3 flex justify-between items-center">
                                <dt class="text-base font-bold text-gray-900">Total</dt>
                                <dd class="text-xl font-bold text-primary-600">
                                    <PriceDisplay Price="@CartService.Subtotal" Currency="@(CartService.Currency ?? "USD")" />
                                </dd>
                            </div>
                        </dl>
                    </div>
                </div>
            </div>
        </div>
    </div>
}

@code {
    private bool _isLoading = true;
    private bool _isSubmitting;
    private readonly CheckoutFormModel _model = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CartService.InitializeAsync();
            if (CartService.Items.Count == 0)
            {
                Navigation.NavigateTo("/cart");
                return;
            }

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task PlaceOrder()
    {
        _isSubmitting = true;
        try
        {
            var buyerId = await CartService.GetBuyerIdAsync();
            var result = await Mediator.Send(new PlaceOrderCommand(buyerId, _model.Email, _model.ShippingAddress));

            if (result.IsSuccess)
            {
                await CartService.ClearAsync();
                Navigation.NavigateTo($"/checkout/confirmation/{result.Value.OrderId}");
            }
            else
            {
                ToastService.ShowError(result.Error ?? "Could not place order.");
            }
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    public void Dispose()
    {
    }

    private sealed class CheckoutFormModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email is required.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "A valid email address is required.")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Shipping address is required.")]
        [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "Shipping address must not exceed 500 characters.")]
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
```

> Check `ToastService` for the exact method name used for error toasts (`ShowInfo` is confirmed in `Cart.razor`; if `ShowError` doesn't exist, run `grep -n "public.*Show" src/Vendix.Web/Services/ToastService.cs` and use whichever method matches an "error" style, or `ShowInfo` if that's the only one).

- [ ] **Step 3: Build to verify it compiles**

Run: `dotnet build src/Vendix.Web`
Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/Vendix.Web/Components/Pages/Checkout/Checkout.razor src/Vendix.Web/Components/Pages/Cart/Cart.razor
git commit -m "feat: add Checkout page and wire up the Cart page's checkout button"
```

---

### Task 10: Web — Order Confirmation Page

**Files:**
- Create: `src/Vendix.Web/Components/Pages/Checkout/OrderConfirmation.razor`

**Interfaces:**
- Consumes: `GetOrderByIdQuery`/`OrderDto` (Task 6), `IMediator`.

- [ ] **Step 1: Write `OrderConfirmation.razor`**

```razor
@page "/checkout/confirmation/{OrderId:guid}"

<PageTitle>Order Confirmed - Vendix</PageTitle>

@inject IMediator Mediator

@if (_isLoading)
{
    <div class="flex justify-center py-24">
        <LoadingSpinner Size="large" />
    </div>
}
else if (_order is null)
{
    <div class="max-w-2xl mx-auto px-4 py-24 text-center">
        <h1 class="text-2xl font-bold text-gray-900">Order not found</h1>
        <a href="/" class="mt-6 inline-block bg-primary-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-primary-700 transition">
            Back to Home
        </a>
    </div>
}
else
{
    <div class="bg-gray-50 min-h-screen">
        <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
            <div class="bg-white rounded-lg shadow-sm p-8 text-center">
                <div class="inline-flex items-center justify-center h-16 w-16 bg-success/10 rounded-full mb-4">
                    <svg class="h-8 w-8 text-success" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                    </svg>
                </div>
                <h1 class="text-2xl font-bold text-gray-900">Thank you for your order!</h1>
                <p class="mt-2 text-gray-500">Order <span class="font-medium text-gray-900">@_order.OrderNumber</span></p>
            </div>

            <div class="bg-white rounded-lg shadow-sm p-6 mt-6">
                <h2 class="text-lg font-bold text-gray-900 mb-4">Order Details</h2>
                <div class="space-y-3">
                    @foreach (var item in _order.Items)
                    {
                        <div class="flex justify-between text-sm">
                            <span class="text-gray-700">@item.ProductName x @item.Quantity</span>
                            <span class="font-medium text-gray-900"><PriceDisplay Price="@item.LineTotal" Currency="@_order.Currency" /></span>
                        </div>
                    }
                </div>
                <div class="border-t mt-4 pt-4 space-y-2 text-sm">
                    <div class="flex justify-between">
                        <span class="text-gray-600">Subtotal</span>
                        <span class="font-medium text-gray-900"><PriceDisplay Price="@_order.Subtotal" Currency="@_order.Currency" /></span>
                    </div>
                    <div class="flex justify-between">
                        <span class="text-gray-600">Shipping</span>
                        <span class="font-medium text-gray-900"><PriceDisplay Price="@_order.ShippingCost" Currency="@_order.Currency" /></span>
                    </div>
                    <div class="flex justify-between text-base font-bold">
                        <span class="text-gray-900">Total</span>
                        <span class="text-primary-600"><PriceDisplay Price="@_order.Total" Currency="@_order.Currency" /></span>
                    </div>
                </div>
                <p class="mt-4 text-sm text-gray-500">Shipping to: @_order.ShippingAddress</p>
            </div>

            <div class="text-center mt-6">
                <a href="/products" class="text-primary-600 hover:text-primary-700 font-medium transition">Continue Shopping</a>
            </div>
        </div>
    </div>
}

@code {
    [Parameter]
    public Guid OrderId { get; set; }

    private bool _isLoading = true;
    private Vendix.Application.Ordering.DTOs.OrderDto? _order;

    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Send(new Vendix.Application.Ordering.Queries.GetOrderByIdQuery(OrderId));
        _order = result.IsSuccess ? result.Value : null;
        _isLoading = false;
    }
}
```

- [ ] **Step 2: Build to verify it compiles**

Run: `dotnet build src/Vendix.Web`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add src/Vendix.Web/Components/Pages/Checkout/OrderConfirmation.razor
git commit -m "feat: add order confirmation page"
```

---

### Task 11: Web — Admin Orders Index &amp; Detail Pages

**Files:**
- Create: `src/Vendix.Web/Components/Pages/Admin/Orders/Index.razor`
- Create: `src/Vendix.Web/Components/Pages/Admin/Orders/Detail.razor`

**Interfaces:**
- Consumes: `GetOrdersQuery`, `GetOrderByIdQuery`, `UpdateOrderStatusCommand`, `CancelOrderCommand` (Task 6), `Pagination`, `ConfirmDialog`, `LoadingSpinner` (existing shared components).

> This task fills in the `/admin/orders` route that `AdminLayout.razor`'s sidebar `NavLink` has been pointing to since Phase 1/2 with no page behind it.

- [ ] **Step 1: Write `Index.razor`**

```razor
@page "/admin/orders"
@layout AdminLayout

<PageTitle>Orders - Vendix Admin</PageTitle>

@inject IMediator Mediator

<div class="p-6">
    <div class="flex items-center justify-between mb-6">
        <h1 class="text-2xl font-bold text-gray-900">Orders</h1>
        <select @bind="_statusFilter" @bind:after="LoadAsync" class="rounded-lg border-gray-300 text-sm">
            <option value="">All Statuses</option>
            @foreach (var status in Enum.GetValues<Vendix.Domain.Ordering.Enums.OrderStatus>())
            {
                <option value="@status">@status</option>
            }
        </select>
    </div>

    @if (_isLoading)
    {
        <LoadingSpinner Size="large" />
    }
    else if (_orders is null || _orders.Items.Count == 0)
    {
        <div class="bg-white rounded-lg shadow-sm p-12 text-center text-gray-500">
            No orders found.
        </div>
    }
    else
    {
        <div class="bg-white rounded-lg shadow-sm overflow-hidden">
            <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                    <tr>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order #</th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Buyer</th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                        <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                    @foreach (var order in _orders.Items)
                    {
                        <tr>
                            <td class="px-6 py-4 text-sm font-medium text-gray-900">@order.OrderNumber</td>
                            <td class="px-6 py-4 text-sm text-gray-700">@order.BuyerEmail</td>
                            <td class="px-6 py-4 text-sm">
                                <span class="px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">@order.Status</span>
                            </td>
                            <td class="px-6 py-4 text-sm text-gray-900"><PriceDisplay Price="@order.Total" Currency="@order.Currency" /></td>
                            <td class="px-6 py-4 text-sm text-gray-500">@order.CreatedAt.ToString("yyyy-MM-dd")</td>
                            <td class="px-6 py-4 text-sm text-right">
                                <a href="/admin/orders/@order.Id" class="text-primary-600 hover:text-primary-700 font-medium">View</a>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>

        <div class="mt-4">
            <Pagination CurrentPage="_pageNumber"
                        TotalPages="_orders.TotalPages"
                        TotalItems="_orders.TotalCount"
                        PageSize="_pageSize"
                        OnPageChanged="OnPageChanged" />
        </div>
    }
</div>

@code {
    private Vendix.Application.Common.Models.PaginatedList<Vendix.Application.Ordering.DTOs.OrderListDto>? _orders;
    private bool _isLoading = true;
    private int _pageNumber = 1;
    private const int _pageSize = 10;
    private string _statusFilter = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        Vendix.Domain.Ordering.Enums.OrderStatus? status = string.IsNullOrEmpty(_statusFilter)
            ? null
            : Enum.Parse<Vendix.Domain.Ordering.Enums.OrderStatus>(_statusFilter);

        _orders = await Mediator.Send(new Vendix.Application.Ordering.Queries.GetOrdersQuery(_pageNumber, _pageSize, status));
        _isLoading = false;
    }

    private async Task OnPageChanged(int page)
    {
        _pageNumber = page;
        await LoadAsync();
    }
}
```

> Check the existing `Pagination.razor` component's actual parameter names (`CurrentPage`/`TotalPages`/`TotalItems`/`PageSize`/`OnPageChanged` are a best guess based on `docs/CHANGELOG.md`'s description of its features) — run `grep -n "\[Parameter\]" -A1 src/Vendix.Web/Components/Shared/Pagination.razor` and adjust the attribute names above to match exactly before this compiles.
>
> Check `PaginatedList<T>`'s actual property name for total page count (`TotalPages` is used here per `src/Vendix.Application/Common/Models/PaginatedList.cs`'s documented `TotalPages` property — already confirmed correct from the file read during planning).

- [ ] **Step 2: Write `Detail.razor`**

```razor
@page "/admin/orders/{Id:guid}"
@layout AdminLayout

<PageTitle>Order Details - Vendix Admin</PageTitle>

@inject IMediator Mediator
@inject ToastService ToastService

<div class="p-6 max-w-4xl">
    @if (_isLoading)
    {
        <LoadingSpinner Size="large" />
    }
    else if (_order is null)
    {
        <div class="bg-white rounded-lg shadow-sm p-12 text-center text-gray-500">Order not found.</div>
    }
    else
    {
        <div class="flex items-center justify-between mb-6">
            <h1 class="text-2xl font-bold text-gray-900">Order @_order.OrderNumber</h1>
            <a href="/admin/orders" class="text-primary-600 hover:text-primary-700 font-medium">Back to Orders</a>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6 mb-6">
            <h2 class="font-bold text-gray-900 mb-3">Buyer</h2>
            <p class="text-sm text-gray-700">Email: @_order.BuyerEmail</p>
            <p class="text-sm text-gray-700">Shipping Address: @_order.ShippingAddress</p>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6 mb-6">
            <h2 class="font-bold text-gray-900 mb-3">Items</h2>
            <div class="space-y-2">
                @foreach (var item in _order.Items)
                {
                    <div class="flex justify-between text-sm">
                        <span>@item.ProductName (@item.Sku) x @item.Quantity</span>
                        <span class="font-medium"><PriceDisplay Price="@item.LineTotal" Currency="@_order.Currency" /></span>
                    </div>
                }
            </div>
            <div class="border-t mt-4 pt-4 flex justify-between font-bold">
                <span>Total</span>
                <span class="text-primary-600"><PriceDisplay Price="@_order.Total" Currency="@_order.Currency" /></span>
            </div>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6 flex items-center gap-4">
            <label class="text-sm font-medium text-gray-700">Status:</label>
            <select @bind="_selectedStatus" class="rounded-lg border-gray-300 text-sm">
                @foreach (var status in Enum.GetValues<Vendix.Domain.Ordering.Enums.OrderStatus>())
                {
                    <option value="@status">@status</option>
                }
            </select>
            <button @onclick="UpdateStatus" class="bg-primary-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-primary-700 transition">
                Update Status
            </button>
            @if (_order.Status != nameof(Vendix.Domain.Ordering.Enums.OrderStatus.Cancelled))
            {
                <button @onclick="() => _confirmCancelVisible = true" class="text-red-600 hover:text-red-700 text-sm font-medium ml-auto">
                    Cancel Order
                </button>
            }
        </div>
    }
</div>

<ConfirmDialog IsVisible="@_confirmCancelVisible"
               IsVisibleChanged="@((bool value) => _confirmCancelVisible = value)"
               Title="Cancel Order"
               Message="Are you sure you want to cancel this order?"
               Type="danger"
               ConfirmText="Cancel Order"
               OnConfirm="CancelOrder" />

@code {
    [Parameter]
    public Guid Id { get; set; }

    private Vendix.Application.Ordering.DTOs.OrderDto? _order;
    private bool _isLoading = true;
    private bool _confirmCancelVisible;
    private Vendix.Domain.Ordering.Enums.OrderStatus _selectedStatus;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        var result = await Mediator.Send(new Vendix.Application.Ordering.Queries.GetOrderByIdQuery(Id));
        _order = result.IsSuccess ? result.Value : null;
        if (_order is not null)
        {
            _selectedStatus = Enum.Parse<Vendix.Domain.Ordering.Enums.OrderStatus>(_order.Status);
        }
        _isLoading = false;
    }

    private async Task UpdateStatus()
    {
        var result = await Mediator.Send(new Vendix.Application.Ordering.Commands.UpdateOrderStatusCommand(Id, _selectedStatus));
        if (result.IsSuccess)
        {
            ToastService.ShowInfo("Order status updated.");
            await LoadAsync();
        }
        else
        {
            ToastService.ShowInfo(result.Error ?? "Could not update status.", "Error");
        }
    }

    private async Task CancelOrder()
    {
        var result = await Mediator.Send(new Vendix.Application.Ordering.Commands.CancelOrderCommand(Id));
        if (result.IsSuccess)
        {
            ToastService.ShowInfo("Order cancelled.");
            await LoadAsync();
        }
        else
        {
            ToastService.ShowInfo(result.Error ?? "Could not cancel order.", "Error");
        }
    }
}
```

> `ToastService.ShowInfo` is confirmed to exist (used in `Cart.razor`); if a two-argument overload `(message, title)` doesn't exist, check `src/Vendix.Web/Services/ToastService.cs` and adjust the error-path calls to whatever error/warning method it actually exposes.

- [ ] **Step 3: Build to verify it compiles**

Run: `dotnet build src/Vendix.Web`
Expected: 0 errors. Fix any `Pagination`/`ToastService` parameter-name mismatches found in Steps 1-2's call-outs.

- [ ] **Step 4: Manual smoke test**

Run: `dotnet run --project src/Vendix.Web`, place a test order through `/checkout`, then visit `/admin/orders`, confirm it appears, open its detail page, change its status, then cancel a different pending order.

- [ ] **Step 5: Commit**

```bash
git add src/Vendix.Web/Components/Pages/Admin/Orders/
git commit -m "feat: add Admin Orders index and detail pages"
```

---

### Task 12: Domain Tests — Basket, Order, OrderNumber

**Files:**
- Create: `tests/Vendix.Domain.Tests/Basket/BasketTests.cs`
- Create: `tests/Vendix.Domain.Tests/Ordering/OrderTests.cs`
- Create: `tests/Vendix.Domain.Tests/Ordering/OrderNumberTests.cs`

**Interfaces:**
- Consumes: `Basket`, `BasketItem` (Task 1), `Order`, `OrderItem`, `OrderNumber`, `OrderStatus` (Task 2).

- [ ] **Step 1: Write `BasketTests.cs`**

```csharp
using Vendix.Domain.Basket.Entities;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Domain.Tests.Basket;

public class BasketTests
{
    private static Money DefaultPrice() => new(10m, "USD");

    [Fact]
    public void Constructor_ValidBuyerId_CreatesEmptyBasket()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");

        basket.BuyerId.Should().Be("buyer-1");
        basket.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNullBuyerId_ThrowsArgumentException(string? buyerId)
    {
        var act = () => new Vendix.Domain.Basket.Entities.Basket(buyerId!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_NewProduct_AddsItemWithGivenQuantity()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();

        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 2, null);

        basket.Items.Should().ContainSingle();
        basket.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_ExistingProduct_IncreasesQuantityInstesadOfDuplicating()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();

        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 2, null);
        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 3, null);

        basket.Items.Should().ContainSingle();
        basket.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_ZeroOrNegativeQuantity_ThrowsArgumentException()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");

        var act = () => basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", DefaultPrice(), 0, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetItemQuantity_ZeroQuantity_RemovesItem()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();
        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 2, null);

        basket.SetItemQuantity(productId, 0);

        basket.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesIt()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        var productId = Guid.NewGuid();
        basket.AddItem(productId, "Widget", "widget", "SKU-1", DefaultPrice(), 1, null);

        basket.RemoveItem(productId);

        basket.Items.Should().BeEmpty();
    }

    [Fact]
    public void Clear_WithItems_EmptiesBasket()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", DefaultPrice(), 1, null);
        basket.AddItem(Guid.NewGuid(), "Gadget", "gadget", "SKU-2", DefaultPrice(), 1, null);

        basket.Clear();

        basket.Items.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Write `OrderNumberTests.cs`**

```csharp
using Vendix.Domain.Ordering.ValueObjects;

namespace Vendix.Domain.Tests.Ordering;

public class OrderNumberTests
{
    [Fact]
    public void Generate_ReturnsValueMatchingPattern()
    {
        var orderNumber = OrderNumber.Generate();

        orderNumber.Value.Should().MatchRegex(OrderNumber.Pattern);
    }

    [Fact]
    public void Generate_CalledTwice_ReturnsDifferentValues()
    {
        var first = OrderNumber.Generate();
        var second = OrderNumber.Generate();

        first.Value.Should().NotBe(second.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-order-number")]
    [InlineData("ORD-2026-ABCDEF")]
    public void Constructor_InvalidFormat_ThrowsArgumentException(string value)
    {
        var act = () => new OrderNumber(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ValidFormat_RoundTripsValue()
    {
        var generated = OrderNumber.Generate();

        var reconstructed = new OrderNumber(generated.Value);

        reconstructed.Should().Be(generated);
    }
}
```

- [ ] **Step 3: Write `OrderTests.cs`**

```csharp
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;

namespace Vendix.Domain.Tests.Ordering;

public class OrderTests
{
    private static Order CreateValidOrder() =>
        new("buyer-1", "buyer@example.com", "123 Main St", "USD", 5m);

    [Fact]
    public void Constructor_ValidInputs_CreatesPendingOrderWithGeneratedNumber()
    {
        var order = CreateValidOrder();

        order.Status.Should().Be(OrderStatus.Pending);
        order.OrderNumber.Should().NotBeNull();
        order.Currency.Should().Be("USD");
        order.ShippingCost.Should().Be(5m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_EmptyBuyerEmail_ThrowsArgumentException(string? email)
    {
        var act = () => new Order("buyer-1", email!, "123 Main St", "USD", 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NegativeShippingCost_ThrowsArgumentException()
    {
        var act = () => new Order("buyer-1", "buyer@example.com", "123 Main St", "USD", -1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_ValidItem_IncludedInSubtotalAndTotal()
    {
        var order = CreateValidOrder();

        order.AddItem(Guid.NewGuid(), "Widget", "SKU-1", 10m, 2, null);

        order.Subtotal.Should().Be(20m);
        order.Total.Should().Be(25m); // 20 subtotal + 5 shipping
    }

    [Fact]
    public void Cancel_PendingOrder_SetsCancelledStatus()
    {
        var order = CreateValidOrder();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var order = CreateValidOrder();
        order.Cancel();

        var act = order.Cancel;

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    public void Cancel_ShippedOrDelivered_ThrowsInvalidOperationException(OrderStatus status)
    {
        var order = CreateValidOrder();
        order.UpdateStatus(status);

        var act = order.Cancel;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateStatus_CancelledOrder_ThrowsInvalidOperationException()
    {
        var order = CreateValidOrder();
        order.Cancel();

        var act = () => order.UpdateStatus(OrderStatus.Processing);

        act.Should().Throw<InvalidOperationException>();
    }
}
```

- [ ] **Step 4: Run all new domain tests**

Run: `dotnet test tests/Vendix.Domain.Tests --filter "FullyQualifiedName~Basket|FullyQualifiedName~Ordering"`
Expected: All PASS.

- [ ] **Step 5: Commit**

```bash
git add tests/Vendix.Domain.Tests/Basket/ tests/Vendix.Domain.Tests/Ordering/
git commit -m "test: add domain tests for Basket, Order, and OrderNumber"
```

---

### Task 13: Application Tests — PlaceOrderCommand

**Files:**
- Create: `tests/Vendix.Application.Tests/Ordering/PlaceOrderCommandValidatorTests.cs`
- Create: `tests/Vendix.Application.Tests/Ordering/PlaceOrderCommandHandlerTests.cs`

**Interfaces:**
- Consumes: `PlaceOrderCommand`, `PlaceOrderCommandValidator`, `PlaceOrderCommandHandler` (Task 6), `IBasketRepository`, `IOrderRepository`, `IUnitOfWork` (mocked with NSubstitute, matching `tests/Vendix.Application.Tests/Common/Behaviors/ValidationBehaviorTests.cs`'s existing style).

- [ ] **Step 1: Write `PlaceOrderCommandValidatorTests.cs`**

```csharp
using Vendix.Application.Ordering.Commands;

namespace Vendix.Application.Tests.Ordering;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St", 5m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_Fails(string email)
    {
        var command = new PlaceOrderCommand("buyer-1", email, "123 Main St", 0m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyShippingAddress_Fails()
    {
        var command = new PlaceOrderCommand("buyer-1", "buyer@example.com", "", 0m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NegativeShippingCost_Fails()
    {
        var command = new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St", -1m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Write `PlaceOrderCommandHandlerTests.cs`**

```csharp
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Ordering.Commands;
using Vendix.Domain.Basket.Repositories;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Repositories;

namespace Vendix.Application.Tests.Ordering;

public class PlaceOrderCommandHandlerTests
{
    private readonly IBasketRepository _basketRepository = Substitute.For<IBasketRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private PlaceOrderCommandHandler CreateHandler() =>
        new(_basketRepository, _orderRepository, _unitOfWork);

    [Fact]
    public async Task Handle_NoBasketForBuyer_ReturnsFailure()
    {
        _basketRepository.GetByBuyerIdAsync("buyer-1", Arg.Any<CancellationToken>())
            .Returns((Vendix.Domain.Basket.Entities.Basket?)null);

        var result = await CreateHandler().Handle(
            new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_EmptyBasket_ReturnsFailure()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        _basketRepository.GetByBuyerIdAsync("buyer-1", Arg.Any<CancellationToken>()).Returns(basket);

        var result = await CreateHandler().Handle(
            new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_BasketWithItems_CreatesOrderAndClearsBasket()
    {
        var basket = new Vendix.Domain.Basket.Entities.Basket("buyer-1");
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", new Money(15m, "USD"), 2, null);
        _basketRepository.GetByBuyerIdAsync("buyer-1", Arg.Any<CancellationToken>()).Returns(basket);

        Order? capturedOrder = null;
        await _orderRepository.AddAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        var result = await CreateHandler().Handle(
            new PlaceOrderCommand("buyer-1", "buyer@example.com", "123 Main St", 5m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(35m); // (15 * 2) + 5 shipping
        basket.Items.Should().BeEmpty(); // cleared after placing the order
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Items.Should().ContainSingle(i => i.ProductName == "Widget" && i.Quantity == 2);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 3: Run the new application tests**

Run: `dotnet test tests/Vendix.Application.Tests --filter "FullyQualifiedName~Ordering"`
Expected: All PASS.

- [ ] **Step 4: Commit**

```bash
git add tests/Vendix.Application.Tests/Ordering/
git commit -m "test: add PlaceOrderCommand validator and handler tests"
```

---

### Task 14: Integration Tests — Basket &amp; Order Repositories

**Files:**
- Create: `tests/Vendix.Integration.Tests/Persistence/BasketRepositoryTests.cs`
- Create: `tests/Vendix.Integration.Tests/Persistence/OrderRepositoryTests.cs`

**Interfaces:**
- Consumes: `DatabaseFixture`/`DatabaseCollection` from `docs/superpowers/plans/2026-09-01-phase2-task12-catalog-integration-tests.md` Task 1. **If that plan hasn't been executed yet, copy `tests/Vendix.Integration.Tests/Persistence/DatabaseFixture.cs` from that plan's Task 1 code block verbatim before starting this task** — its migration-apply step will pick up this plan's `AddBasketAndOrder` migration automatically since it runs `context.Database.MigrateAsync()` against whatever migrations exist at execution time.

- [ ] **Step 1: Write `BasketRepositoryTests.cs`**

```csharp
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

[Collection(nameof(DatabaseCollection))]
public class BasketRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public BasketRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ThenGetByBuyerId_ReturnsBasketWithItems()
    {
        var buyerId = $"buyer-{Guid.NewGuid():N}";

        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BasketRepository(writeContext);
        var basket = new Vendix.Domain.Basket.Entities.Basket(buyerId);
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", new Money(9.99m, "USD"), 3, null);

        await writeRepository.AddAsync(basket);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BasketRepository(readContext);
        var found = await readRepository.GetByBuyerIdAsync(buyerId);

        found.Should().NotBeNull();
        found!.Items.Should().ContainSingle(i => i.Quantity == 3 && i.UnitPrice.Amount == 9.99m);
    }

    [Fact]
    public async Task Update_AfterClear_PersistsEmptyBasket()
    {
        var buyerId = $"buyer-{Guid.NewGuid():N}";

        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BasketRepository(writeContext);
        var basket = new Vendix.Domain.Basket.Entities.Basket(buyerId);
        basket.AddItem(Guid.NewGuid(), "Widget", "widget", "SKU-1", new Money(5m, "USD"), 1, null);
        await writeRepository.AddAsync(basket);
        await writeContext.SaveChangesAsync();

        await using var clearContext = _fixture.CreateContext();
        var clearRepository = new BasketRepository(clearContext);
        var loaded = await clearRepository.GetByBuyerIdAsync(buyerId);
        loaded!.Clear();
        clearRepository.Update(loaded);
        await clearContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BasketRepository(readContext);
        var found = await readRepository.GetByBuyerIdAsync(buyerId);

        found!.Items.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Write `OrderRepositoryTests.cs`**

```csharp
using Vendix.Domain.Ordering.Entities;
using Vendix.Domain.Ordering.Enums;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

[Collection(nameof(DatabaseCollection))]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsOrderWithItemsAndGeneratedNumber()
    {
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new OrderRepository(writeContext);
        var order = new Order($"buyer-{Guid.NewGuid():N}", "buyer@example.com", "123 Main St", "USD", 5m);
        order.AddItem(Guid.NewGuid(), "Widget", "SKU-1", 10m, 2, null);

        await writeRepository.AddAsync(order);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new OrderRepository(readContext);
        var found = await readRepository.GetByIdAsync(order.Id);

        found.Should().NotBeNull();
        found!.OrderNumber.Value.Should().Be(order.OrderNumber.Value);
        found.Items.Should().ContainSingle(i => i.Quantity == 2);
        found.Total.Should().Be(25m);
    }

    [Fact]
    public async Task SearchAsync_FilterByStatus_ReturnsOnlyMatchingOrders()
    {
        var buyerId = $"buyer-{Guid.NewGuid():N}";

        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new OrderRepository(writeContext);

        var pending = new Order(buyerId, "buyer@example.com", "123 Main St", "USD", 0m);
        pending.AddItem(Guid.NewGuid(), "Widget", "SKU-1", 10m, 1, null);
        await writeRepository.AddAsync(pending);

        var cancelled = new Order(buyerId, "buyer@example.com", "123 Main St", "USD", 0m);
        cancelled.AddItem(Guid.NewGuid(), "Gadget", "SKU-2", 10m, 1, null);
        cancelled.Cancel();
        await writeRepository.AddAsync(cancelled);

        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new OrderRepository(readContext);
        var (items, totalCount) = await readRepository.SearchAsync(buyerId: buyerId, status: OrderStatus.Cancelled);

        items.Should().ContainSingle(o => o.Id == cancelled.Id);
        items.Should().NotContain(o => o.Id == pending.Id);
        totalCount.Should().Be(1);
    }
}
```

- [ ] **Step 3: Run the new integration tests**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~BasketRepositoryTests|FullyQualifiedName~OrderRepositoryTests"`
Expected: All PASS (requires Docker running and the `AddBasketAndOrder` migration from Task 3 present).

- [ ] **Step 4: Commit**

```bash
git add tests/Vendix.Integration.Tests/Persistence/BasketRepositoryTests.cs tests/Vendix.Integration.Tests/Persistence/OrderRepositoryTests.cs
git commit -m "test: add Basket and Order repository integration tests"
```

---

### Task 15: Documentation — Update Phase 3 Status

**Files:**
- Modify: `docs/ARCHITECTURE.md`
- Modify: `docs/CHANGELOG.md`

- [ ] **Step 1: Update `docs/ARCHITECTURE.md`**

Change the status line near the top to reflect the current phase:

```markdown
> **Status:** Phase 3 - Shopping Flow (Basket + Checkout + Order complete; Basket/Order admin cancel+status update done)
```

In §14, change the Phase 3 checklist:

```markdown
### Phase 3: Shopping Flow
- [x] Basket
- [x] Checkout
- [x] Orders
```

- [ ] **Step 2: Add a CHANGELOG.md entry**

Insert under `## [Unreleased]`, above the most recent existing entry:

```markdown
### Phase 3: Basket, Checkout & Order (Date: <today's date>)

**Added:**

Domain (src/Vendix.Domain/):
- `Basket/Entities/Basket.cs`, `BasketItem.cs` - Basket aggregate keyed by anonymous BuyerId, snapshotting product name/slug/SKU/price/image per line
- `Basket/Repositories/IBasketRepository.cs`
- `Ordering/Entities/Order.cs`, `OrderItem.cs` - Order aggregate with Pending/Processing/Shipped/Delivered/Cancelled lifecycle
- `Ordering/ValueObjects/OrderNumber.cs` - Human-readable "ORD-yyyyMMdd-XXXXXX" order numbers
- `Ordering/Enums/OrderStatus.cs`
- `Ordering/Repositories/IOrderRepository.cs`

Infrastructure:
- EF Core configurations, migration `AddBasketAndOrder`, `BasketRepository`, `OrderRepository`

Application:
- Basket commands (Add/UpdateQuantity/Remove/Clear) + `GetBasketQuery`
- Order commands (`PlaceOrder`/`Cancel`/`UpdateStatus`) + queries (`GetOrderById`/`GetMyOrders`/`GetOrders`)

Web:
- `BuyerIdProvider` - anonymous buyer identity via localStorage (guest baskets/orders ahead of Phase 5 auth)
- `CartService` rewritten to persist through the server-side Basket instead of localStorage
- `/checkout` and `/checkout/confirmation/{orderId}` pages
- `/admin/orders` (list + filter) and `/admin/orders/{id}` (detail + status update + cancel) — fills in the sidebar link that has pointed nowhere since Phase 1/2

**Technical Decisions:**
- No authentication yet (Phase 5), so Basket/Order are keyed by an anonymous BuyerId persisted client-side, matching the pattern the previous localStorage-only cart already used for cart data.
- Product price/name/image are re-resolved server-side from the authoritative Product on every `AddToBasketCommand` — the client never supplies pricing.
- A basket is emptied (not deleted) after checkout so returning buyers reuse the same row.
- Shipping address is a free-text string; a structured `Address` value object is deferred to Phase 7 per the existing architecture doc.

**Notes:**
- TODO: Payment integration (Phase 4) — orders are currently placed with no payment step.
- TODO: "My Orders" buyer-facing page using the already-built `GetMyOrdersQuery` (not wired to a page in this phase).
- TODO: Replace the anonymous-GUID order-number collision risk with a DB sequence if order volume grows.

---
```

- [ ] **Step 3: Commit**

```bash
git add docs/ARCHITECTURE.md docs/CHANGELOG.md
git commit -m "docs: update Phase 3 status - Basket, Checkout, and Order complete"
```

---

## Self-Review Notes

- **Spec coverage:** `docs/ARCHITECTURE.md` §14 Phase 3 items (Basket → Tasks 1,3-5,7-8,12,14; Checkout → Tasks 6,9,13; Orders → Tasks 2,6,10-11,12-14) are all covered. Admin visibility for the pre-existing dangling `/admin/orders` nav link is covered by Task 11.
- **Placeholder scan:** Every task has runnable code. The few spots where an exact existing signature couldn't be confirmed during planning (`Product.UpdatePrice` name in the sibling Phase 2 plan doesn't apply here; in this plan: `Pagination.razor`'s exact parameter names in Task 11, `ToastService`'s error-display method name in Tasks 9 and 11) are called out explicitly with the `grep` command to resolve them before proceeding — not left vague.
- **Type consistency:** `BuyerIdProvider.GetOrCreateAsync()` (Task 7) is the one method every later Web task calls the same way. `BasketDto`/`BasketItemDto` (Task 5) field names (`ProductSlug`, `UnitPrice`, `Currency`, `LineTotal`) match what `CartService.MapToCartItems` (Task 8) reads. `PlaceOrderResultDto(Guid OrderId, string OrderNumber, decimal Total, string Currency)` (Task 6) matches exactly how `Checkout.razor` (Task 9) consumes `result.Value.OrderId`. `OrderDto.Status` is a `string` (via `.ToString()` in the Mapster config) everywhere it's read (Tasks 10-11), never treated as the enum directly except where explicitly re-parsed with `Enum.Parse` (Task 11's `Detail.razor`).
