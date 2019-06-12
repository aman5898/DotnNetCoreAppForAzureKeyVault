using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;

namespace WebAppKeyVault
{
    public class Common
    {

        private IConfiguration configuration;

        public Common(IConfiguration iconfig)
        {
            configuration = iconfig;
        }

        public string Message { get; set; }

        public async Task<string> OnGetAsync()
        {
            Message = "Your application description page.";
            string url = configuration.GetSection("keyForVault").Value;
            try
            {

                //var URL = _configuration["keyForVault"];
                /* The next four lines of code show you how to use AppAuthentication library to fetch secrets from your key vault */
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync(url)
                        .ConfigureAwait(false);
                Message = secret.Value;
            }
            
            catch (KeyVaultErrorException keyVaultException)
            {
                Message = keyVaultException.Message;
            }

            return Message;
        }

        // This method implements exponential backoff if there are 429 errors from Azure Key Vault
        private long getWaitTime(int retryCount)
        {
            long waitTime = ((long)Math.Pow(2, retryCount) * 100L);
            return waitTime;
        }

        // This method fetches a token from Azure Active Directory, which can then be provided to Azure Key Vault to authenticate
        public async Task<string> GetAccessTokenAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://vault.azure.net");
            return accessToken;
        }
    }
}
