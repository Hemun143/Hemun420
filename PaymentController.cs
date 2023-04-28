using HKTDC.DAL.Models;

namespace Integrations.HKTDC.CustomAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger _logger;

        public PaymentController(ILogger<PaymentController> logger)
        {
            _logger = logger;
        }

        [HttpGet, Route("GetPayments")]
        public async Task<IEnumerable<PaymentExport>> GetPayments([FromQuery] string organization, DateTime lastUpdated)
        {
            // todo: Get this from the custom data source.
            // todo: this is relevant to REQ-INT-005
            return null;
        }
    }
}
