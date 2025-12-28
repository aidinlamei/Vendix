using Mapster;
using Vendix.Application.Catalog.DTOs;
using Vendix.Domain.Catalog.Entities;

namespace Vendix.Application.Catalog.Mappings;

public class BrandMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Brand, BrandDto>()
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.ProductCount, src => 0); // TODO: Add product count via repository

        config.NewConfig<Brand, BrandListDto>()
            .Map(dest => dest.Slug, src => src.Slug.Value)
            .Map(dest => dest.ProductCount, src => 0); // TODO: Add product count via repository

        config.NewConfig<Brand, BrandSelectDto>();
    }
}

