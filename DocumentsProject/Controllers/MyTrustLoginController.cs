using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Claims;
using DocumentsProject.Web.Helper;
using DocumentsProject.Core.DTOs;

namespace DocumentsProject.Web.Controllers
{
    public class MyTrustLoginController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly OpenID openIDHelper;
        private readonly ILogger<MyTrustLoginController> _logger;

        public MyTrustLoginController(IConfiguration configuration,
            ILogger<MyTrustLoginController> logger)
        {
            _configuration = configuration;
            openIDHelper = new OpenID(_configuration);
            _logger = logger;
        }

        public IActionResult Login()
        {
            try
            {
                /* Verify user is authenticated or not
                  1-:  if user not authenticated create IDP login url
                       with service provider details
                  2-:  if user is authenticated redirect to home page
                 */
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    _logger.LogInformation("Login : Redirect for IDP Login");

                    /* Generate nonce and state value and store in session or somewhere else
                     to validate authorization and token endpoint responce from idp*/
                    var state = Guid.NewGuid().ToString("N");
                    var nonce = Guid.NewGuid().ToString("N");

                    HttpContext.Session.SetString("Nonce", nonce);
                    HttpContext.Session.SetString("state", state);

                    //generate IDP login url
                    return Redirect(openIDHelper.GetAuthorizationUrl(nonce, state));
                }

                return RedirectToAction("Index", "Home");

            }
            catch (Exception e)
            {
                _logger.LogError("Login Exception :{0}", e.Message);
                ViewBag.error = "Something Went wrong";
                ViewBag.error_description = e.Message;
                return View("CustomError");
            }
        }

        public IActionResult Logout()
        {
            try
            {
                /* Generate state value and store in session or somewhere else
                   to validate logout responce from idp*/
                var state = Guid.NewGuid().ToString("N");

                HttpContext.Session.Remove("Nonce");
                HttpContext.Session.SetString("state", state);

                /* get id_token from session which we get from token endpoint at the 
                 * time of login. this id_token we need to pass in logout url for
                 * validate service provider logout request at idp side
                 */
                var idToken = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "ID_Token").Value;

                _logger.LogInformation("Logout : Redirect for IDP Logout");
                //generate idp logout url
                return Redirect(openIDHelper.GetLogoutUrl(idToken, state));
            }
            catch (Exception e)
            {
                _logger.LogError("Logout Exception : {0}", e.Message);
                ViewBag.error = "Something Went Wrong!";
                ViewBag.error_description = e.Message;
                return View("CustomError");
            }
        }

        [Route("callback")]
        public async Task<IActionResult> OpenIDLoginAsync()
        {
            try
            {
                _logger.LogInformation("OpenIDLogin:Get Authorization Responce form IDP");

                var isOpenId = _configuration.GetValue<bool>("UgPassAuth_Connect");

                // Check IDP responce where we get some error from idp or not
                if (!string.IsNullOrEmpty(Request.Query["error"]) && !string.
                    IsNullOrEmpty(Request.Query["error_description"]))
                {
                    ViewBag.error = Request.Query["error"].ToString();
                    ViewBag.error_description = Request.Query["error_description"]
                        .ToString();
                    return View("CustomError");
                }

                //get authorization code from idp responce
                string code = Request.Query["code"].ToString();
                if (string.IsNullOrEmpty(code))
                {
                    ViewBag.error = "Invalid code";
                    ViewBag.error_description = "The code value is empty string or null";
                    return View("CustomError");
                }

                JObject TokenResponse = null;
                if (isOpenId)
                {
                    //get acces token object by passing authorization code to idp using UgPassAuthorization
                    TokenResponse = openIDHelper.GetAccessTokenWithoutUgHub(code).Result;
                    //check where we get error in token endpoint or not
                    if (TokenResponse.ContainsKey("error") && TokenResponse.
                        ContainsKey("error_description"))
                    {
                        ViewBag.error = TokenResponse["error"].ToString();
                        ViewBag.error_description = TokenResponse["error_description"]
                            .ToString();
                        return View("CustomError");
                    }
                }
                else
                {
                    //get acces token object by passing authorization code to idp without using UgPassAuthorization
                    TokenResponse = openIDHelper.GetAccessToken(code).Result;
                    //check where we get error in token endpoint or not
                    if (TokenResponse.ContainsKey("error") && TokenResponse.
                        ContainsKey("error_description"))
                    {
                        ViewBag.error = TokenResponse["error"].ToString();
                        ViewBag.error_description = TokenResponse["error_description"]
                            .ToString();
                        return View("CustomError");
                    }
                }


                _logger.LogInformation("OpenIDLogin  : Get Token Endpoint Responce form IDP success");
                //get id_token from idp token responce
                var ID_Token = TokenResponse["id_token"].ToString();
                if (string.IsNullOrEmpty(ID_Token))
                {
                    ViewBag.error = "Invalid code";
                    ViewBag.error_description = "The ID_Token value is empty string or null";
                    return View("CustomError");
                }


                //get access token from idp token endpoint responce
                var accessToken = TokenResponse["access_token"].ToString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    ViewBag.error = "Invalid responce";
                    ViewBag.error_description = "The access_token value is empty " +
                        "string or null";
                    return View("CustomError");
                }


                UserObj user = null;

                if (!isOpenId)
                {

                    //validate id_token and get cliam values from  id_token
                    ClaimsPrincipal userObj = openIDHelper.ValidateIdentityToken(ID_Token)
                        .Result;
                    if (userObj == null)
                    {
                        ViewBag.error = "Something went wrong ";
                        ViewBag.error_description = "Claim Object getting null value";
                        return View("CustomError");
                    }
                    _logger.LogInformation("OpenIDLogin  : Validate Id_token success");

                    //get user details from idtoken claim
                    var daesClaim = userObj.FindFirst("daes_claims")?.Value ?? "";
                    user = JsonConvert.DeserializeObject<UserObj>(daesClaim);
                    user.sub = userObj.FindFirst("sub")?.Value ?? "";

                }
                else
                {
                    //code for oauth
                    JObject userObj = await openIDHelper.GetUserInfo(accessToken);
                    if (userObj.ContainsKey("error") && userObj.ContainsKey("error_description"))
                    {
                        ViewBag.error = userObj["error"].ToString();
                        ViewBag.error_description = userObj["error_description"].ToString();
                        return View("CustomError");
                    }

                    _logger.LogInformation("OpenIDLogin  : get userinfo from idp successfully");
                    JObject daesClaims = (JObject)userObj["daes_claims"];
                    user = JsonConvert.DeserializeObject<UserObj>(userObj.ToString());
                    user.suid = daesClaims["suid"]?.ToString();
                    user.birthdate = daesClaims["birthdate"]?.ToString();
                    user.name = daesClaims["name"]?.ToString();
                    user.phone = daesClaims["phone"]?.ToString();
                    user.email = daesClaims["email"]?.ToString();
                    user.gender = daesClaims["gender"]?.ToString();
                    user.id_document_type = daesClaims["id_document_type"]?.ToString();
                    user.id_document_number = daesClaims["id_document_number"]?.ToString();
                    user.loa = daesClaims["loa"]?.ToString();

                }

                //set cliam values for session
                var identity = new ClaimsIdentity(new[] {
                    new Claim("Access_Token", accessToken),
                    new Claim("ID_Token", ID_Token),
                    new Claim(ClaimTypes.Name, user.name),
                    new Claim(ClaimTypes.NameIdentifier, user.suid),
                    new Claim(ClaimTypes.Email,user.email),
                    new Claim(ClaimTypes.UserData,user.sub),
                    new Claim("UserDetails",JsonConvert.SerializeObject(user)),
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                //set option for authentication cookies
                var principal = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties();
                properties.IsPersistent = true;
                properties.AllowRefresh = false;

                properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(30));
                //create authentication cookies
                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults
                    .AuthenticationScheme, principal, properties);
                _logger.LogInformation("OpenIDLogin : User login success");
                _logger.LogInformation("OpenIDLogin : redirct to home page");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                _logger.LogError("OpenIDLogin Exception: {0}", e.Message);
                ViewBag.error = "Something Went Wrong!";
                ViewBag.error_description = e.Message;
                return View("CustomError");
            }
        }

        [Route("signout")]
        public IActionResult OpenIDLogout()
        {
            try
            {
                _logger.LogInformation("OpenIDLogout : Get responce from idp logout");
                //clear session and authentication cookies
                var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults
                            .AuthenticationScheme);
                HttpContext.Session.Clear();
                _logger.LogInformation("OpenIDLogout : redirct to home page");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                _logger.LogError("OpenIDLogout Exception : {0}", e.Message);
                ViewBag.error = "Something Went Wrong!";
                ViewBag.error_description = e.Message;
                return View("CustomError");
            }
        }
    }
}
