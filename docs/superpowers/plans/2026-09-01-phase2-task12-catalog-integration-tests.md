# Phase 2 Task 12: Catalog Integration Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close out Phase 2 (Core Catalog) by adding real-database integration tests for Product, Category, and Brand repositories, and mark Phase 2 as 100% complete in the project docs.

**Architecture:** Use Testcontainers to spin up a disposable PostgreSQL 16 container per test run, apply EF Core migrations against it, and exercise the real `ProductRepository`, `CategoryRepository`, and `BrandRepository` implementations directly (no mocks) to catch mapping/query issues that unit tests can't see (owned-entity conversions, unique indexes, query filters, split queries).

**Tech Stack:** xUnit, FluentAssertions, Testcontainers.PostgreSql (already referenced in `Vendix.Integration.Tests.csproj`), EF Core 10 / Npgsql, `Vendix.Infrastructure` (already referenced by the test project).

**Spec:** `docs/ARCHITECTURE.md` §14 (Phase 2 checklist, item 12: "Unit & Integration Tests"), `docs/CHANGELOG.md` (Phase 2 history).

## Global Constraints

- .NET 10 / C# 14, file-scoped namespaces, XML doc comments on public members (match existing test style — existing test files in this repo don't XML-doc every test method, only the class; follow that convention).
- Test naming: `Method_Scenario_ExpectedResult` (per `docs/ARCHITECTURE.md` §13).
- Every integration test must create its own `VendixDbContext` per logical step (arrange vs. act) to avoid change-tracker bleed, exactly like production code never assumes a long-lived context — see `ProductRepository` which is always constructed per-context.
- Docker must be running locally for these tests to execute (Testcontainers requirement). This is a pre-existing constraint of the test project (already true for `LocalFileStorageTests` if it uses containers — it doesn't, so this plan introduces the first Docker-dependent tests).
- Do not modify any production (`src/`) code in this plan — this is test-only work.

---

## File Structure

```
tests/Vendix.Integration.Tests/
├── Persistence/
│   ├── DatabaseFixture.cs          # NEW - Testcontainers PostgreSQL fixture + xUnit collection
│   ├── ProductRepositoryTests.cs   # NEW
│   ├── CategoryRepositoryTests.cs  # NEW
│   └── BrandRepositoryTests.cs     # NEW
docs/
├── ARCHITECTURE.md                 # MODIFY - Phase 2 checklist item 12 → ✅, status line → 12/12
└── CHANGELOG.md                    # MODIFY - new "Phase 2 - Task 12" entry
```

---

### Task 1: Database Fixture (Testcontainers PostgreSQL)

**Files:**
- Create: `tests/Vendix.Integration.Tests/Persistence/DatabaseFixture.cs`
- Test: `tests/Vendix.Integration.Tests/Persistence/DatabaseFixture.cs` (the smoke test lives in the same file, as a nested-free simple class, matching how small the file is)

**Interfaces:**
- Produces: `DatabaseFixture` with `public VendixDbContext CreateContext()`, implementing `IAsyncLifetime` (`InitializeAsync`/`DisposeAsync`) so xUnit starts/stops the container once per collection.
- Produces: `[CollectionDefinition(nameof(DatabaseCollection))] class DatabaseCollection : ICollectionFixture<DatabaseFixture>` — every later task's test classes consume this via `[Collection(nameof(DatabaseCollection))]` and a constructor parameter `DatabaseFixture fixture`.

- [ ] **Step 1: Write the fixture**

```csharp
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Vendix.Infrastructure.Persistence;

namespace Vendix.Integration.Tests.Persistence;

/// <summary>
/// Spins up a disposable PostgreSQL 16 container for the lifetime of the test collection and
/// exposes freshly-configured <see cref="VendixDbContext"/> instances against it. Migrations are
/// applied once during <see cref="InitializeAsync"/>. Each test should call <see cref="CreateContext"/>
/// per arrange/act/assert step rather than reusing one context, to avoid EF Core's change tracker
/// masking real persistence bugs.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("vendix_test")
        .WithUsername("vendix")
        .WithPassword("vendix")
        .Build();

    /// <summary>
    /// Starts the PostgreSQL container and applies all EF Core migrations against it.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Stops and removes the PostgreSQL container.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a new <see cref="VendixDbContext"/> pointed at the running container.
    /// </summary>
    public VendixDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<VendixDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new VendixDbContext(options);
    }
}

/// <summary>
/// xUnit collection definition so all fixture-consuming test classes share one container
/// instead of each spinning up its own (which would be correct but slow).
/// </summary>
[CollectionDefinition(nameof(DatabaseCollection))]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
```

Append a smoke test to the bottom of the same file:

```csharp
[Collection(nameof(DatabaseCollection))]
public class DatabaseFixtureTests
{
    private readonly DatabaseFixture _fixture;

    public DatabaseFixtureTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateContext_AfterInitialize_CanConnectAndHasAppliedMigrations()
    {
        await using var context = _fixture.CreateContext();

        var canConnect = await context.Database.CanConnectAsync();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        canConnect.Should().BeTrue();
        pendingMigrations.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run the smoke test to verify the fixture works**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~DatabaseFixtureTests"`
Expected: PASS (requires Docker Desktop / Docker daemon running). If it fails with a Docker connection error, start Docker and re-run — do not change the test.

- [ ] **Step 3: Commit**

```bash
git add tests/Vendix.Integration.Tests/Persistence/DatabaseFixture.cs
git commit -m "test: add Testcontainers PostgreSQL fixture for catalog integration tests"
```

---

### Task 2: Product Repository Integration Tests

**Files:**
- Create: `tests/Vendix.Integration.Tests/Persistence/ProductRepositoryTests.cs`

**Interfaces:**
- Consumes: `DatabaseFixture.CreateContext()` from Task 1. `ProductRepository(VendixDbContext context)` from `Vendix.Infrastructure.Persistence.Repositories`. `Product(string name, Sku sku, Slug slug, Money price, ProductType productType, string? description = null, Guid? categoryId = null, Guid? brandId = null)` from `Vendix.Domain.Catalog.Entities`. `Category(string name, Slug slug, Guid? parentCategoryId = null)`.

- [ ] **Step 1: Write the failing tests**

```csharp
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Enums;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

[Collection(nameof(DatabaseCollection))]
public class ProductRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static string UniqueSku(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedProductWithCorrectPrice()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new ProductRepository(writeContext);
        var product = new Product(
            "Integration Test Product",
            new Sku(UniqueSku("ITP")),
            new Slug($"integration-test-product-{Guid.NewGuid():N}"),
            new Money(49.99m, "USD"),
            ProductType.Physical);

        // Act
        await writeRepository.AddAsync(product);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var found = await readRepository.GetByIdAsync(product.Id);

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Integration Test Product");
        found.Price.Amount.Should().Be(49.99m);
        found.Price.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetBySlugAsync_DifferentCase_ReturnsProduct()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new ProductRepository(writeContext);
        var slug = $"case-test-{Guid.NewGuid():N}";
        var product = new Product(
            "Case Test Product",
            new Sku(UniqueSku("CTP")),
            new Slug(slug),
            new Money(19.99m, "USD"),
            ProductType.Physical);

        await writeRepository.AddAsync(product);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var found = await readRepository.GetBySlugAsync(slug.ToUpperInvariant());

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task SearchAsync_FilterByCategory_ReturnsOnlyMatchingProducts()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var category = new Category("Search Test Category", new Slug($"search-test-cat-{Guid.NewGuid():N}"));
        writeContext.Categories.Add(category);
        await writeContext.SaveChangesAsync();

        var writeRepository = new ProductRepository(writeContext);
        var matching = new Product(
            "Matching Product",
            new Sku(UniqueSku("MAT")),
            new Slug($"matching-product-{Guid.NewGuid():N}"),
            new Money(10m, "USD"),
            ProductType.Physical,
            categoryId: category.Id);
        var nonMatching = new Product(
            "Other Product",
            new Sku(UniqueSku("OTH")),
            new Slug($"other-product-{Guid.NewGuid():N}"),
            new Money(10m, "USD"),
            ProductType.Physical);

        await writeRepository.AddAsync(matching);
        await writeRepository.AddAsync(nonMatching);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var (items, totalCount) = await readRepository.SearchAsync(categoryId: category.Id, pageNumber: 1, pageSize: 10);

        // Assert
        items.Should().ContainSingle(p => p.Id == matching.Id);
        items.Should().NotContain(p => p.Id == nonMatching.Id);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task Update_ChangedPrice_PersistsNewPrice()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new ProductRepository(writeContext);
        var product = new Product(
            "Update Test Product",
            new Sku(UniqueSku("UPD")),
            new Slug($"update-test-product-{Guid.NewGuid():N}"),
            new Money(25m, "USD"),
            ProductType.Physical);
        await writeRepository.AddAsync(product);
        await writeContext.SaveChangesAsync();

        // Act — load fresh, mutate, persist via a new context+repository (mirrors handler flow)
        await using var updateContext = _fixture.CreateContext();
        var updateRepository = new ProductRepository(updateContext);
        var loaded = await updateRepository.GetByIdAsync(product.Id);
        loaded!.UpdatePrice(new Money(30m, "USD"));
        updateRepository.MarkPriceAsModified(loaded);
        await updateContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new ProductRepository(readContext);
        var reloaded = await readRepository.GetByIdAsync(product.Id);

        // Assert
        reloaded!.Price.Amount.Should().Be(30m);
    }
}
```

> Note: `Product.UpdatePrice(Money)` is assumed to exist alongside `UpdateName`/`UpdateDescription` (same naming convention). If the actual method has a different name, run `grep -n "public void Update" src/Vendix.Domain/Catalog/Entities/Product.cs` first and use whatever price-mutation method exists — do not add a new one; this task tests existing behavior only.

- [ ] **Step 2: Run tests to verify they fail or reveal the real API**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~ProductRepositoryTests"`
Expected: Compiles and runs against the real repository. If `UpdatePrice` doesn't exist, fix the compile error using the actual method name found via the grep above, then re-run.

- [ ] **Step 3: Run tests to verify they pass**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~ProductRepositoryTests"`
Expected: PASS (4 tests)

- [ ] **Step 4: Commit**

```bash
git add tests/Vendix.Integration.Tests/Persistence/ProductRepositoryTests.cs
git commit -m "test: add Product repository integration tests against real PostgreSQL"
```

---

### Task 3: Category Repository Integration Tests

**Files:**
- Create: `tests/Vendix.Integration.Tests/Persistence/CategoryRepositoryTests.cs`

**Interfaces:**
- Consumes: `CategoryRepository(VendixDbContext context)`, `ICategoryRepository.GetRootCategoriesAsync`, `GetWithChildrenAsync(Guid)`, `GetBySlugAsync(string)`. `Category.AddTranslation(string languageCode, string name, string? description = null)` returning `CategoryTranslation`.

- [ ] **Step 1: Write the failing tests**

```csharp
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

[Collection(nameof(DatabaseCollection))]
public class CategoryRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public CategoryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetRootCategoriesAsync_ExcludesSubCategories()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var root = new Category("Root Category", new Slug($"root-cat-{Guid.NewGuid():N}"));
        writeContext.Categories.Add(root);
        await writeContext.SaveChangesAsync();

        var child = new Category("Child Category", new Slug($"child-cat-{Guid.NewGuid():N}"), root.Id);
        writeContext.Categories.Add(child);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new CategoryRepository(readContext);
        var roots = await readRepository.GetRootCategoriesAsync();

        // Assert
        roots.Should().Contain(c => c.Id == root.Id);
        roots.Should().NotContain(c => c.Id == child.Id);
    }

    [Fact]
    public async Task GetWithChildrenAsync_ReturnsParentWithSubCategoriesLoaded()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var parent = new Category("Parent Category", new Slug($"parent-cat-{Guid.NewGuid():N}"));
        writeContext.Categories.Add(parent);
        await writeContext.SaveChangesAsync();

        var child = new Category("Child Of Parent", new Slug($"child-of-parent-{Guid.NewGuid():N}"), parent.Id);
        writeContext.Categories.Add(child);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new CategoryRepository(readContext);
        var found = await readRepository.GetWithChildrenAsync(parent.Id);

        // Assert
        found.Should().NotBeNull();
        found!.SubCategories.Should().ContainSingle(c => c.Id == child.Id);
    }

    [Fact]
    public async Task AddTranslation_ThenReload_PersistsTranslation()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var category = new Category("Translated Category", new Slug($"translated-cat-{Guid.NewGuid():N}"));
        category.AddTranslation("fa", "دسته‌بندی ترجمه‌شده", "توضیحات نمونه");
        writeContext.Categories.Add(category);
        await writeContext.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateContext();
        var readRepository = new CategoryRepository(readContext);
        var found = await readRepository.GetBySlugAsync(category.Slug.Value);

        // Assert
        found.Should().NotBeNull();
        found!.Translations.Should().ContainSingle(t => t.LanguageCode == "fa" && t.Name == "دسته‌بندی ترجمه‌شده");
    }
}
```

- [ ] **Step 2: Run tests to verify they compile and reveal any API mismatches**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~CategoryRepositoryTests"`
Expected: Compiles against the real `ICategoryRepository`/`Category` API. If `CategoryTranslation.Name` is actually called something else (e.g. `Title`, matching `ProductTranslation.Title`), run `grep -n "class CategoryTranslation" -A 15 src/Vendix.Domain/Catalog/Entities/CategoryTranslation.cs` and use the real property name.

- [ ] **Step 3: Run tests to verify they pass**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~CategoryRepositoryTests"`
Expected: PASS (3 tests)

- [ ] **Step 4: Commit**

```bash
git add tests/Vendix.Integration.Tests/Persistence/CategoryRepositoryTests.cs
git commit -m "test: add Category repository integration tests against real PostgreSQL"
```

---

### Task 4: Brand Repository Integration Tests

**Files:**
- Create: `tests/Vendix.Integration.Tests/Persistence/BrandRepositoryTests.cs`

**Interfaces:**
- Consumes: `BrandRepository(VendixDbContext context)`, `IBrandRepository.GetBySlugAsync(string)`, `GetAllAsync()`. `Brand(string name, Slug slug, string? logoUrl = null)`, `Brand.MarkAsDeleted(string? deletedBy = null)` (from `ISoftDelete`).

- [ ] **Step 1: Write the failing tests**

```csharp
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Infrastructure.Persistence.Repositories;

namespace Vendix.Integration.Tests.Persistence;

[Collection(nameof(DatabaseCollection))]
public class BrandRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public BrandRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ThenGetBySlug_ReturnsPersistedBrand()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BrandRepository(writeContext);
        var slug = $"acme-{Guid.NewGuid():N}";
        var brand = new Brand("Acme", new Slug(slug), "https://example.com/logo.png");

        // Act
        await writeRepository.AddAsync(brand);
        await writeContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BrandRepository(readContext);
        var found = await readRepository.GetBySlugAsync(slug);

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Acme");
        found.LogoUrl.Should().Be("https://example.com/logo.png");
    }

    [Fact]
    public async Task AddAsync_DuplicateSlug_ThrowsOnSaveDueToUniqueIndex()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BrandRepository(writeContext);
        var slug = $"dup-brand-{Guid.NewGuid():N}";
        await writeRepository.AddAsync(new Brand("First", new Slug(slug)));
        await writeContext.SaveChangesAsync();

        // Act
        await using var secondContext = _fixture.CreateContext();
        var secondRepository = new BrandRepository(secondContext);
        await secondRepository.AddAsync(new Brand("Second", new Slug(slug)));
        var act = async () => await secondContext.SaveChangesAsync();

        // Assert — the unique index on Slug (BrandConfiguration) rejects the duplicate at the DB level
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task MarkAsDeleted_ThenGetAll_ExcludesSoftDeletedBrand()
    {
        // Arrange
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new BrandRepository(writeContext);
        var brand = new Brand("To Be Deleted", new Slug($"to-be-deleted-{Guid.NewGuid():N}"));
        await writeRepository.AddAsync(brand);
        await writeContext.SaveChangesAsync();

        // Act
        await using var deleteContext = _fixture.CreateContext();
        var deleteRepository = new BrandRepository(deleteContext);
        var loaded = await deleteContext.Brands.FirstAsync(b => b.Id == brand.Id);
        loaded.MarkAsDeleted("test-user");
        await deleteContext.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var readRepository = new BrandRepository(readContext);
        var all = await readRepository.GetAllAsync();

        // Assert — BrandConfiguration's global query filter (!IsDeleted) hides it
        all.Should().NotContain(b => b.Id == brand.Id);
    }
}
```

Add `using Microsoft.EntityFrameworkCore;` at the top for `DbUpdateException` and `FirstAsync`.

- [ ] **Step 2: Run tests to verify they fail appropriately / reveal API mismatches**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~BrandRepositoryTests"`
Expected: Compiles. The duplicate-slug test should throw `DbUpdateException` because `BrandConfiguration` (mirrors `ProductConfiguration`'s pattern) puts a unique index on `Slug`; if `BrandConfiguration` doesn't yet have that index, this test will fail here — check `src/Vendix.Infrastructure/Persistence/Configurations/BrandConfiguration.cs` and add `builder.HasIndex(b => b.Slug).IsUnique();` if missing (this would be the one production-code touch allowed in this test-only plan, since it's completing documented Phase 1/2 behavior, not new scope — confirm with the user before changing `src/` if it's actually missing).

- [ ] **Step 3: Run tests to verify they pass**

Run: `dotnet test tests/Vendix.Integration.Tests --filter "FullyQualifiedName~BrandRepositoryTests"`
Expected: PASS (3 tests)

- [ ] **Step 4: Commit**

```bash
git add tests/Vendix.Integration.Tests/Persistence/BrandRepositoryTests.cs
git commit -m "test: add Brand repository integration tests against real PostgreSQL"
```

---

### Task 5: Documentation — Close Out Phase 2

**Files:**
- Modify: `docs/ARCHITECTURE.md` (checklist item 12, status header, Phase 2 Checklist table)
- Modify: `docs/CHANGELOG.md` (new entry)

**Interfaces:**
- None (docs only).

- [ ] **Step 1: Update `docs/ARCHITECTURE.md`**

Change the status line near the top:

```markdown
> **Status:** Phase 2 - Core Catalog (12/12 tasks complete) ✅
```

In §14 "Implementation Phases", change:

```markdown
- [ ] Variants & Specs management (domain/DTO/display done, admin UI pending)
```
stays as-is (that item is intentionally still open per the existing note — it is not part of Task 12's scope, do not mark it done).

In the same section, change the Phase 2 header from `⏳ (Current)` to `✅ (Completed: <today's date>)` only if Variants & Specs admin UI is also done — otherwise leave the phase header as `⏳` since one sub-item remains, and only update the Task 12 line in the §17 checklist table:

In §17 "Phase 2 Checklist" table, change:

```markdown
| 12 | Unit & Integration Tests | ⬜ |
```
to:
```markdown
| 12 | Unit & Integration Tests | ✅ |
```

Add a new row to the "Code Reviews" table:

```markdown
| Task 12 | `docs/CHANGELOG.md` (Phase 2 - Task 12 entry) | - |
```

- [ ] **Step 2: Add a CHANGELOG.md entry**

Insert under `## [Unreleased]`, above the most recent existing entry:

```markdown
### Phase 2 - Task 12: Catalog Integration Tests (Date: <today's date, e.g. 2026-09-01>)

**Added:**

Integration Tests (tests/Vendix.Integration.Tests/Persistence/):
- `DatabaseFixture.cs` - Testcontainers PostgreSQL 16 fixture, one container shared per test collection, applies EF Core migrations on startup
- `ProductRepositoryTests.cs` - Add/GetById round-trip, case-insensitive GetBySlug, SearchAsync category filter, price update persistence
- `CategoryRepositoryTests.cs` - Root category filtering, parent/children loading, translation persistence
- `BrandRepositoryTests.cs` - Add/GetBySlug round-trip, unique slug constraint enforcement, soft-delete query filter exclusion

**Technical Decisions:**
- Used Testcontainers instead of an in-memory EF provider so unique indexes, owned-entity (Money) column mapping, and PostgreSQL-specific behavior (ILike, split queries) are exercised for real.
- Each test creates a fresh `VendixDbContext` per arrange/act/assert step to mirror how production request-scoped contexts behave, avoiding false passes from a lingering change tracker.

**Notes:**
- Requires Docker running locally / in CI for `Vendix.Integration.Tests` to execute.
- Phase 2 (Core Catalog) is now 12/12 tasks complete.

---
```

- [ ] **Step 3: Commit**

```bash
git add docs/ARCHITECTURE.md docs/CHANGELOG.md
git commit -m "docs: close out Phase 2 - Core Catalog (12/12 tasks complete)"
```

---

### Task 6: Full Solution Verification

**Files:** None (verification only).

- [ ] **Step 1: Build the whole solution**

Run: `dotnet build`
Expected: 0 errors, 0 warnings (matches the baseline established in the 2026-08-13 CHANGELOG entry).

- [ ] **Step 2: Run the full test suite**

Run: `dotnet test`
Expected: All prior tests still pass (265 baseline) plus the new integration tests from Tasks 1-4 (1 fixture smoke test + 4 + 3 + 3 = 11 new tests), so ≥276 passing, 0 failing.

- [ ] **Step 3: Report results**

If anything fails, do not mark Phase 2 complete — fix the failing test or the docs update from Task 5 accordingly, re-run, and only proceed once green.

---

## Self-Review Notes

- **Spec coverage:** `docs/ARCHITECTURE.md` §14 item 12 ("Unit & Integration Tests") → Tasks 1-4. Docs closure → Task 5. Verification → Task 6. No gaps.
- **Placeholder scan:** All test code is concrete; the two "check the real API name" call-outs (Task 2's `UpdatePrice`, Task 3's `CategoryTranslation.Name`) are flagged explicitly with the exact `grep` command to resolve them rather than left as TBD, because the exact method/property name wasn't confirmed by reading the full entity file in the planning session — the executor resolves this in Step 2 of those tasks before proceeding, not by guessing.
- **Type consistency:** `DatabaseFixture.CreateContext()` signature is identical across all consuming tasks; repository constructor calls (`new ProductRepository(context)`, etc.) match the existing production constructors read from `src/Vendix.Infrastructure/Persistence/Repositories/`.
