using Mapster;
using Vendix.Application.Basket.DTOs;
using BasketEntity = Vendix.Domain.Basket.Entities.Basket;
using Vendix.Domain.Basket.Entities;

namespace Vendix.Application.Basket.Mappings;

public class BasketMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BasketItem, BasketItemDto>()
            .Map(dest => dest.UnitPrice, src => src.UnitPrice.Amount)
            .Map(dest => dest.Currency, src => src.UnitPrice.Currency)
            .Map(dest => dest.LineTotal, src => src.LineTotal);

        config.NewConfig<BasketEntity, BasketDto>()
            .Map(dest => dest.Items, src => src.Items)
            .Map(dest => dest.Subtotal, src => src.Items.Sum(i => i.LineTotal))
            .Map(dest => dest.Currency, src => src.Items.Select(i => i.UnitPrice.Currency).FirstOrDefault())
            .Map(dest => dest.ItemCount, src => src.Items.Sum(i => i.Quantity));
    }
}
