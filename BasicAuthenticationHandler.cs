using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Integrations.HKTDC.CustomAPI.Handlers
{
    /// <summary>
    /// Basic authentication handler.
    /// </summary>
    public class BasicAuthenticationHandler : AuthenticationHandler<CustomAPIOptions>
    {
        private readonly CustomAPIOptions _options;

        public BasicAuthenticationHandler(
            IOptionsMonitor<CustomAPIOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _options = options.CurrentValue;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                var username = credentials[0];
                var password = credentials[1];

                var authKey = _options.EncryptedAuthentication;
                var loginCredentialBytes = Convert.FromBase64String(authKey);
                var loginCredentials = Encoding.UTF8.GetString(loginCredentialBytes).Split(':', 2);
                var auth = loginCredentials[0] == username && loginCredentials[1] == password;

                if (!auth)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
                }

                var claimsIdentity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    },
                    Scheme.Name);

                var authenticationTicket = new AuthenticationTicket(
                    new ClaimsPrincipal(claimsIdentity), Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }
    }
}
