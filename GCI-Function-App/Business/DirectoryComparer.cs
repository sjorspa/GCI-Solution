using GCI_Function_App.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
namespace GCI_Function_App.Business
{
    public class DirectoryComparer
    {
        public DirectoryComparer() { }
        public DirectoryComparer(DirectoryUsersOverview directoryGroupsResult, AnalyticsUsersOverview analyticsUsersOverview, ConfigurationObject configurationObject)
        {
            Config = configurationObject;
            DirectoryGroupsResult = directoryGroupsResult;
            AnalyticsUsersOverview = analyticsUsersOverview;
            AnalyticsHiearchy = GroupAnalyticsUsers();
            DirectoryHiearchy = GroupDirectoryUsers();

        }

        public List<UpsertAction> GetComparisonResult()
        {
            return GenerateComparisonResult();
        }
        public List<UpsertAction> GenerateComparisonResult()
        {
            List<UpsertAction> upsertActions = new List<UpsertAction>();
            foreach (var directoryAccount in DirectoryHiearchy.Accounts)
            {
                var mappedDirectoryAccount = GetAnalyticsIdbyFriendlyName(directoryAccount.Name);
                var analyticsAccount = AnalyticsHiearchy.Accounts.Where(x => x.Name == mappedDirectoryAccount).FirstOrDefault();
                if (analyticsAccount != null)
                {
                    foreach (var directoryAccountGroup in directoryAccount.AccountGroups)
                    {
                        var analyticsAccountGroup = analyticsAccount.AccountGroups.Where(x => x.Name == directoryAccountGroup.Name).FirstOrDefault();
                        //If Nobody is in this role we create a new group to not break the code
                        if (analyticsAccountGroup == null)
                        {
                            analyticsAccountGroup = new AccountGroup { Name = directoryAccountGroup.Name };
                        }
                        foreach (var directoryAccountGroupMember in directoryAccountGroup.AccountGroupMembers)
                        {
                            if (analyticsAccountGroup.AccountGroupMembers.Where(x => x.Email == directoryAccountGroupMember.Email).FirstOrDefault() == null)
                            {
                                //add member
                                Console.WriteLine($"Adding {directoryAccountGroupMember.Email} to Group {analyticsAccountGroup.Name} of Account {GetFriendlyNameByAnalayticsIC(analyticsAccount.Name)}");
                                upsertActions.Add(new UpsertAction { Action = "Add", Email = directoryAccountGroupMember.Email, Group = analyticsAccountGroup.Name, Account = analyticsAccount.Name });
                            }
                        }
                        foreach (var accountGroupMemberin in analyticsAccountGroup.AccountGroupMembers)
                        {
                            if (directoryAccountGroup.AccountGroupMembers.Where(x => x.Email == accountGroupMemberin.Email).FirstOrDefault() == null && Config.ProtectedAccunts.Where(x => x== accountGroupMemberin.Email).Count()==0)
                            {
                                //remove member
                                Console.WriteLine($"Removing {accountGroupMemberin.Email} from Group {analyticsAccountGroup.Name} of Account {GetFriendlyNameByAnalayticsIC(analyticsAccount.Name)}");
                                upsertActions.Add(new UpsertAction { Action = "Remove", Email = accountGroupMemberin.Email, Group = analyticsAccountGroup.Name, Account = analyticsAccount.Name });
                            }

                        }
                    }
                }
            }
            return upsertActions;
        }
        private string GetAnalyticsIdbyFriendlyName(string friendlyName)
        {
            var result = Config.AccountInfos.Where(x => x.FriendlyName == friendlyName).FirstOrDefault();
            if (result == null)
            {
                LogCollection.Add(new logItem { type = "error", message = $"account not found for account: {friendlyName}" });
                return string.Empty;
            }
            else
            {
                return result.AnalyticsID;
            }

        }
        private string GetFriendlyNameByAnalayticsIC(string analyticsId)
        {
            var result = Config.AccountInfos.Where(x => x.AnalyticsID == analyticsId).FirstOrDefault();
            if (result == null)
            {
                LogCollection.Add(new logItem { type = "error", message = $"account not found for analyticsId: {analyticsId}" });
                return string.Empty;
            }
            else
            {
                return result.FriendlyName;
            }
        }
        private string ConvertDirectRole(string directRole)
        {
            var result = Config.Roles.Where(x => x.Rolename == directRole).FirstOrDefault();
            if (result == null)
            {
                LogCollection.Add(new logItem { type = "error", message = $"No role found for {directRole}" });
                return string.Empty;
            }
            else
            {
                return result.FriendlyName;
            }
        }
        public Hiearchy GroupAnalyticsUsers()
        {
            Hiearchy hiearchy = new Hiearchy();
            foreach (var analyticsAccounts in AnalyticsUsersOverview.AnalyticsAccounts)
            {
                if (Config.AccountInfos.Where(x => x.AnalyticsID == analyticsAccounts.name).Count() == 1) {
                    var account = new Account();
                    account.Name = analyticsAccounts.name;
                    foreach (var groupMember in analyticsAccounts.AnalyticsUsers)
                    {
                        foreach (var directRole in groupMember.DirectRoles)
                        {
                            var groupCount = account.AccountGroups.Where(x => x.Name == ConvertDirectRole(directRole)).Count();
                            if (groupCount == 0)
                            {
                                account.AccountGroups.Add(new AccountGroup() { Name = ConvertDirectRole(directRole) });
                            }
                            account.AccountGroups.FirstOrDefault(x => x.Name == ConvertDirectRole(directRole)).AccountGroupMembers.Add(new AccountGroupMember { Email = groupMember.Email });
                        }
                    }
                    hiearchy.Accounts.Add(account);
                }
                else {
                    LogCollection.Add(new logItem { type = "error", message = $"No account found for {analyticsAccounts.name} in configuration" });
                }

            }
            return hiearchy;
        }
        public Hiearchy GroupDirectoryUsers()
        {
            Hiearchy hiearchy = new Hiearchy();
            var accountGroups = ExtractAccounts(DirectoryGroupsResult.DirectoryGroups);
            foreach (var accountGroup in accountGroups)
            {
                var account = new Account();
                account.Name = accountGroup;
                foreach (var directoryGroup in DirectoryGroupsResult.DirectoryGroups)
                {
                    if (directoryGroup.Name.StartsWith($"{accountGroup}_"))
                    {
                        var group = new AccountGroup { Name = directoryGroup.Name.Replace($"{accountGroup}_", "") };
                        foreach (var groupMember in directoryGroup.GroupMembers)
                        {
                            group.AccountGroupMembers.Add(new AccountGroupMember { Email = groupMember.Email });
                        }
                        account.AccountGroups.Add(group);
                    }
                }
                hiearchy.Accounts.Add(account);
            }
            return hiearchy;
        }
        private List<string> ExtractAccounts(List<DirectoryGroup> directoryGroups)
        {
            List<string> accountResult = new List<string>();
            foreach (var directoryGroup in directoryGroups)
            {

                    var groupName = ValidateGroupName(directoryGroup.Name);

                    if (!accountResult.Contains(groupName) && !string.IsNullOrEmpty(groupName))
                    {
                            accountResult.Add(groupName);
                    }
            }
            return accountResult;
        }

