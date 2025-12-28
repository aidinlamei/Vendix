using Vendix.Domain.Catalog.ValueObjects;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents a brand or manufacturer of products.
/// </summary>
/// <remarks>
/// Brand is an aggregate root that groups products by manufacturer.
/// It implements IAuditableEntity for tracking creation and modification,
/// and ISoftDelete for soft deletion support.
/// </remarks>
public class Brand : AggregateRoot, IAuditableEntity, ISoftDelete
{
    /// <summary>
    /// Gets the brand name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the URL-friendly slug for the brand.
    /// </summary>
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// Gets the URL to the brand's logo image.
    /// </summary>
    public string? LogoUrl { get; private set; }

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
    private Brand() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Brand"/> class.
    /// </summary>
    /// <param name="name">The brand name. Cannot be null or whitespace.</param>
    /// <param name="slug">The URL-friendly slug for the brand.</param>
    /// <param name="logoUrl">Optional URL to the brand's logo image.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when slug is null.</exception>
    public Brand(string name, Slug slug, string? logoUrl = null)
    {
        SetName(name);
        SetSlug(slug);
        LogoUrl = logoUrl;
    }

    /// <summary>
    /// Updates the brand name.
    /// </summary>
    /// <param name="name">The new brand name.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void UpdateName(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Updates the brand slug.
    /// </summary>
    /// <param name="slug">The new slug.</param>
    /// <exception cref="ArgumentNullException">Thrown when slug is null.</exception>
    public void UpdateSlug(Slug slug)
    {
        SetSlug(slug);
    }

    /// <summary>
    /// Updates the brand logo URL.
    /// </summary>
    /// <param name="logoUrl">The new logo URL, or null to remove.</param>
    public void UpdateLogoUrl(string? logoUrl)
    {
        LogoUrl = logoUrl;
    }

    /// <inheritdoc />
    public void MarkAsDeleted(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Brand name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetSlug(Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);
        Slug = slug;
    }
}
