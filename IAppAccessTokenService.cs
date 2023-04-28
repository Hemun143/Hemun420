using Integrations.HKTDC.Webforms.Domain;

namespace Integrations.HKTDC.Webforms.Services
{
    /// <summary>
    /// App access token service contract.
    /// </summary>
    public interface IAppAccessTokenService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        Task<AppAccessTokenResponse> GetAppAccessTokenAsync(string accessToken, string encryptionKey, string secretKey);
    }
}
