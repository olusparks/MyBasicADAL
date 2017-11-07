using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
//using System.ServiceModel.Description;

namespace CRMWebApi
{
    class BasicOperations
    {

        //TODO: Uncomment then substitute your correct Dynamics 365 organization service 
        // address for either CRM Online or on-premise (end with a forward-slash).
        //private static string serviceUrl = "https://abisoyeltd.crm4.dynamics.com/";   // CRM Online
        //private static string serviceUrl = "https://<organization name>.<domain name>/";   // CRM IFD
        //private statics string serviceUrl = "http://myserver/myorg/";        // CRM on-premises

        //TODO: For an on-premises deployment, set your organization credentials here. (If
        // online or IFD, you can you can disregard or set to null.)
        //private static string userAccount = "<user-account>";  //CRM user account
        //private static string domain = "<server-domain>";  //CRM server domain

        //TODO: For CRM Online or IFD deployments, substitute your app registration values  
        // here. (If on-premise, you can disregard or set to null.)
        //private static string clientId = "c2636dcd-f072-40c4-9d93-071bf5bae4db";     //e.g. "e5cf0024-a66a-4f16-85ce-99ba97a24bb2"
                                                                                     //private static string redirectUrl = "http://MyCRMWebAPI.com";  //e.g. "http://localhost/SdkSample"


        //Provides a persistent client-to-CRM server communication channel.
        // HttpMessageHandler messageHandler;
        //private HttpClient httpClient;
        private string mediaType = "application/json";

        private Version webAPIVersion = new Version(8, 0);
        private string getVersionWebAPIPath()
        {
            
            return string.Format("v{0}/", webAPIVersion.ToString(2));
        }


        //Get the actual Version from of the Organization
        public async Task getWebAPIVersion(HttpClient httpClient)
        {
            HttpRequestMessage retrieveVersionRequest = new HttpRequestMessage(HttpMethod.Get, getVersionWebAPIPath() + "RetrieveVersion");
            HttpResponseMessage retrieveVersionResponse = await httpClient.SendAsync(retrieveVersionRequest);

            if (retrieveVersionResponse .StatusCode == HttpStatusCode.OK)
            {
                JObject retrievedVersion = JsonConvert.DeserializeObject<JObject>(await retrieveVersionResponse.Content.ReadAsStringAsync());
                //Capture the actual version available in this organization
                webAPIVersion = Version.Parse((String)retrievedVersion.GetValue("Version"));
            }
            else
            {
                Console.WriteLine("Failed to retrieve the version for reason: {0}", retrieveVersionResponse.ReasonPhrase);
                throw new CrmHttpException(retrieveVersionResponse.Content);
            }
        }


        //Centralized collection of entity URIs used to manage lifetimes.
        List<string> entityURIs = new List<string>();

        //A set of variables to hold the state of and URIs for primary entity instances.
        private JObject contact1 = new JObject(), contact2 = new JObject(), retrievedContact1;// retrievedContact2;
        private string contact1Uri;

        private JObject account1 = new JObject(), account2 = new JObject();// retrievedAccount1, retrievedAccount2; 
       // private string account1Uri, account2Uri;

        /// <summary>
        /// Create Operation
        /// </summary>
        /// <returns></returns>
        public async Task BasicCreate(HttpClient httpClient)
        {
            ChangeColor("Create Operation using CRM Web API");

            string objectContent1 = "{firstname: 'Jide', lastname: 'Bantale'}";
            contact1.Add(objectContent1);


            //Make a POST request to create a contact and recieve Response
            HttpRequestMessage createRequest1 = new HttpRequestMessage(HttpMethod.Post, getVersionWebAPIPath() + "contacts");
            createRequest1.Content = new StringContent(contact1.ToString(), Encoding.UTF8, mediaType);
            HttpResponseMessage createResponse1 = await httpClient.SendAsync(createRequest1);

            if (createResponse1.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("Contact {0} {1}", contact1.GetValue("firstname"), contact1.GetValue("lastname"));
                contact1Uri = createResponse1.Headers.GetValues("OData-EntityId").FirstOrDefault();
                entityURIs.Add(contact1Uri);
                Console.WriteLine("Conact URI {0}", contact1Uri);
            }
            else
            {
                ErrorColor(string.Format("Failed to create contact for reason: {0}", createResponse1.ReasonPhrase));
                throw new CrmHttpException(createResponse1.Content);
            }
        }

        /// <summary>
        /// Update Operation
        /// </summary>
        /// <returns></returns>
        public async Task BasicUpdate(HttpClient httpClient)
        {
            //create an HttpRequest, Response, check response status code
            JObject contactAdd1 = new JObject();
            contactAdd1.Add("annualincome", 8000);
            contactAdd1.Add("jobtitle", "CRM Developer");

            //create an update request
            HttpRequestMessage updateRequest1 = new HttpRequestMessage(new HttpMethod("PATCH"), contact1Uri);
            updateRequest1.Content = new StringContent(contactAdd1.ToString(), Encoding.UTF8, mediaType);
            HttpResponseMessage updateResponse = await httpClient.SendAsync(updateRequest1);
            if (updateResponse.StatusCode == HttpStatusCode.NoContent)
            {
                Console.WriteLine("Contact {0} updated with jobtitle and annual income", contact1.GetValue("firstname"));
            }
            else
            {
                ErrorColor(string.Format("Failed to create contact for reason: {0}", updateResponse.ReasonPhrase));
                throw new CrmHttpException(updateResponse.Content);
            }

        }

        /// <summary>
        /// Retrieve Operation
        /// </summary>
        /// <returns></returns>
        public async Task BasicRetrieve(HttpClient httpClient)
        {
            //Create properties to retrieve, query string, httpRequestMessage, httpResponseMessage, 
            ChangeColor("---Basic Query---");
            string[] contactProperties = { "fullname", "jobtitle", "annualincome" };
            string queryOperation = "?$select=" + String.Join(",", contactProperties);

            HttpResponseMessage retrieveResponse = await httpClient.GetAsync(contact1Uri + queryOperation);
            if (retrieveResponse.StatusCode == HttpStatusCode.OK) //200
            {
                retrievedContact1 = JsonConvert.DeserializeObject<JObject>(await retrieveResponse.Content.ReadAsStringAsync());
                //Display retrieved values
                Console.WriteLine("Fullname:{0}  \n AnnualIncome: {1}  \n Job Title: {2}  \n Description: {3} ", retrievedContact1.GetValue("fullname"), retrievedContact1["annualincome"],retrievedContact1["jobtitle"], retrievedContact1["description"] );
            }
            else
            {
                ErrorColor(string.Format("Failed to create contact for reason: {0}", retrieveResponse.ReasonPhrase));
                throw new CrmHttpException(retrieveResponse.Content);
            }
        }


        private void ChangeColor(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        
        private void ErrorColor(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
