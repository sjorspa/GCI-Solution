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
using Role = GCI_Function_App.Classes.Role;

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
            Console.WriteLine(secret);
            _directoryImpersonateAccount = directoryImpersonateAccount;
            CreateGoogleAnalyticsAdminService();
            CreateDirectoryService();
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


        public void RemoveAnalyticsUser(string account)
        {
            var googleAnalyticsAdminService = _googleAnalyticsAdminService.Accounts.UserLinks.Delete(account);
            try
            {
                googleAnalyticsAdminService.Execute();
            }
            catch (Exception e)
            {
            }
        }

        public void AddAnalyticsUser(string emailAddress, string parent, List<Role> roles, string directroles)
        {
            var efficiveroles = new List<string>();
            foreach (var role in directroles.Split(',').ToList())
            {
                if (roles.Where(x => x.FriendlyName == role).Count()==1) { 
                    efficiveroles.Add(roles.Where(x => x.FriendlyName == role).FirstOrDefault().Rolename);
                }
            }
            var googleAnalyticsAdminService = _googleAnalyticsAdminService.Accounts.UserLinks.Create(new GoogleAnalyticsAdminV1alphaUserLink { EmailAddress = emailAddress, DirectRoles = efficiveroles },parent);
            try
            {
                googleAnalyticsAdminService.Execute();
            }
            catch (Exception e)
            {
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
