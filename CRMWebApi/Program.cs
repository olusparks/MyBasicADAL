using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi
{
    class Program
    {

        static void Main(string[] args)
        {
            HttpClient httpClient = TheHttpClient();
            BasicOperations basicOp = new BasicOperations();
            basicOp.getWebAPIVersion(httpClient).Wait();
            basicOp.BasicCreate(httpClient).Wait();

            //Authentication authClass = new Authentication(new Configuration(""));
            ////string authority = "https://login.windows.net/8bdbc917-caf6-4a97-a043-94eaebb42374/oauth2/authorize";
            //string serviceUrl = "https://abisoyeltd.crm4.dynamics.com/";
            //string clientId = "c2636dcd-f072-40c4-9d93-071bf5bae4db";
            //UserPasswordCredential userPass = new UserPasswordCredential("abisoyes@abisoyeltd.onmicrosoft.com", "Adejumoke1!");
            ////UserCredential userCred = new UserCredential("abisoyes@abisoyeltd.onmicrosoft.com");
            //string innerException = string.Empty;

            ////string authorityUri = "https://login.windows.net/abisoyeltd.onmicrosoft.com/oauth2/authorize";
            //AuthenticationParameters ap = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(serviceUrl + "api/data/")).Result;
            //TokenCache tokenCache = new TokenCache();
            //AuthenticationContext authContext = new AuthenticationContext(ap.Authority);
            //try
            //{

            //    AuthenticationResult authResult = authContext.AcquireTokenAsync(serviceUrl, clientId, userPass).Result;
            //    //AuthenticationResult authResult2 = authContext.AcquireTokenSilentAsync(serviceUrl, clientId).Result;
            //    //string accessToken2 = authResult2.AccessToken;
            //    string accessToken = authResult.AccessToken;
            //    LoadAccounts(accessToken);
            //}
            //catch (Exception e)
            //{
            //    if (e.InnerException != null)
            //         Console.WriteLine($"InnerException: {e.InnerException.Message.ToString()} \n Message: {e.Message}"); 
            //    //throw new Exception(e.Message);
            //}


            Console.ReadLine();
        }

        static void LoadAccounts(string accessToken)
        {
            var webRequest = WebRequest.Create(new Uri("https://abisoyeltd.crm4.dynamics.com/api/data/v9.0/accounts?$select=name,address1_city&$top=10"));
            webRequest.Method = "GET";
            webRequest.ContentLength = 0;
            webRequest.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken));
            webRequest.Headers.Add("OData-MaxVersion", "4.0");
            webRequest.Headers.Add("OData-Version", "4.0");
            webRequest.ContentType = "application/json; charset=utf-8";

            using (var response = webRequest.GetResponse() as System.Net.HttpWebResponse)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();
                    dynamic dynamicObj = JsonConvert.DeserializeObject(responseContent);
                    foreach (var item in dynamicObj.value)
                    {
                        Console.WriteLine("Account: {0}", item.name.Value);
                    }
                }
            }
        }

        static HttpClient TheHttpClient()
        {
            Authentication auth = new Authentication(new Configuration("CrmOnline"));
            string accessToken = Authentication.AcquireToken().AccessToken;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");

            return httpClient;
        }
    }
}
