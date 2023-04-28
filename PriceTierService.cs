using HKTDC.DAL.Models;
using HKTDC.DAL.Repository;
using HKTDC.SPCS.ResourceDeactivation.Interfaces;
using Integrations.HKTDC.SPCS.ResourceImport.Configuration;
using Microsoft.Extensions.Options;
using Ungerboeck.Api.Sdk;

namespace HKTDC.SPCS.ResourceDeactivation.Services
{
    public class PriceTierService : IPriceTierService
    {
        private readonly ApiClient _apiClient;
        private readonly ResourceImportOptions _resourceImportOptions;
        private readonly IPriceListRepository _priceTierRepository;
        public PriceTierService(ApiClient apiClient, IOptions<ResourceImportOptions> options, IPriceListRepository priceTierRepository)
        {
            _apiClient = apiClient;
            _resourceImportOptions = options.Value;
            _priceTierRepository = priceTierRepository; 
        }
        public IEnumerable<PriceList> GetPriceListTier()
        {
            var result = _priceTierRepository.GetByEffectiveDateAsync();
            foreach (var priceList in result)
            {
                priceList.Active = "I";
            }
            return result;        
        }
        public void UpdatePriceListTier()
        {
            var result = GetPriceListTier();
            if (result != null)
            {
                foreach (var priceModel in result)
                {
                   var searchResponse = _apiClient.Endpoints.PriceListItems.Search(_resourceImportOptions.OrganizationCode, $"Code  eq '{priceModel.ResourseCode}'");
                   var response = searchResponse.Results.FirstOrDefault();
                   response.Active = priceModel.Active;
                   _apiClient.Endpoints.PriceListItems.Update(response);
                }
            }

        }

       
    }
}
