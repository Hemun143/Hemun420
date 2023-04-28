using HKTDC.DAL.Models;
using HKTDC.DAL.Repository;
using Integrations.HKTDC.SPCS.Mapping;
using Ungerboeck.Api.Sdk;

namespace Integrations.HKTDC.SPCS.Services
{
    public class SellableItemService : ISellableItemService
    {
        private readonly ILogger<SellableItemService> _logger;
        private readonly IMapper _mapper;
        private readonly ApiClient _client;
        private readonly IResourceUDFRepository _resourceUDFRepository;

        public SellableItemService(
            ILogger<SellableItemService> logger,
            IMapper mapper,
            ApiClient apiClient,
            IResourceUDFRepository resourceUDFRepository
            )
        {
            _logger = logger;
            _mapper = mapper;
            _client = apiClient;
            _resourceUDFRepository = resourceUDFRepository;
        }

        public Task<IEnumerable<SellableItem>> GetItemsAsync(DateTime changedSince)
        {
            X();
            Y();
            var x = new List<SellableItem>();
            return Task.FromResult<IEnumerable<SellableItem>>(x);
        }

        private void X()
        {
            _logger.LogInformation("Doing X!");

            // Get all modified sellable items since X
            var sellableItems = new List<SellableItem>();

            foreach (var sellableItem in sellableItems)
            {
                if (sellableItem.PriceTiers != null)
                {
                    // resources are made from the tier
                    foreach (var priceTier in sellableItem.PriceTiers)
                    {
                        var resource = _mapper.Map<ResourcesModel>(sellableItem);

                        resource.ResourceCode = $"{Helpers.ParseItemCode(sellableItem.ItemCode)}{Helpers.ParseTierCode(priceTier.TierCode)}";

                        var priceListItemHKD = _mapper.Map<PriceListItemsModel>(priceTier);

                        priceListItemHKD.BasePrice = priceTier.HKD;
                        priceListItemHKD.Code = resource.ResourceCode;

                        // todo: incorrect, needs to come from mapping table
                        priceListItemHKD.PriceList = "SellableHKD";

                        var priceListItemUSD = _mapper.Map<PriceListItemsModel>(priceTier);

                        priceListItemUSD.BasePrice = priceTier.USD;
                        priceListItemUSD.Code = resource.ResourceCode;


                        // todo: incorrect, needs to come from mapping table
                        priceListItemUSD.PriceList = "SellableUSD";
                    }
                }
            }
        }

        private void Y()
        {
            _logger.LogInformation("Doing Y!");

            var packages = new List<Package>();

            // Get category validation table entries
            // todo: don't hardcode 61, put in config.
            // todo: this is where you would get the validation table entries for validation table 61 (SPCS Product Category)

            // https://supportwebap.ungerboeck.com/Hktdc/Dev/api/v1/ValidationEntries/01?search=ValidationTableID%20eq%2061
            var categoryMap = new List<ValidationEntriesModel>();

            foreach (var package in packages)
            {
                // package code remove 0 add 5? why?
                if (package.PriceTiers != null)
                {
                    foreach (var priceTier in package.PriceTiers)
                    {
                        if (priceTier.SellableItems != null)
                        {
                            // resources are made from the sellable items.
                            // if level = package, do we even care, it's same as if there are multiple?
                            foreach (var sellableItem in priceTier.SellableItems)
                            {
                                var resource = _mapper.Map<ResourcesModel>(package);

                                var userFields = _mapper.Map<UserFields>(package);

                                // todo: get the category, parse this to automapper?
                                userFields.UserText01 = categoryMap.SingleOrDefault(x => x.AlternateDescription1 == package.Category).Code;

                                resource.ResourceCode = $"{Helpers.ParseItemCode(package.PackageCode)}{Helpers.ParseTierCode(priceTier.TierCode)}";

                                var priceListItemHKD = _mapper.Map<PriceListItemsModel>(priceTier);

                                priceListItemHKD.BasePrice = sellableItem.HKD;
                                priceListItemHKD.Code = resource.ResourceCode;
                                priceListItemHKD.PriceList = "SellableHKD";
                                priceListItemHKD.MinimumQuantity = sellableItem.Quantity; // override automapper

                                var priceListItemUSD = _mapper.Map<PriceListItemsModel>(priceTier);

                                priceListItemUSD.BasePrice = sellableItem.USD;
                                priceListItemUSD.Code = resource.ResourceCode;
                                priceListItemUSD.PriceList = "SellableUSD";
                                priceListItemUSD.MinimumQuantity = sellableItem.Quantity; // override automapper
                            }
                        }
                    }
                }
            }
        }

        public async Task ImportSellableItem(SellableItem sellableItem)
        {
            ResourcesModel resource = _mapper.Map<ResourcesModel>(sellableItem);
            var resources = _client.Endpoints.Resources.Search("01", $"ResourceCode eq '{resource.ResourceCode}'").Results?.ToList();
            if (resources != null && resources.Count() > 0)
            {
                resource = _mapper.Map(resources[0], resource);
                _client.Endpoints.Resources.Update(resource);
            }
            else
            {
                resource.Organization = "01";
                resource.UM = "EA";
                resource.UT = "EA";
                _client.Endpoints.Resources.Add(resource);
                var addedResource = _client.Endpoints.Resources.Search("01", $"ResourceCode eq '{resource.ResourceCode}'").Results?.ToList();
                if(addedResource != null && addedResource.Count() > 0)
                {
                    resource = addedResource.First();
                }
            }

            ResourceUDF resourceUdf = await _resourceUDFRepository.GetResourceUDF("01", resource.Sequence.Value);
            if(resourceUdf != null)
            {
                resourceUdf.EV379_TXT_01 = sellableItem.Category ?? "";
                resourceUdf.EV379_TXT_02 = sellableItem.Type ?? "";
                resourceUdf.EV379_TXT_03 = Convert.ToString(sellableItem.Duration);
                resourceUdf.EV379_TXT_04 = sellableItem.DurationUnit ?? "";
                await _resourceUDFRepository.UpdateResourceUDF(resourceUdf);  
            }
            else
            {
                resourceUdf = new ResourceUDF();
                resourceUdf.Organization = "01";
                resourceUdf.ResourceSequence = resource.Sequence ?? 0;
                resourceUdf.Sequence = resource.Sequence ?? 0;
                resourceUdf.EV379_TXT_01 = sellableItem.Category ?? "";
                resourceUdf.EV379_TXT_02 = sellableItem.Type ?? "";
                resourceUdf.EV379_TXT_03 = Convert.ToString(sellableItem.Duration);
                resourceUdf.EV379_TXT_04 = sellableItem.DurationUnit ?? "";
                await _resourceUDFRepository.CreateResourceUDF(resourceUdf);
            }
            List<PriceListItemsModel> priceListModels = _mapper.Map<List<PriceListItemsModel>>(sellableItem.PriceTiers);
            foreach(PriceListItemsModel priceModel in priceListModels)
            {
                _client.Endpoints.PriceList.Search("10", "");
            }

            // todo: check if the resource is on a pricelist, if not add it.

        }
    }
}

// todo: minor is SPCS
// todo: move this logic to HKTDC.SPCS.ResourceImport