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
    public class DirectorySync
    {
        [FunctionName("DirectorySync")]
        public static async Task RunAsync([TimerTrigger("%TimerIntervalRotator%")] TimerInfo myTimer, ILogger log)
        {
            var azureClient = new AzureClient(Environment.GetEnvironmentVariable("keyvault"));
            var credentials = azureClient.GetKeyVaultSecretAsync("serviceaccount").ToString();
            var logspace = azureClient.GetKeyVaultSecretAsync("logspace").ToString();

            var googleClient = new GoogleClient(credentials, Environment.GetEnvironmentVariable("impersonateAccount"));
            //var directoryUsersResult = googleClient.GetDirectoryUsers("hamertijd.com");
            var directoryGroupsResult = googleClient.GetGroups(Environment.GetEnvironmentVariable("domain"));

            //Get Overview of Groups with their Members
            DirectoryUsersOverview directoryUsersOverview = new DirectoryUsersOverview();
            foreach (var group in directoryGroupsResult.GroupsValue)
            {
                DirectoryGroup directoryGroup = new DirectoryGroup { Name = group.Name, GroupId = group.Id };
                var groupMembers = googleClient.GetMembers(group.Id);
                //TODO check or this works with larger amount of members
                if (groupMembers.MembersValue != null)
                {
                    foreach (var member in groupMembers.MembersValue)
                    {
                        directoryGroup.GroupMembers.Add(new GroupMember { Email = member.Email, Id = member.Id });
                    }
                }
                directoryUsersOverview.DirectoryGroups.Add(directoryGroup);
            }

            //Get An Overview of all AnalyticsUsers Per Analytics Account
            AnalyticsUsersOverview analyticsUsersOverview = new AnalyticsUsersOverview();
            var AccountsResponse = googleClient.GetAnalyticsAccounts();
            foreach (var account in AccountsResponse.Accounts)
            {
                AnalyticsAccount analyticsAccount = new AnalyticsAccount { name = account.Name, DisplayName = account.DisplayName };
                var usersResult = googleClient.GetAnalyticsUsersByAccount(account.Name);
                foreach (var user in usersResult)
                {
                    AnalyticsUser analyticsUser = new AnalyticsUser { Name = user.Name, Email = user.EmailAddress, DirectRoles = user.DirectRoles };
                    analyticsAccount.AnalyticsUsers.Add(analyticsUser);
                }
                analyticsUsersOverview.AnalyticsAccounts.Add(analyticsAccount);
            }
            var configurationObject = JsonConvert.DeserializeObject<ConfigurationObject>(File.ReadAllText(@"configuration.json"));
            DirectoryComparer directoryComparer = new DirectoryComparer(directoryUsersOverview, analyticsUsersOverview, configurationObject);
            var result = directoryComparer.GenerateComparisonResult();

            LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: Environment.GetEnvironmentVariable("workspaceId"),
                sharedKey: logspace);
            foreach (var entry in result)
            {
                if (entry.Action == "Add")
                {
                    googleClient.AddAnalyticsUser(entry.Email,entry.Account, configurationObject.Roles, entry.Group);
                }
                if (entry.Action == "Remove")
                {
                    googleClient.RemoveAnalyticsUser(entry.AnaltyicsUserName);
                }

                logger.SendLogEntry(new UpsertItem
                {
                    Action = entry.Action,
                    Account = entry.Account,
                    Email = entry.Email,
                    AnaltyicsUserName = entry.AnaltyicsUserName
                }, "UpsertEntries").Wait();
            }

        }
    }
}
