using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

namespace GCI_Function_App.Clients
{
    internal class AzureClient
    {
        private string _keyvaultName;

        public AzureClient(string keyVaultName)
        {
            _keyvaultName = keyVaultName;
        }



        public string GetKeyVaultSecretAsync(string secretName)
        {
            var kvUri = "https://" + _keyvaultName + ".vault.azure.net";
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            var secret = client.GetSecret(secretName);
            return secret.Value.Value.ToString();
        }
    }
}
