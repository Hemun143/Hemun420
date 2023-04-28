using System.ComponentModel.DataAnnotations;

namespace Integrations.HKTDC.Webforms.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Order
    {
        public string EBMSContractId { get; set; }
        public string SPCSContractId { get; set; }
        public string OrderNumber { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; }
        public string Currency { get; set; }
        public decimal Total { get; set; }
        public decimal NetDue { get; set; }
    }
}
