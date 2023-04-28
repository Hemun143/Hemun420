namespace Integrations.HKTDC.Webforms.Configuration
{
    /// <summary>
    /// Custom link options.
    /// </summary>
    public class CustomLinkOptions
    {
        /// <summary>
        /// Requires a trailing '/'
        /// </summary>
        public Uri Uri { get; set; }

        public string EncryptionKey { get; set; }

        public string SecretKey { get; set; }
    }
}
