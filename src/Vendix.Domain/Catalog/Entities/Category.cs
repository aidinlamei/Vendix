using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents a product category in the catalog.
/// </summary>
/// <remarks>
/// Category is an aggregate root that organizes products into a hierarchical structure.
/// Categories can have parent categories (for subcategories) and translations for multi-language support.
/// </remarks>
public class Category : AggregateRoot, IAuditableEntity, ISoftDelete
{
    private readonly List<Product> _products = [];
    private readonly List<CategoryTranslation> _translations = [];
    private readonly List<Category> _subCategories = [];

    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the URL-friendly slug for the category.
    /// </summary>
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the parent category, or null if this is a root category.
    /// </summary>
    public Guid? ParentCategoryId { get; private set; }

    /// <summary>
    /// Gets the parent category navigation property.
    /// </summary>
    public Category? ParentCategory { get; private set; }

    /// <summary>
    /// Gets the products in this category.
    /// </summary>
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    /// <summary>
    /// Gets the translations for this category.
    /// </summary>
    public IReadOnlyCollection<CategoryTranslation> Translations => _translations.AsReadOnly();

    /// <summary>
    /// Gets the subcategories of this category.
    /// </summary>
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this entity has been soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Category() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Category"/> class.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="slug">The URL-friendly slug.</param>
    /// <param name="parentCategoryId">Optional parent category ID for subcategories.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when slug is null.</exception>
    public Category(string name, Slug slug, Guid? parentCategoryId = null)
    {
        SetName(name);
        SetSlug(slug);
        ParentCategoryId = parentCategoryId;
    }

    /// <summary>
    /// Updates the category name.
    /// </summary>
    /// <param name="name">The new category name.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void UpdateName(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Updates the category slug.
    /// </summary>
    /// <param name="slug">The new slug.</param>
    /// <exception cref="ArgumentNullException">Thrown when slug is null.</exception>
    public void UpdateSlug(Slug slug)
    {
        SetSlug(slug);
    }

    /// <summary>
    /// Sets the parent category, making this a subcategory.
    /// </summary>
    /// <param name="parentCategoryId">The parent category ID, or null to make this a root category.</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to set itself as parent.</exception>
    public void SetParentCategory(Guid? parentCategoryId)
    {
        if (parentCategoryId == Id)
        {
            throw new InvalidOperationException("A category cannot be its own parent.");
        }

        ParentCategoryId = parentCategoryId;
    }

    /// <summary>
    /// Adds a translation for this category.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <param name="name">The translated category name.</param>
    /// <param name="description">Optional translated description.</param>
    /// <returns>The created translation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a translation for this language already exists.</exception>
    public CategoryTranslation AddTranslation(string languageCode, string name, string? description = null)
    {
        var normalizedCode = languageCode.ToLowerInvariant();

        if (_translations.Any(t => t.LanguageCode == normalizedCode))
        {
            throw new InvalidOperationException($"A translation for language '{languageCode}' already exists.");
        }

        var translation = new CategoryTranslation(Id, languageCode, name, description);
        _translations.Add(translation);
        return translation;
    }

    /// <summary>
    /// Gets the translation for a specific language.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <returns>The translation if found; otherwise, null.</returns>
    public CategoryTranslation? GetTranslation(string languageCode)
    {
        var normalizedCode = languageCode.ToLowerInvariant();
        return _translations.FirstOrDefault(t => t.LanguageCode == normalizedCode);
    }

    /// <summary>
    /// Gets the category name in the specified language, falling back to the default name.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <returns>The translated name if available; otherwise, the default name.</returns>
    public string GetName(string languageCode)
    {
        return GetTranslation(languageCode)?.Name ?? Name;
    }

    /// <summary>
    /// Removes a translation for the specified language.
    /// </summary>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <returns>True if the translation was removed; otherwise, false.</returns>
    public bool RemoveTranslation(string languageCode)
    {
        var normalizedCode = languageCode.ToLowerInvariant();
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == normalizedCode);

        if (translation is null)
        {
            return false;
        }

        _translations.Remove(translation);
        return true;
    }

    /// <inheritdoc />
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetSlug(Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);
        Slug = slug;
    }
}
