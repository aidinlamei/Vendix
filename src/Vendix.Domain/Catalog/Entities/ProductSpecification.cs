using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.Entities;

/// <summary>
/// Represents a specification or attribute of a product.
/// </summary>
/// <remarks>
/// Specifications are key-value pairs that describe product characteristics
/// such as dimensions, weight, material, or technical details.
/// </remarks>
public class ProductSpecification : BaseEntity
{
    /// <summary>
    /// Gets the ID of the parent product.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the specification name (e.g., "Weight", "Material", "Screen Size").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the specification value (e.g., "500g", "Cotton", "6.5 inches").
    /// </summary>
    public string Value { get; private set; } = null!;

    /// <summary>
    /// Gets the parent product navigation property.
    /// </summary>
    public Product Product { get; private set; } = null!;

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private ProductSpecification() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductSpecification"/> class.
    /// </summary>
    /// <param name="productId">The ID of the parent product.</param>
    /// <param name="name">The specification name.</param>
    /// <param name="value">The specification value.</param>
    /// <exception cref="ArgumentException">Thrown when productId is empty, or name/value is null or whitespace.</exception>
    public ProductSpecification(Guid productId, string name, string value)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        }

        ProductId = productId;
        SetName(name);
        SetValue(value);
    }

    /// <summary>
    /// Updates the specification name.
    /// </summary>
    /// <param name="name">The new specification name.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public void UpdateName(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Updates the specification value.
    /// </summary>
    /// <param name="value">The new specification value.</param>
    /// <exception cref="ArgumentException">Thrown when value is null or whitespace.</exception>
    public void UpdateValue(string value)
    {
        SetValue(value);
    }

    /// <summary>
    /// Updates both the name and value of the specification.
    /// </summary>
    /// <param name="name">The new specification name.</param>
    /// <param name="value">The new specification value.</param>
    /// <exception cref="ArgumentException">Thrown when name or value is null or whitespace.</exception>
    public void Update(string name, string value)
    {
        SetName(name);
        SetValue(value);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Specification name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Specification value is required.", nameof(value));
        }

        Value = value.Trim();
    }
}
