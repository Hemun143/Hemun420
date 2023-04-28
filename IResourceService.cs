using HKTDC.DAL.Models;
using Integrations.HKTDC.SPCS.ResourceImport.Model;
using Ungerboeck.Api.Models.Subjects;

namespace Integrations.HKTDC.SPCS.ResourceImport.Services
{
    public interface IResourceService
    {
        IEnumerable<ValidationEntriesModel> GetValidationSettings();
        ResourceItem GetResourceFileContent();
        Task<bool> CreateResourceUDF(ResourceUDF resourceUDF, string resourceCode);
        ResourcesModel CreateResource(ResourcesModel resourcesModel);
        void CreatePriceList(IEnumerable<PriceListModel> priceListModels);
        ResourcesModel GetResource(string resourceCode);
        Task<int?> GetLatestResourceUDFSequence();
    }
}
