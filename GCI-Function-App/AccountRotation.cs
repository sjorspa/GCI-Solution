using GCI_Function_App.Business;
using GCI_Function_App.Classes;
using GCI_Function_App.Clients;
using LogAnalytics.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GCI_Function_App
{
    public class AccountRotation
    {
        private static GoogleIAMClient iamClient;
        private static AzureClient azureClient;
        [FunctionName("AccountRotation")]
        public static async Task RunAsync([TimerTrigger("%TimerIntervalDirectorySync%")] TimerInfo myTimer, ILogger log)
        {
            azureClient = new AzureClient(Environment.GetEnvironmentVariable("keyvault"));
            var credentials = azureClient.GetKeyVaultSecretAsync("secretrotator").ToString();
            iamClient = new GoogleIAMClient(credentials);
            SetupNewKeySequence(Environment.GetEnvironmentVariable("serviceAccount"), Environment.GetEnvironmentVariable("googleProject"), Environment.GetEnvironmentVariable("kvserviceAccount"));
            SetupNewKeySequence(Environment.GetEnvironmentVariable("rotationAccount"), Environment.GetEnvironmentVariable("googleProject"), Environment.GetEnvironmentVariable("kvrotationAccount"));

        }

        public static void SetupNewKeySequence(string account, string project, string secretName) {
            //Create A new Key
            var newKey = CreateNewKey(account, project);
            //Store The new Key in KeyVault
            if (StoreNewKey(secretName, newKey)) {
                //Succes, we purge all keys besides the most recent one.
                PurgeKeys(account, project);
            }
        }
        public static bool StoreNewKey(string secretName, string secretValue)
        {
            try
            {
                azureClient.PutKeyVaultSecretAsync(secretName, secretValue);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string CreateNewKey(string account, string project)
        {
            try
            {
                return iamClient.GetNewServiceAccountKey($"projects/{project}/serviceAccounts/{account}");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool PurgeKeys(string account, string project)
        {
            try
            {
                iamClient.PurgeKeys($"projects/{project}/serviceAccounts/{account}");
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }


}
