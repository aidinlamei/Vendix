namespace Vendix.Application.Catalog.DTOs;

/// <summary>
/// Brand data transfer object for detail views
/// </summary>
public record BrandDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ProductCount { get; init; }
}

/// <summary>
/// Lightweight brand for lists and dropdowns
/// </summary>
public record BrandListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public int ProductCount { get; init; }
}

/// <summary>
/// Brand for dropdown selection
/// </summary>
public record BrandSelectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

