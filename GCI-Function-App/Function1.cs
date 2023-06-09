using GCI_Function_App.Business;
using GCI_Function_App.Classes;
using GCI_Function_App.Clients;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GCI_Function_App
{
    public class Function1
    {
        [FunctionName("Function1")]
        public static async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var azureClient = new AzureClient(Environment.GetEnvironmentVariable("keyvault"));
            var credentials = azureClient.GetKeyVaultSecretAsync("directoryuser").ToString();
            var googleClient = new GoogleClient(credentials, "sjors@hamertijd.com");
            //var directoryUsersResult = googleClient.GetDirectoryUsers("hamertijd.com");
            var directoryGroupsResult = googleClient.GetGroups("hamertijd.com");

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
            DirectoryComparer directoryComparer = new DirectoryComparer(directoryUsersOverview, analyticsUsersOverview, new ConfigurationObject());
        }
    }
}
