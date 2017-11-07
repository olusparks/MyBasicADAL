using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CRMWebApi
{
    class CrmHttpException : System.Exception
    {
        private static string _stackTrace;

        public override string StackTrace
        {
            get
            {
                return _stackTrace;
            }
        }

        //constructors

        /// <summary>
        /// Initializes a new instance of the CrmHttpResponseException class.
        /// </summary>
        /// <param name="content">The populated HTTP content in Json format.</param>
        public CrmHttpException(HttpContent content) :
            base(ExtractMessageFromContent(content))
        {

        }

        /// <summary>
        /// Initializes a new instance of the CrmHttpResponseException class.
        /// </summary>
        /// <param name="content">The populated HTTP content in Json format.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference
        /// if no inner exception is specified.</param>
        public CrmHttpException(HttpContent content, Exception innerException): 
            base(ExtractMessageFromContent(content), innerException)
        { }

        //Methods
        /// <summary>
        /// Extracts the CRM specific error message and stack trace from an HTTP content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>The error message.</returns>
        private static string ExtractMessageFromContent(HttpContent content)
        {
            string message = string.Empty;
            string downloadedContent = content.ReadAsStringAsync().Result;

            if (content.Headers.ContentType.MediaType.Equals("text/plain"))
            {
                message = downloadedContent;
            }
            else if (content.Headers.ContentType.MediaType.Equals("application/json"))
            {
                //Use JObject to deserialize the objects
                JObject jcontent = (JObject)JsonConvert.DeserializeObject(downloadedContent);
                IDictionary<string, JToken> d = jcontent;

                if (d.ContainsKey("error"))
                {
                    message = (String)jcontent.Property("error").Value;
                }
                else if(d.ContainsKey("Message"))
                {
                    message = (String)jcontent.Property("Message").Value;
                }
                else if (d.ContainsKey("StackTrace"))
                {
                    _stackTrace = (String)jcontent.Property("StackTrace").Value;
                }
                else if(content.Headers.ContentType.MediaType.Equals("text/html"))
                {
                    message = "HTML content that was returned is shown below.";
                    message += "\n\n" + downloadedContent;
                }
                else
                {
                    message = String.Format("No handler is available for content in the {0} format.", content.Headers.ContentType.MediaType.ToString());
                }
            }

            return message;
        }
    }
}
