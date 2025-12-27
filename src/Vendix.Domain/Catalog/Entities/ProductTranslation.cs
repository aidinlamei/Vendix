using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents a translation of product content for a specific language.
/// </summary>
/// <remarks>
/// Product translations enable multi-language support by storing localized
/// titles and descriptions for each supported language.
/// </remarks>
public class ProductTranslation : BaseEntity
{
    /// <summary>
    /// Gets the ID of the parent product.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the ISO 639-1 language code (e.g., "en", "fa").
    /// </summary>
    public string LanguageCode { get; private set; } = null!;

    /// <summary>
    /// Gets the translated product title.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Gets the translated product description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the parent product navigation property.
    /// </summary>
    public Product Product { get; private set; } = null!;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private ProductTranslation() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductTranslation"/> class.
    /// </summary>
    /// <param name="productId">The ID of the parent product.</param>
    /// <param name="languageCode">The ISO 639-1 language code.</param>
    /// <param name="title">The translated product title.</param>
    /// <param name="description">Optional translated product description.</param>
    /// <exception cref="ArgumentException">Thrown when productId is empty, or languageCode/title is null or whitespace.</exception>
    public ProductTranslation(
        Guid productId,
        string languageCode,
        string title,
        string? description = null)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        }

        ProductId = productId;
        SetLanguageCode(languageCode);
        SetTitle(title);
        Description = description?.Trim();
    }

    /// <summary>
    /// Updates the translated title.
    /// </summary>
    /// <param name="title">The new translated title.</param>
    /// <exception cref="ArgumentException">Thrown when title is null or whitespace.</exception>
    public void UpdateTitle(string title)
    {
        SetTitle(title);
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
    /// Updates both the title and description.
    /// </summary>
    /// <param name="title">The new translated title.</param>
    /// <param name="description">The new translated description, or null to remove.</param>
    /// <exception cref="ArgumentException">Thrown when title is null or whitespace.</exception>
    public void Update(string title, string? description)
    {
        SetTitle(title);
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

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Title = title.Trim();
    }
}
