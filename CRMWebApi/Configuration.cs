using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi
{
    class Configuration
    {
        private string _serviceUrl;

        public string ServiceUrl
        {
            get { return _serviceUrl; }
            set { _serviceUrl = value; }
        }

        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private SecureString _password;

        public SecureString Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _domain;

        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        private string _clientId;

        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        private string _redirectUrl;

        public string RedirectUrl
        {
            get { return _redirectUrl; }
            set { _redirectUrl = value; }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }


        public Configuration(string connectionStringName)
        {
            Name = connectionStringName;
            GetConnectionStringValues(Name);
        }


        public void GetConnectionStringValues(string connectionName)
        {
            try
            {
                var config = ConfigurationManager.ConnectionStrings;
                ConnectionStringSettings connection;
                var appSettings = ConfigurationManager.AppSettings;

                connection = config[connectionName];
                Name = connectionName;

                //Get connectionString parameter values
                if (connection != null)
                {
                    var parameters = connection.ConnectionString.Split(';');
                    foreach (string parameter in parameters)
                    {
                        var trimmedParamters = parameter.Trim();
                        if (trimmedParamters.StartsWith("Url="))
                        {
                            //parameter.Replace("Url=", String.Empty).TrimStart(' ');
                            ServiceUrl = parameter.Replace("Url=", String.Empty).TrimStart(' ');
                        }
                        if (trimmedParamters.StartsWith("Username="))
                        {
                            Username = parameters[1].Replace("Username=", String.Empty).TrimStart(' ');
                        }
                        if (trimmedParamters.StartsWith("Password="))
                        {
                            var password = parameters[2].Replace("Password=", String.Empty).TrimStart(' ');

                            Password = new SecureString();
                            foreach (char c in password) Password.AppendChar(c);
                        }
                        if (trimmedParamters.StartsWith("Domain="))
                        {
                            Domain = parameter.Replace("Domain=", String.Empty).TrimStart(' ');
                        }
                    }
                }
                else
                    throw new Exception("The specified connection string could not be found.");

                // Get the Azure Active Directory application registration settings.
                RedirectUrl = appSettings["RedirectUrl"];
                ClientId = appSettings["ClientId"];
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Required setting in app.config does not exist or is of the wrong type.", e);
            }
        }
    }
}
