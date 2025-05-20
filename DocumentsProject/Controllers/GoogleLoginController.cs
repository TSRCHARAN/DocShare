using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web;
using System.Text.Json;
using DocumentsProject.Models;
using DocumentsProject.Core.Domain.Services;

namespace DocumentsProject.Web.Controllers
{
    public class GoogleLoginController : Controller
    {
        private readonly string _googleClientId;
        private readonly string _googleClientSecret;
        private readonly string _googleRedirectUri;
        private readonly ILoginService _loginService;

        public GoogleLoginController(IConfiguration configuration, ILoginService loginService)
        {
            _googleClientId = configuration["GoogleAuthSettings:ClientId"];
            _googleClientSecret = configuration["GoogleAuthSettings:ClientSecret"];
            _googleRedirectUri = configuration["GoogleAuthSettings:RedirectUri"];

            _loginService = loginService;
        }
        public IActionResult Index()
        {
            var queryParams = new Dictionary<string, string>
            {
                {"client_id", _googleClientId},
                {"redirect_uri", _googleRedirectUri},
                {"response_type", "code"},
                {
                    "scope", string.Join(" ", new[]
                    {
                        "openid",
                        "email",
                        "profile",
                    })
                },
                {"access_type", "offline"}
            };

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";

            return Redirect(googleAuthUrl);
        }

        public async Task<IActionResult> GoogleCallback(string code)
        {
            using var httpClient = new HttpClient();

            // Exchange the authorization code for an access token
            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", _googleClientId),
                new KeyValuePair<string, string>("client_secret", _googleClientSecret),
                new KeyValuePair<string, string>("redirect_uri", _googleRedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            }));

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenContent);

            // Use the access token to get the user's info
            var userInfoResponse = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={tokenData.access_token}");
            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);

            // Create user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.id),
                new Claim(ClaimTypes.Name, userInfo.name),
                new Claim(ClaimTypes.Email, userInfo.email),
                new Claim("urn:google:picture", userInfo.picture),
                //new Claim("urn:google:locale", userInfo.locale),
                //new Claim("AccessToken", tokenData.access_token)
            };

            // Create claims identity
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Create claims principal
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user with cookie authentication
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            // Add User if doesnt exist
            var createUser = await _loginService.AddUser(userInfo.email, userInfo.name);
            //if (createUser == false)
            //{
            //    return Json(new { success = false, message = createUser.Message });
            //}
            //return Json(new { success = true, message = createUser.Message });

            // Redirect to a secure page or return the user info
            //return Redirect("/securepage");
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> GoogleLogout()
        {
            // Get the access token from the user's claims
            var accessToken = User.FindFirst("AccessToken")?.Value;

            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("No access token found.");
            }

            // Revoke the access token
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync($"https://oauth2.googleapis.com/revoke?token={accessToken}", null);

            if (response.IsSuccessStatusCode)
            {
                // Sign out the user and clear the authentication cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Redirect the user to the homepage or a specific page after logout
                return RedirectToAction("Index", "Home");
            }

            return StatusCode((int)response.StatusCode, "Error revoking token.");
        }
    }
}
