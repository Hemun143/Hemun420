using Integrations.HKTDC.Webforms.Domain;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Integrations.HKTDC.Webforms.Services
{
    public class AppAccessTokenService : IAppAccessTokenService
    {
        private readonly HttpClient _httpClient;

        public AppAccessTokenService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestParameters"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private static string GetRequestBody(
            Dictionary<string, object> requestParameters,
            string encryptionKey,
            string secretKey)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(requestParameters.GetType());

                serializer.WriteObject(memoryStream, requestParameters);

                var bytes = memoryStream.ToArray();

                var requestBody = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                return Crypto.Encrypt(requestBody, encryptionKey, secretKey);
            }
        }

        public async Task<AppAccessTokenResponse> GetAppAccessTokenAsync(
            string accessToken,
            string encryptionKey,
            string secretKey)
        {
            var requestParameters = new Dictionary<string, object>
            {
                { "aat", accessToken },
                { "SecretKey", secretKey }
            };

            var content = new StringContent(
                GetRequestBody(requestParameters, encryptionKey, secretKey),
                null,
                "text/plain");

            var httpResponse = await _httpClient.PostAsync(
                "api/AccountValidation/GetAppAccessTokenData",
                content);

            if (httpResponse.IsSuccessStatusCode)
            {
                var appAccessTokenResponse = await httpResponse.Content.ReadFromJsonAsync<AppAccessTokenResponse>();

                if (appAccessTokenResponse != null)
                {
                    return appAccessTokenResponse;
                }
            }

            throw new Exception("Invalid AppAccessToken");
        }
    }
}
