using Integrations.HKTDC.Webforms.Configuration;
using Integrations.HKTDC.Webforms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Integrations.HKTDC.Webforms.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly CustomLinkOptions _customLinkOptions;
        private readonly IAppAccessTokenService _appAccessTokenService;
        private readonly IConfiguration _config;

        public TokenController(
            ILogger<TokenController> logger,
            IOptions<CustomLinkOptions> customLinkOptions,
            IAppAccessTokenService appAccessTokenService,
            IConfiguration config)
        {
            _logger = logger;
            _customLinkOptions = customLinkOptions.Value;
            _appAccessTokenService = appAccessTokenService;
            _config = config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string CreateAuthenticationTicket(string userName)
        {
            var schemeName = "CustomLinkAppAccessTokenAuth";

            var claims = new[]
            {
                 new Claim(schemeName, schemeName),
                 new Claim("UserName", userName),
            };

            // var identity = new ClaimsIdentity(claims, schemeName);
            // var principal = new ClaimsPrincipal(identity);

            // create JWT 
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [HttpPost("[action]")]
        public ActionResult Redirect()
        {
            #region Validate referer

            var referer = Request.GetTypedHeaders().Referer;

            if (referer == null || !_customLinkOptions.Uri.ToString().Contains(referer.Host))
            {
                return BadRequest("Invalid referer.");
            }

            #endregion

            if (!Request.Form.ContainsKey("AppAccessToken"))
            {
                return BadRequest("Missing application access token.");
            }

            var accessToken = Request.Form["AppAccessToken"];

            if (!Request.Query.ContainsKey("RedirectUrl"))
            {
                return BadRequest("Missing redirect url.");
            }

            var redirectUrl = Request.Query["RedirectUrl"];

            var query = string.Empty;

            foreach (var param in Request.Query)
            {
                if (param.Key == "AppAccessToken" ||
                    param.Key == "RedirectUrl")
                {
                    continue;
                }

                query = $"{query}&{param.Key}={param.Value}";
            }

            return Redirect($"{redirectUrl}?aat={accessToken}{query}");
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> CreateToken()
        {
            #region Validate user

            var currentUserID = Request.Query["CurrentUserID"].ToString();

            if (string.IsNullOrEmpty(currentUserID))
            {
                 return BadRequest("Missing CurrentUserID parameter.");
            }

            #endregion

            #region Validate AAT

            var accessToken = Request.Form["AppAccessToken"].ToString();

            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Missing AppAccessToken.");
            }

            try
            {
                var appAccessTokenResponse = await _appAccessTokenService.GetAppAccessTokenAsync(
                    accessToken,
                    _customLinkOptions.EncryptionKey,
                    _customLinkOptions.SecretKey);

                if (!appAccessTokenResponse.AATParameters.TryGetValue("Type", out object aatType))
                {
                    return BadRequest("Invalid AppAccessToken");
                }

                // Ensure application access token is the correct type.
                if (aatType.ToString() != "CustomLinkAppAccessToken")
                {
                    return BadRequest("Invalid AppAccessToken");
                }

                if (!appAccessTokenResponse.AATParameters.TryGetValue("CustomLinkUserID", out object customLinkUserID))
                {
                    return BadRequest("Invalid AppAccessToken");
                }

                // Ensure the provided application access token matches the user.
                if (customLinkUserID.ToString() != currentUserID)
                {
                    return BadRequest("Invalid AppAccessToken");
                }

                return Ok(CreateAuthenticationTicket(currentUserID));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            #endregion
        }
    }
}
