namespace Integrations.HKTDC.SPCS.ResourceImport.Configuration
{
    public class ResourceImportOptions
    {
        public string ResourceImportPath { get; set; }
        public string OrganizationCode { get; set; }
        public string OrganizationCodeQuery { get; set; }
        public string ArchiveResourceImportPath { get; set; }
        public string ResourceCustomApi { get; set; }
        public string EncryptedAuthentication { get; set; }
        public string SellableUSD { get; set; }
        public string SellableHKD { get; set; }
        public string PackageUSD { get; set; }
        public string PackageHKD { get; set; }
    }
}
