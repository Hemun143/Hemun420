namespace Integrations.HKTDC.SPCS.Mapping;

/// <summary>
/// Price list item AutoMapper profile.
/// </summary>
public class PriceListItemProfile : Profile
{
    public PriceListItemProfile()
    {
        _ = CreateMap<PriceTier, PriceListItemsModel>()
                .ForMember(dest => dest.AlternateDescription4, opt => opt.MapFrom(src => src.EffectiveStart))
                .ForMember(dest => dest.AlternateDescription5, opt => opt.MapFrom(src => src.EffectiveEnd))
                .ForMember(dest => dest.MinimumQuantity, opt => opt.MapFrom(src => src.Quantity));
    }
}
