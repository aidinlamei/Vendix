namespace Vendix.Web.Components.Shared;

/// <summary>
/// Event arguments for ProductFilters filter changes.
/// </summary>
public class FilterChangedEventArgs
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

