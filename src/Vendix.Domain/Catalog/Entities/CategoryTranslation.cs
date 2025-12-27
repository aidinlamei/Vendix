using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents a translation of category content for a specific language.
/// </summary>
/// <remarks>
/// Category translations enable multi-language support by storing localized
/// names and descriptions for each supported language.
/// </remarks>
public class CategoryTranslation : BaseEntity
{
    /// <summary>
    /// Gets the ID of the parent category.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Gets the ISO 639-1 language code (e.g., "en", "fa").
    /// </summary>
    public string LanguageCode { get; private set; } = null!;

    /// <summary>
    /// Gets the translated category name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the translated category description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the parent category navigation property.
    /// </summary>
    public Category Category { get; private set; } = null!;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private CategoryTranslation() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryTranslation"/> class.
    /// </summary>
    /// <param name="categoryId">The ID of the parent category.</param>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <param name="name">The translated category name.</param>
    /// <param name="description">Optional translated category description.</param>
    /// <exception cref="ArgumentException">Thrown when categoryId is empty, or languageCode/name is null or whitespace.</exception>
    public CategoryTranslation(
        Guid categoryId,
        string languageCode,
        string name,
        string? description = null)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));
        }

        CategoryId = categoryId;
        SetLanguageCode(languageCode);
        SetName(name);
        Description = description?.Trim();
    }

    /// <summary>
    /// Updates the translated name.
    /// </summary>
    /// <param name="name">The new translated name.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void UpdateName(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Updates the translated description.
    /// </summary>
    /// <param name="description">The new translated description, or null to remove.</param>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }

    /// <summary>
    /// Updates both the name and description.
    /// </summary>
    /// <param name="name">The new translated name.</param>
    /// <param name="description">The new translated description, or null to remove.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void Update(string name, string? description)
    {
        SetName(name);
        Description = description?.Trim();
    }

    private void SetLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new ArgumentException("Language code is required.", nameof(languageCode));
        }

        if (languageCode.Length != 2)
        {
            throw new ArgumentException("Language code must be a 2-character ISO 639-1 code.", nameof(languageCode));
        }

        LanguageCode = languageCode.ToLowerInvariant();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Name = name.Trim();
    }
}
