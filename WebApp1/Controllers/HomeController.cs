using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace WebApp1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        private String apiEndpoint = "https://localhost:44335/api/values/";

        [Authorize]
        public async System.Threading.Tasks.Task<ActionResult> About()
        {
            try
            {
                // Retrieve the token with the specified scopes
                var scope = new string[] { Startup.ReadTasksScope };
                string signedInUserID = ClaimsPrincipal.Current.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                TokenCache userTokenCache = new MSALSessionCache(signedInUserID, this.HttpContext).GetMsalCacheInstance();
                ConfidentialClientApplication cca = new ConfidentialClientApplication(Startup.ClientId, Startup.Authority, Startup.RedirectUri, new ClientCredential(Startup.ClientSecret), userTokenCache, null);

                var user = cca.Users.FirstOrDefault();
                if (user == null)
                {
                    throw new Exception("The User is NULL.  Please clear your cookies and try again.  Specifically delete cookies for 'login.microsoftonline.com'.  See this GitHub issue for more details: https://github.com/Azure-Samples/active-directory-b2c-dotnet-webapp-and-webapi/issues/9");
                }

                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, user, Startup.Authority, false);

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);

                // Add token to the Authorization header and make the request
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        String responseString = await response.Content.ReadAsStringAsync();
                        JArray tasks = JArray.Parse(responseString);
                        ViewBag.Message = tasks.ToString();

                        return View();
                    //case HttpStatusCode.Unauthorized:
                        //return ErrorAction("Please sign in again. " + response.ReasonPhrase);
                    //default:
                        //return ErrorAction("Error. Status code = " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                //return ErrorAction("Error reading to do list: " + ex.Message);
            }
            return RedirectToAction("index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}