namespace Integrations.HKTDC.SPCS.OrderImport.Configuration
{
    public class ContractOrderImportOptions
    {
        public string OrganizationCode { get; set; }


        public string FuflfilmentUDFType { get; set; }

        public string FulfilmentUDFTypeClass { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public ContractStatus ContractStatus { get; set; }

        public PriceList PriceList { get; set; }
    }

    public class OrderStatus
    {
        public string ActiveStatus { get; set; }

        public string ClosedStatus { get; set; }
    }

    public class ContractStatus
    {
        public string OpenStatus { get; set; }

        public string CancelledStatus { get; set; }
    }

    public class PriceList
    {
        public string PackageHKD { get; set; }

        public string PackageUSD { get; set; }

        public string SellableHKD { get; set; }

        public string SellableUSD { get; set; }
    }

    
}
