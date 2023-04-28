using HKTDC.DAL.Models;
using HKTDC.DAL.Repository;

namespace Integrations.HKTDC.CustomAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ResourceUDFController : ControllerBase
    {
        private readonly IResourceUDFRepository _resourceUDFRepository;

        private readonly ILogger<ResourceUDFController> _logger;

        public ResourceUDFController(ILogger<ResourceUDFController> logger, IResourceUDFRepository repository)
        {
            _logger = logger;
            _resourceUDFRepository = repository;
        }

        [HttpGet, Route("GetResourceUDFList")]
        public async Task<IEnumerable<ResourceUDF>> GetResourceUDFList([FromQuery] string organization)
        {
            return await _resourceUDFRepository.GetResourceUDFList(organization);
        }

        [HttpGet, Route("GetResourceUDF")]
        public async Task<ResourceUDF> GetResourceUDF([FromQuery] string organization, [FromQuery] int resourceSeq, [FromQuery] int seq)
        {
            return await _resourceUDFRepository.GetResourceUDF(organization, resourceSeq, seq);
        }

        [HttpPost]
        public async Task<ResourceUDF> CreateResourceUDF(ResourceUDF resourceUDF)
        {
            return await _resourceUDFRepository.CreateResourceUDF(resourceUDF);
        }

        [HttpPut]
        public async Task UpdateResourceUDF(ResourceUDF resourceUDF)
        {
            await _resourceUDFRepository.UpdateResourceUDF(resourceUDF);
        }

        [HttpDelete]
        public async Task DeleteResourceUDF([FromQuery] string organization, [FromQuery] int resourceSeq, [FromQuery] int seq)
        {
            await _resourceUDFRepository.DeleteResourceUDF(organization, resourceSeq, seq);
        }

        [HttpGet, Route("GetResourceUDFSequence")]
        public async Task<int> GetResourceUDFSequence([FromQuery] string organization)
        {
            return await _resourceUDFRepository.GetLatestResourceUDFSequence(organization);
        }

        
    }
}
