namespace Vendix.Application.Catalog.DTOs;

/// <summary>
/// Category data transfer object for detail views
/// </summary>
public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
    public string? ParentName { get; init; }
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<CategoryTranslationDto> Translations { get; init; } = [];
    public List<CategoryDto> Children { get; init; } = [];
}

/// <summary>
/// Lightweight category for lists and dropdowns
/// </summary>
public record CategoryListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public int ProductCount { get; init; }
    public int ChildrenCount { get; init; }
}

/// <summary>
/// Category translation DTO
/// </summary>
public record CategoryTranslationDto
{
    public string LanguageCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// Tree node for hierarchical category display
/// </summary>
public record CategoryTreeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int Level { get; init; }
    public List<CategoryTreeDto> Children { get; init; } = [];
}

