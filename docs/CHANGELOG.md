# Changelog

All notable changes to the Vendix project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

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