        private string ValidateGroupName(string groupName) {
            string validatedResult=string.Empty;
            if (!groupName.Contains("_") ) {
                LogCollection.Add(new logItem { type = "error", message = $"Groupname should contain a underscore, groupname: {groupName}" });
                return validatedResult;
            }
            var splittedResult = groupName.Split('_').ToList();
            if (splittedResult.Count() > 2)
            {
                LogCollection.Add(new logItem { type = "error", message = $"Groupname should contain not more than one underscore, groupname: {groupName}" });
                return validatedResult;
            }
            else {
                if (Config.AccountInfos.Where(x => x.FriendlyName == splittedResult[0]).Count() != 1)
                {
                    LogCollection.Add(new logItem { type = "error", message = $"Unknown Group, groupname: {splittedResult[0]}" });
                    return string.Empty;
                }
                if (Config.Roles.Where(x => x.FriendlyName == splittedResult[1]).Count() != 1)
                {
                    LogCollection.Add(new logItem { type = "error", message = $"Unknown Role, role: {splittedResult[1]}" });
                    return string.Empty;
                }
                else return splittedResult[0];

            }
        }

        public DirectoryUsersOverview DirectoryGroupsResult { get; set; }
        public AnalyticsUsersOverview AnalyticsUsersOverview { get; set; }
        public Hiearchy AnalyticsHiearchy { get; set; }
        public Hiearchy DirectoryHiearchy { get; set; }
        public ConfigurationObject Config { get; set; }
        public List<logItem> LogCollection { get; private set; } = new List<logItem>();
    }

    public class logItem
    {
        public string type;
        public string message;
    }
}
