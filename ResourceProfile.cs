using HKTDC.DAL.Models;

namespace Integrations.HKTDC.SPCS.Mapping;

/// <summary>
/// Resource AutoMapper profile.
/// </summary>
public class ResourceProfile : Profile
{
    public ResourceProfile()
    {
        _ = CreateMap<SellableItemBase, ResourcesModel>()
                .ForMember(dest => dest.ResourceCodeDescription, opt => opt.MapFrom(src => src.NameEN))
                .ForMember(dest => dest.ResourceTypeDescription, opt => opt.MapFrom(src => src.NameEN))
                .ForMember(dest => dest.AlternateCodeDescription1, opt => opt.MapFrom(src => src.NameTC))
                .ForMember(dest => dest.AlternateCodeDescription2, opt => opt.MapFrom(src => src.NameSC))
                .ForMember(dest => dest.AlternateCodeDescription4, opt => opt.MapFrom(src => src.EffectiveStart)) // todo: datetime format
                .ForMember(dest => dest.AlternateCodeDescription5, opt => opt.MapFrom(src => src.EffectiveEnd)) // todo: datetime format
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Enabled == "Y" ? "A" : "I"))
                // todo: incorrect, because that should be using the validation table list
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

        _ = CreateMap<SellableItem, ResourcesModel>()
                .ForMember(dest => dest.ResourceCode, opt => opt.MapFrom(src => Helpers.ParseItemCode(src.ItemCode)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Helpers.ParseItemCode(src.ItemCode)))
                .ForMember(dest => dest.ResourceCodeDescription, opt => opt.MapFrom(src => src.NameEN))
                .ForMember(dest => dest.ResourceTypeDescription, opt => opt.MapFrom(src => src.NameEN))
                .ForMember(dest => dest.AlternateCodeDescription1, opt => opt.MapFrom(src => src.NameTC))
                .ForMember(dest => dest.AlternateCodeDescription2, opt => opt.MapFrom(src => src.NameSC))
                .ForMember(dest => dest.AlternateCodeDescription4, opt => opt.MapFrom(src => src.EffectiveStart))
                .ForMember(dest => dest.AlternateCodeDescription5, opt => opt.MapFrom(src => src.EffectiveEnd))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Enabled == "Y" ? "A" : "I"))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .IncludeBase<SellableItemBase, ResourcesModel>();

        _ = CreateMap<Package, ResourcesModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Helpers.ParseItemCode(src.PackageCode)))
                .ForMember(dest => dest.ResourceCode, opt => opt.MapFrom(src => Helpers.ParseItemCode(src.PackageCode)))
                .ForMember(dest => dest.ResourceCodeDescription, opt => opt.MapFrom(src => src.NameEN))
                .ForMember(dest => dest.ResourceTypeDescription, opt => opt.MapFrom(src => src.NameEN))
                .ForMember(dest => dest.AlternateCodeDescription1, opt => opt.MapFrom(src => src.NameTC))
                .ForMember(dest => dest.AlternateCodeDescription2, opt => opt.MapFrom(src => src.NameSC))
                .ForMember(dest => dest.AlternateCodeDescription4, opt => opt.MapFrom(src => src.EffectiveStart))
                .ForMember(dest => dest.AlternateCodeDescription5, opt => opt.MapFrom(src => src.EffectiveEnd))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Enabled == "Y" ? "A" : "I"))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .IncludeBase<SellableItemBase, ResourcesModel>();

        _ = CreateMap<Package, ResourceUDF>()
            // todo: Category -> ResourceUDF(Product Category) todo: comes from a validation table
            .ForMember(dest => dest.EV379_TXT_02, opt => opt.MapFrom(src => src.Type)) // Type -> ResourceUDF(Product Type)
            .ForMember(dest => dest.EV379_AMT_01, opt => opt.MapFrom(src => src.Duration)) // Duration -> ResourceUDF(Duration)
            .ForMember(dest => dest.EV379_TXT_03, opt => opt.MapFrom(src => src.DurationUnit)) // DurationUnit -> ResourceUDF(Duration Unit)
            .ForMember(dest => dest.EV379_TXT_01, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.EV379_ISSUE_CLASS, opt => opt.MapFrom(src => "I"))
            .ForMember(dest => dest.EV379_ISSUE_TYPE, opt => opt.MapFrom(src => "SP"));
    

        _ = CreateMap<SellableItem, ResourceUDF>()
         // todo: Category -> ResourceUDF(Prsoduct Category) todo: comes from a validation table
         .ForMember(dest => dest.EV379_TXT_02, opt => opt.MapFrom(src => src.Type)) // Type -> ResourceUDF(Product Type)
         .ForMember(dest => dest.EV379_AMT_01, opt => opt.MapFrom(src => src.Duration)) // Duration -> ResourceUDF(Duration)
         .ForMember(dest => dest.EV379_TXT_03, opt => opt.MapFrom(src => src.DurationUnit)) // DurationUnit -> ResourceUDF(Duration Unit)
         .ForMember(dest => dest.EV379_TXT_01, opt => opt.MapFrom(src => src.Category))
         .ForMember(dest => dest.EV379_ISSUE_CLASS, opt => opt.MapFrom(src => "I"))
         .ForMember(dest => dest.EV379_ISSUE_TYPE, opt => opt.MapFrom(src => "SP"));
       
    }
}
