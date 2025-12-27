namespace Vendix.Domain.Catalog.Enums;

/// <summary>
/// Represents the type of product in the catalog.
/// </summary>
public enum ProductType
{
    /// <summary>
    /// A physical product that requires shipping.
    /// </summary>
    Physical = 0,

    /// <summary>
    /// A digital product that can be downloaded or accessed online.
    /// </summary>
    Digital = 1
}
