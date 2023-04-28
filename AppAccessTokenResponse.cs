namespace Integrations.HKTDC.Webforms.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public class AppAccessTokenResponse
    {
        public int? Sequence { get; set; }

        public int? StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public Dictionary<string, object> AATParameters { get; set; }

    }
}
