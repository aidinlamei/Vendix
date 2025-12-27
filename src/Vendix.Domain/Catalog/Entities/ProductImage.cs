using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents an image associated with a product.
/// </summary>
/// <remarks>
/// Product images are used to display the product in the catalog and detail pages.
/// Each image has a URL, alt text for accessibility, and a sort order for display sequence.
/// One image can be marked as the main/primary image.
/// </remarks>
public class ProductImage : BaseEntity
{
    /// <summary>
    /// Gets the ID of the parent product.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the URL where the image is stored.
    /// </summary>
    public string Url { get; private set; } = null!;

    /// <summary>
    /// Gets the alternative text for accessibility.
    /// </summary>
    public string? AltText { get; private set; }

    /// <summary>
    /// Gets the sort order for displaying images.
    /// </summary>
    /// <remarks>
    /// Lower values are displayed first.
    /// </remarks>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is the main/primary image for the product.
    /// </summary>
    public bool IsMain { get; private set; }

    /// <summary>
    /// Gets the parent product navigation property.
    /// </summary>
    public Product Product { get; private set; } = null!;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private ProductImage() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductImage"/> class.
    /// </summary>
    /// <param name="productId">The ID of the parent product.</param>
    /// <param name="url">The URL where the image is stored.</param>
    /// <param name="altText">Optional alternative text for accessibility.</param>
    /// <param name="sortOrder">The display sort order. Defaults to 0.</param>
    /// <param name="isMain">Whether this is the main product image. Defaults to false.</param>
    /// <exception cref="ArgumentException">Thrown when productId is empty, url is null/whitespace, or sortOrder is negative.</exception>
    public ProductImage(
        Guid productId,
        string url,
        string? altText = null,
        int sortOrder = 0,
        bool isMain = false)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        }

        ProductId = productId;
        SetUrl(url);
        AltText = altText?.Trim();
        SetSortOrder(sortOrder);
        IsMain = isMain;
    }

    /// <summary>
    /// Updates the image URL.
    /// </summary>
    /// <param name="url">The new URL.</param>
    /// <exception cref="ArgumentException">Thrown when url is null or whitespace.</exception>
    public void UpdateUrl(string url)
    {
        SetUrl(url);
    }

    /// <summary>
    /// Updates the alternative text.
    /// </summary>
    /// <param name="altText">The new alt text, or null to remove.</param>
    public void UpdateAltText(string? altText)
    {
        AltText = altText?.Trim();
    }

    /// <summary>
    /// Updates the sort order.
    /// </summary>
    /// <param name="sortOrder">The new sort order.</param>
    /// <exception cref="ArgumentException">Thrown when sortOrder is negative.</exception>
    public void UpdateSortOrder(int sortOrder)
    {
        SetSortOrder(sortOrder);
    }

    /// <summary>
    /// Marks this image as the main product image.
    /// </summary>
    public void SetAsMain()
    {
        IsMain = true;
    }

    /// <summary>
    /// Removes the main status from this image.
    /// </summary>
    public void UnsetAsMain()
    {
        IsMain = false;
    }

    private void SetUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Image URL is required.", nameof(url));
        }

        Url = url.Trim();
    }

    private void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentException("Sort order cannot be negative.", nameof(sortOrder));
        }

        SortOrder = sortOrder;
    }
}
