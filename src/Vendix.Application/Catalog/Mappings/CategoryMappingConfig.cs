using Mapster;
using Vendix.Application.Catalog.DTOs;
using Vendix.Domain.Catalog.Entities;

namespace Vendix.Application.Catalog.Mappings;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryDto>()
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.Description, src => (string?)null) // Description is only in translations
            .Map(dest => dest.ParentId, src => src.ParentCategory != null ? src.ParentCategory.Id : (Guid?)null)
            .Map(dest => dest.ParentName, src => src.ParentCategory != null ? src.ParentCategory.Name : null)
            .Map(dest => dest.ProductCount, src => src.Products.Count(p => !p.IsDeleted))
            .Map(dest => dest.Children, src => src.SubCategories.Where(c => !c.IsDeleted))
            .Map(dest => dest.Translations, src => src.Translations);

        config.NewConfig<Category, CategoryListDto>()
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.ParentId, src => src.ParentCategory != null ? src.ParentCategory.Id : (Guid?)null)
            .Map(dest => dest.ProductCount, src => src.Products.Count(p => !p.IsDeleted))
            .Map(dest => dest.ChildrenCount, src => src.SubCategories.Count(c => !c.IsDeleted));

        config.NewConfig<CategoryTranslation, CategoryTranslationDto>();
    }
}

