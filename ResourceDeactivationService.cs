using HKTDC.DAL.Models;
using HKTDC.DAL.Repository;
using Integrations.HKTDC.SPCS.ResourceImport.Configuration;
using Microsoft.Extensions.Options;
using Ungerboeck.Api.Sdk;

namespace HKTDC.SPCS.ResourceDeactivation.Services
{
    public class ResourceDeactivationService : IResourceDeactivationService
    {
        private readonly ApiClient _apiClient;
        private readonly ResourceImportOptions _resourceImportOptions;
        private readonly IResourceRepository _resourceRepository;
        public ResourceDeactivationService(ApiClient apiClient, IOptions<ResourceImportOptions> options, IResourceRepository resourceRepository)
        {
            _apiClient = apiClient;
            _resourceImportOptions = options.Value;
            _resourceRepository = resourceRepository;
        }
        public IEnumerable<Resource> GetResourceDeactivationService()
        {
            var result = _resourceRepository.GetByEffectiveDate();
            foreach (var resource in result)
            {
                resource.Status = "I";
            }
            return result;
        }
        public void UpdateResourceDeactivation()
        {
            var result = GetResourceDeactivationService(); ;

            if (result != null)
            {
                foreach (var resourcesModel in result)
                {
                    var resource = resourcesModel.ResourceCode;
                    var searchPriceList = _apiClient.Endpoints.PriceListItems.Search(_resourceImportOptions.OrganizationCode, $"Code eq '{resource}'");
                    if (searchPriceList.Results.Any())
                    {
                        foreach (var priceListItem in searchPriceList.Results)
                        {
                            priceListItem.Active = resourcesModel.Status;
                            _apiClient.Endpoints.PriceListItems.Update(priceListItem);
                        }
                    }
                   _  = _resourceRepository.UpdateByEffectiveDate(resourcesModel.OrganizationCode,
                                             resourcesModel.ResourceCode, resourcesModel.Status);                     
                    
                }
            } 
        }

    }
}


