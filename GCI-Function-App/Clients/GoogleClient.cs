using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.GoogleAnalyticsAdmin.v1alpha;
using Google.Apis.GoogleAnalyticsAdmin.v1alpha.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;

namespace GCI_Function_App.Clients
{
    internal class GoogleClient
    {
        private static string _secret;
        private static string _directoryImpersonateAccount;
        private static GoogleAnalyticsAdminService _googleAnalyticsAdminService;
        private static DirectoryService _directoryService;

        public GoogleClient(string secret, string directoryImpersonateAccount)
        {
            _secret = secret;
            _directoryImpersonateAccount = directoryImpersonateAccount;
            CreateGoogleAnalyticsAdminService();
            CreateDirectoryService();
        }

        public Users GetDirectoryUsers(string domain)
        {
            var list = _directoryService.Users.List();
            list.Domain = domain;
            try
            {
                var users = list.Execute();
                return users;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Groups GetGroups(string domain)
        {
            var list = _directoryService.Groups.List();
            list.Domain = domain;
            try
            {
                var groups = list.Execute();
                return groups;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Members GetMembers(string group)
        {
            var list = _directoryService.Members.List(group);
            try
            {
                var members = list.Execute();
                return members;
            }
            catch (Exception e)
            {
                return null;
            }
        }


        public GoogleAnalyticsAdminV1alphaListAccountsResponse GetAnalyticsAccounts()
        {
            var googleAnalyticsAdminService = _googleAnalyticsAdminService.Accounts.List();
            try
            {
                var accountsResponse = googleAnalyticsAdminService.Execute();
                return accountsResponse;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public IList<GoogleAnalyticsAdminV1alphaUserLink> GetAnalyticsUsersByAccount(string account)
        {
            var googleAnalyticsAdminService = _googleAnalyticsAdminService.Accounts.UserLinks.List(account);
            try
            {
                var UserLinksRequest = googleAnalyticsAdminService.Execute();
                return UserLinksRequest.UserLinks;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static void CreateGoogleAnalyticsAdminService()
        {

            var credentials = GoogleCredential.FromJson(_secret);
            if (credentials.IsCreateScopedRequired)
                credentials = credentials.CreateScoped(new[] { GoogleAnalyticsAdminService.Scope.AnalyticsReadonly, GoogleAnalyticsAdminService.Scope.AnalyticsManageUsers });

            var service = new GoogleAnalyticsAdminService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
                //BaseUri = "https://myapisjors.azure-api.net"
            });
            _googleAnalyticsAdminService = service;
        }
        private static void CreateDirectoryService()
        {

            var credentials = GoogleCredential.FromJson(_secret).CreateWithUser(_directoryImpersonateAccount);
            if (credentials.IsCreateScopedRequired)
                credentials = credentials.CreateScoped(new[] { DirectoryService.Scope.AdminDirectoryUserReadonly, DirectoryService.Scope.AdminDirectoryGroup });

            var service = new DirectoryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
            });
            _directoryService = service;
        }
    }


}
