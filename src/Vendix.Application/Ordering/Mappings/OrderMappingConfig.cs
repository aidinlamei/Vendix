using Mapster;
using Vendix.Application.Ordering.DTOs;
using Vendix.Domain.Ordering.Entities;

namespace Vendix.Application.Ordering.Mappings;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderItem, OrderItemDto>();

        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.OrderNumber, src => src.OrderNumber.Value)
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<Order, OrderListDto>()
            .Map(dest => dest.OrderNumber, src => src.OrderNumber.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ItemCount, src => src.Items.Sum(i => i.Quantity));
    }
}
