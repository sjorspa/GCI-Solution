using GCI_Function_App.Classes;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.GoogleAnalyticsAdmin.v1alpha;
using Google.Apis.GoogleAnalyticsAdmin.v1alpha.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Iam.v1;
using Google.Apis.Iam.v1.Data;

using Role = GCI_Function_App.Classes.Role;
using System.Text;

namespace GCI_Function_App.Clients
{
    internal class GoogleIAMClient
    {
        private static string _secret;
        private static IamService _iamService;

        public GoogleIAMClient(string secret)
        {
            _secret = secret;
            CreateIAMService();
        }
        public string GetNewServiceAccountKey(string serviceAccountl)
        {

            var key = _iamService.Projects.ServiceAccounts.Keys.Create(
    new CreateServiceAccountKeyRequest(),
    serviceAccountl)
    .Execute();

            // The PrivateKeyData field contains the base64-encoded service account key
            // in JSON format.
            // TODO(Developer): Save the below key (jsonKeyFile) to a secure location.
            //  You cannot download it later.
            byte[] valueBytes = System.Convert.FromBase64String(key.PrivateKeyData);
            string jsonKeyContent = Encoding.UTF8.GetString(valueBytes);

            Console.WriteLine("Key created successfully");
            return jsonKeyContent;

        }
        public void PurgeKeys(string account)
        {
            var currentKeys = ListServiceAccountKeys(account).Keys.Where(x => x.KeyType != "SYSTEM_MANAGED").OrderBy(x => x.ValidAfterTime).ToList();
            currentKeys = currentKeys.Take(currentKeys.Count -1).ToList();

            foreach (var key in currentKeys) {
                _iamService.Projects.ServiceAccounts.Keys.Delete(key.Name).Execute();
            }

            //var result = _iamService.Projects.ServiceAccounts.Keys.Delete(key).Execute();
        }
        public ListServiceAccountKeysResponse ListServiceAccountKeys(string account)
        {
            return _iamService.Projects.ServiceAccounts.Keys.List(account).Execute();
        }

        private static void CreateIAMService()
        {

            var credentials = GoogleCredential.FromJson(_secret); ;
            if (credentials.IsCreateScopedRequired)
                credentials = credentials.CreateScoped(new[] { IamService.Scope.CloudPlatform });

            var service = new IamService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
            });
            _iamService = service;
        }

    }


}
