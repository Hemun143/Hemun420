using HKTDC.DAL.Models;
using Integrations.HKTDC.SPCS.Domain;
using Integrations.HKTDC.SPCS.Mapping;
using Integrations.HKTDC.SPCS.ResourceImport.Configuration;
using Integrations.HKTDC.SPCS.ResourceImport.Helper;
using Integrations.HKTDC.SPCS.ResourceImport.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ungerboeck.Api.Models.Subjects;
using Ungerboeck.Api.Sdk;

namespace Integrations.HKTDC.SPCS.ResourceImport.Services
{
    public class ResourceService : IResourceService
    {
        private readonly ApiClient _apiClient;
        private readonly ResourceImportOptions _resourceImportOptions;
        private const string ResourceUDFApiPath = "ResourceUDF";
        private const string ResourceUDFListApiPath = "ResourceUDF/GetResourceUDFSequence";
        private readonly ApiHelper _apiHelper;
        public ResourceService(ApiClient apiClient, IOptions<ResourceImportOptions> options, ApiHelper apiHelper)
        {
            _apiClient = apiClient;
            _resourceImportOptions = options.Value;
            _apiHelper = apiHelper;
        }

        public async Task<bool> CreateResourceUDF(ResourceUDF resourceUDF, string resourceCode)
        {
            var resource = GetResource(resourceCode);
            var resourceSequence = resource != null && resource.Sequence.HasValue ? (int)resource.Sequence : new Random().Next(0000, 9999);
            resourceUDF.ResourceSequence = resourceSequence;
            resourceUDF.Organization = _resourceImportOptions.OrganizationCode;
            var sequenceNumber = await GetLatestResourceUDFSequence();
            resourceUDF.Sequence = sequenceNumber.HasValue ? (int)sequenceNumber : new Random().Next(0000, 9999);// To be discussed
            var httpResponseMessage = await _apiHelper.PostAsync(resourceUDF, ResourceUDFApiPath);
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public ResourceItem GetResourceFileContent()
        {
            var allFiles = Directory.GetFiles(_resourceImportOptions.ResourceImportPath, "*.*");
            var resourceItem = new ResourceItem();
            foreach (var item in allFiles)
            {
                using StreamReader reader = new StreamReader(item);
                var fileData = reader.ReadToEnd();
                if (fileData.Contains("packageCode"))
                {
                    resourceItem.Packages = JsonConvert.DeserializeObject<IEnumerable<Package>>(fileData);
                }
                else if (fileData.Contains("itemCode"))
                {
                    resourceItem.SellableItems = JsonConvert.DeserializeObject<IEnumerable<SellableItem>>(fileData);
                }
                reader.Close();
                var fileToArchive = Path.GetFileName(item);
                File.Move(item, $"{_resourceImportOptions.ArchiveResourceImportPath}//{fileToArchive}");
            }
            return resourceItem;
        }

        public IEnumerable<ValidationEntriesModel> GetValidationSettings()
        {
            var response = _apiClient.Endpoints.ValidationEntries.Search(_resourceImportOptions.OrganizationCode, _resourceImportOptions.OrganizationCodeQuery);
            return response.Results;
        }

        public ResourcesModel CreateResource(ResourcesModel resourcesModel)
        {
            resourcesModel.Organization = _resourceImportOptions.OrganizationCode;
            resourcesModel.UM = "EA";
            resourcesModel.UT = "EA";
            return _apiClient.Endpoints.Resources.Add(resourcesModel);
        }

        public void CreatePriceList(IEnumerable<PriceListModel> priceListModels)
        {
            foreach (var priceListModel in priceListModels)
            {
                _apiClient.Endpoints.PriceList.Add(priceListModel);
            }
        }

        public ResourcesModel GetResource(string resourceCode)
        {
            resourceCode = Helpers.ParseItemCode(resourceCode);
            var result = _apiClient.Endpoints.Resources.Search(_resourceImportOptions.OrganizationCode, $"ResourceCode eq '{resourceCode}'");
            var resource = result.Results;
            if (resource.Any())
            {
                return resource.FirstOrDefault();
            }
            return null;
        }

        public async Task<int?> GetLatestResourceUDFSequence()
        {
            var requestValue = $"?organization={_resourceImportOptions.OrganizationCode}";
            var response = await _apiHelper.GetAsync(requestValue, ResourceUDFListApiPath);
            if (response.Item1)
            {
                if (!string.IsNullOrWhiteSpace(response.Item2))
                {
                    return int.Parse(response.Item2) + 1;
                }
                else
                {
                    return 1;
                }
            }
            return null;
        }
        
    }
}
