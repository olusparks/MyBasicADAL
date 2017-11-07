using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi
{
    class Authentication
    {
        private static Configuration _config = null;
        private static HttpMessageHandler _clientHandler = null;
        private static AuthenticationContext _context = null;
        private static string _authority = null;

        public HttpMessageHandler ClientHandler
        {
            get { return _clientHandler; }
            set { _clientHandler = value; }
        }

        public AuthenticationContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public string Authority
        {
            get
            {
                if (_authority != null)
                {
                    _authority = DiscoverAuthority(_config.ServiceUrl);
                }
                return _authority;
            }
            set { _authority = value; }
        }

        #region Constructor

        public Authentication() { }

        public Authentication(Configuration config): base()
        {
            _config = config;
        }

        #endregion


        public static string DiscoverAuthority(string serviceUrl)
        {
            try
            {
                AuthenticationParameters ap = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri($"{serviceUrl}api/data/")).Result;
                return ap.Authority;
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An HTTP request exception occurred during authority discovery.", e);
            }
        }

        public static async Task<string> DiscoverAuthorityAsync(string serviceUrl)
        {
            try
            {
                AuthenticationParameters ap = await AuthenticationParameters.CreateFromResourceUrlAsync(new Uri($"{serviceUrl}api/data"));
                return ap.Authority;
            }
            catch (HttpRequestException e)
            {

                throw new Exception("An HTTP request exception occurred during authority discovery.", e);
            }
        }

        public static async Task<AuthenticationResult> AcquireTokenAsync(string username, SecureString password)
        {
            try
            {
                if (!string.IsNullOrEmpty(username) || password != null)
                {
                    UserPasswordCredential userCred = new UserPasswordCredential(username, password);
                    _context = new AuthenticationContext(DiscoverAuthorityAsync(_config.ServiceUrl).Result);
                    return await _context.AcquireTokenAsync(_config.ServiceUrl, _config.ClientId, userCred);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed. Verify the configuration values are correct.", e);
            }
            return null;
        }

        public static AuthenticationResult AcquireToken()
        {
            try
            {
                if (_config != null && (_config.Username != null && _config.Password != null))
                {
                    UserPasswordCredential userCred = new UserPasswordCredential(_config.Username, _config.Password);
                    _context = new AuthenticationContext(DiscoverAuthority(_config.ServiceUrl));
                    return _context.AcquireTokenAsync(_config.ServiceUrl, _config.ClientId, userCred).Result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed. Verify the configuration values are correct.", e);
            }
            return null;
        }
    }
}
