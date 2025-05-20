using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using DocumentsProject.Core.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace DocumentsProject.Web.Helper
{
    public class OpenID
    {
        public IConfiguration configuration;
        public OpenID(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public string GetAuthorizationUrl(string nonce, string state)
        {
            try
            {
                /*Prepare jwtToken object to generate jwt token which is send form
                 request parameter in query string*/
                var requestObject = new JWTokenDTO();
                requestObject.Expiry = 60;
                requestObject.Audience = configuration["dtidp:Issuer"];
                requestObject.Issuer = configuration["dtidp:ClientId"];
                requestObject.ResponseType = "code";
                requestObject.RedirecUri = configuration["dtidp:RedirectUri"];
                requestObject.Scope = configuration["dtidp:Scopes"];
                requestObject.State = state;
                requestObject.Nonce = nonce;

                //generate jwt token by passing jwttoken object details
                var response = JWTTokenManager.GenerateJWTToken(requestObject);
                if (null == response)
                {
                    throw new Exception("Fail to generate JWT token for " +
                        "Authorization request.");
                }

                //generate idp login url using ClientId,ClientId,Scopes,state,nonce,request
                //check all values in appsettings.Development.json file
                var authorizationURl = configuration.
                    GetValue<string>("dtidp:AuthorizationEndpoint") +
                    "?client_id={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}&" +
                    "nonce={4}&request={5}";

                return String.Format(authorizationURl,
                                     configuration.GetValue<string>("dtidp:ClientId"),
                                     configuration.GetValue<string>("dtidp:RedirectUri"),
                                     configuration.GetValue<string>("dtidp:Scopes"),
                                     state, nonce, response);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetLogoutUrl(string idToken, string state)
        {
            try
            {
                var LogoutURl = configuration.GetValue<string>("dtidp:EndSessionEndpoint") +
                    "?id_token_hint={0}&post_logout_redirect_uri={1}&state={2}";

                //generate idp logout url using id_token,PostLogoutRedirectUri and state value
                return String.Format(LogoutURl, idToken,
                             configuration.GetValue<string>("dtidp:PostLogoutRedirectUri"),
                             state);
            }
            catch (Exception)
            {
                throw;
            }
        }


        //code for get access token without ughub authorization....
        public async Task<JObject> GetAccessToken(string code)
        {
            try
            {
                //get token endpoint url from appsetting.Development.json file
                var TokenUrl = configuration.GetValue<string>("dtidp:TokenEndpoint");

                //set client assertion type
                var ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

                /*Prepare jwtToken object to generate jwt token which is send form
                  client_assertion parameter in query string*/
                var requestObject = new JWTokenDTO();
                requestObject.Expiry = 60;
                requestObject.Audience = configuration["dtidp:TokenEndpoint"];
                requestObject.Issuer = configuration["dtidp:ClientId"];
                requestObject.Subject = configuration["dtidp:ClientId"];

                var Jwtresponse = JWTTokenManager.GenerateJWTToken(requestObject);
                if (null == Jwtresponse)
                {
                    throw new Exception("Fail to generate JWT token for Token request.");
                }
                var ClientAssertion = Jwtresponse;

                //prepare data object which is send with token endpoint url 
                var data = new Dictionary<string, string>
                {
                   { "code", code },
                   { "client_id", configuration.GetValue<string>("dtidp:ClientId") },
                   { "redirect_uri", configuration.GetValue<string>("dtidp:RedirectUri") },
                   { "grant_type", "authorization_code" },
                   { "client_assertion_type", ClientAssertionType},
                   { "client_assertion", ClientAssertion}
                };

                //convert data object in url encoded form
                var content = new FormUrlEncodedContent(data);

                //call tokent endpoint
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message,
                    cert,
                    chain,
                    sslPolicyErrors) =>
                {
                    return true;
                };
                var client = new HttpClient(httpClientHandler);
                client.BaseAddress = new Uri(TokenUrl);

                var response = await client.PostAsync(TokenUrl, content);
                if (response == null)
                {
                    throw new Exception("GetAccessToken responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(responseString);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        //code for get access token with ughub authorization....
        public async Task<JObject> GetAccessTokenUgPassFlow(string code)
        {
            try
            {
                var TokenUrl1 = configuration.GetValue<string>("dtidp:UgPassTokenUrl");

                //prepare data object which is send with token endpoint url 
                var data = new Dictionary<string, string>
                {
                   { "grant_type", "client_credentials" }
                };
                //call tokent endpoint
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message,
                    cert,
                    chain,
                    sslPolicyErrors) =>
                {
                    return true;
                };

                //convert data object in url encoded form
                var content1 = new FormUrlEncodedContent(data);

                var client = new HttpClient(httpClientHandler);

                client.DefaultRequestHeaders.Clear();
                var authToken = Encoding.ASCII.GetBytes($"{configuration.GetValue<string>("Config:IDP_Config:UgPassClientId")}:{configuration.GetValue<string>("Config:IDP_Config:UgPassClientSecret")}");
                var authzHeader = "Basic  " + Convert.ToBase64String(authToken);
                client.DefaultRequestHeaders.Add("Authorization", "Basic " +
                    "VEFRS1BmU2F3R29Ja09ST2k3SU5LU3hUQ244YTptREM2aGt3YWdYaEFvRnpQYm1udWhSZks2YXNh");

                client.BaseAddress = new Uri(TokenUrl1);

                var response = await client.PostAsync(TokenUrl1, content1);
                if (response == null)
                {
                    throw new Exception("GetAccessToken responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }

                var responseString1 = await response.Content.ReadAsStringAsync();

                JObject TokenResponse = JObject.Parse(responseString1);
                var accessToken = TokenResponse["access_token"].ToString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    dynamic error = new JObject();
                    error.error = "Invalid responce";
                    error.error_description = "The access_token value is empty " +
                        "string or null";
                    return error;
                }

                //================================================================
                //get token endpoint url from appsetting.Development.json file
                var TokenUrl = configuration.GetValue<string>("dtidp:UgHubBaseUrl");

                //set client assertion type
                var ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

                /*Prepare jwtToken object to generate jwt token which is send form
                  client_assertion parameter in query string*/
                var requestObject = new JWTokenDTO();
                requestObject.Expiry = 60;
                requestObject.Audience = configuration["dtidp:TokenEndpoint"];
                requestObject.Issuer = configuration["dtidp:ClientId"];
                requestObject.Subject = configuration["dtidp:ClientId"];

                var Jwtresponse = JWTTokenManager.GenerateJWTToken(requestObject);
                if (null == Jwtresponse)
                {
                    throw new Exception("Fail to generate JWT token for Token request.");
                }
                var ClientAssertion = Jwtresponse;

                //prepare data object which is send with token endpoint url 
                var data1 = new Dictionary<string, string>
                {
                   { "code", code },
                   { "client_id", configuration.GetValue<string>("dtidp:ClientId") },
                   { "redirect_uri", configuration.GetValue<string>("dtidp:RedirectUri") },
                   { "grant_type", "authorization_code" },
                   { "client_assertion_type", ClientAssertionType},
                   { "client_assertion", ClientAssertion}
                };

                //convert data object in url encoded form
                var content = new FormUrlEncodedContent(data1);


                var client1 = new HttpClient(httpClientHandler);
                client1.DefaultRequestHeaders.Add("Authorization", "Bearer " +
                    accessToken);
                client1.BaseAddress = new Uri(TokenUrl);

                var response1 = await client1.PostAsync(TokenUrl, content);
                if (response1 == null)
                {
                    throw new Exception("GetAccessToken responce getting null");
                }
                if (!response1.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response1.StatusCode;
                    error.error_description = response1.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response1.Content.ReadAsStringAsync();

                    return JObject.Parse(responseString);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<JObject> GetAccessTokenWithoutUgHub(string code)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message,
                cert,
                chain,
                sslPolicyErrors) =>
            {
                return true;
            };
            var TokenUrl = configuration.GetValue<string>("dtidp:UgPassBaseUrl");

            //set client assertion type
            var ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

            /*Prepare jwtToken object to generate jwt token which is send form
              client_assertion parameter in query string*/
            var requestObject = new JWTokenDTO();
            requestObject.Expiry = 60;
            requestObject.Audience = configuration["dtidp:TokenEndpoint"];
            requestObject.Issuer = configuration["dtidp:ClientId"];
            requestObject.Subject = configuration["dtidp:ClientId"];

            var Jwtresponse = JWTTokenManager.GenerateJWTToken(requestObject);
            if (null == Jwtresponse)
            {
                throw new Exception("Fail to generate JWT token for Token request.");
            }
            var ClientAssertion = Jwtresponse;

            //prepare data object which is send with token endpoint url 
            var data1 = new Dictionary<string, string>
                {
                   { "code", code },
                   { "client_id", configuration.GetValue<string>("dtidp:ClientId") },
                   { "redirect_uri", configuration.GetValue<string>("dtidp:RedirectUri") },
                   { "grant_type", "authorization_code" },
                   { "client_assertion_type", ClientAssertionType},
                   { "client_assertion", ClientAssertion}
                };

            //convert data object in url encoded form
            var content = new FormUrlEncodedContent(data1);


            var client1 = new HttpClient(httpClientHandler);
            //client1.DefaultRequestHeaders.Add("Authorization", "Bearer " +
            //    accessToken);
            client1.BaseAddress = new Uri(TokenUrl);

            var response1 = await client1.PostAsync(TokenUrl, content);
            if (response1 == null)
            {
                throw new Exception("GetAccessToken responce getting null");
            }
            if (!response1.IsSuccessStatusCode)
            {
                dynamic error = new JObject();
                error.error = response1.StatusCode;
                error.error_description = response1.ReasonPhrase;
                return error;
            }
            else
            {
                var responseString = await response1.Content.ReadAsStringAsync();

                return JObject.Parse(responseString);
            }
        }

        public async Task<JObject> GetUserInfo(string accessToken)
        {

            var UserInfoUrl = configuration.GetValue<string>("dtidp:UserInfoEndpoint");

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message,
                cert,
                chain,
                sslPolicyErrors) =>
            {
                return true;
            };

            var client = new HttpClient(httpClientHandler);

            client.DefaultRequestHeaders.Clear();
            var authzHeader = "Bearer  " + accessToken;
            client.DefaultRequestHeaders.Add("UgPassAuthorization",
                authzHeader);

            var response = await client.GetAsync(UserInfoUrl);
            if (response == null)
            {
                throw new Exception("get user info responce getting null");
            }
            if (!response.IsSuccessStatusCode)
            {
                dynamic error = new JObject();
                error.error = response.StatusCode;
                error.error_description = response.ReasonPhrase;
                return error;
            }
            else
            {
                var responseString = await response.Content.ReadAsStringAsync();
                JObject info = JObject.Parse(responseString);
                return info;
            }

        }

        public async Task<ClaimsPrincipal> ValidateIdentityToken(string idToken)
        {
            try
            {
                //validate id_token
                var user = await ValidateJwt(idToken);

                //return id_token claim values
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ClaimsPrincipal> ValidateJwt(string jwt)
        {
            try
            {
                //set options for jwt signature validation
                var parameters = new TokenValidationParameters
                {
                    IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    {
                        var httpClientHandler = new HttpClientHandler();
                        httpClientHandler.ServerCertificateCustomValidationCallback = (message,
                            cert,
                            chain,
                            sslPolicyErrors) =>
                        {
                            return true;
                        };
                        /*get key from idp jwks url to validate id_token signature*/
                        var client = new HttpClient(httpClientHandler);
                        var response = client.GetAsync(configuration["dtidp:JwksUri"]).Result;
                        var responseString = response.Content.ReadAsStringAsync().Result;
                        var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(responseString);
                        return keys.Keys;
                    },
                    //set flag true for validate issuer
                    ValidateIssuer = true,
                    //set flag true for validate Audience
                    ValidateAudience = true,
                    //set valid issuer to verify in token issuer
                    ValidIssuer = configuration["dtidp:Issuer"],
                    //set valid Audience to verify in token Audience
                    ValidAudience = configuration["dtidp:ClientId"],
                    NameClaimType = "name"
                };

                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                //validate jwt token
                // if token is valid it return claim otherwise throw exception
                var user = handler.ValidateToken(jwt, parameters, out var _);
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
