using GCI_Function_App.Classes;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace GCI_Function_App.Business
{
    public class DirectoryComparer
    {
        public DirectoryComparer() { }
        public DirectoryComparer(DirectoryUsersOverview directoryGroupsResult, AnalyticsUsersOverview analyticsUsersOverview)
        {
            DirectoryGroupsResult = directoryGroupsResult;
            AnalyticsUsersOverview = analyticsUsersOverview;
            AnalyticsHiearchy = GroupAnalyticsUsers();
            DirectoryHiearchy = GroupDirectoryUsers();
        }

        public List<UpsertAction> GetComparisonResult() {
            return GenerateComparisonResult();
        }

        public List<UpsertAction> GenerateComparisonResult() {
            List<UpsertAction> upsertActions = new List<UpsertAction>();
            foreach (var directoryAccount in DirectoryHiearchy.Accounts)
            {
                var mappedDirectoryAccount = GetMappedAccount(directoryAccount.Name);
                var analyticsAccount = AnalyticsHiearchy.Accounts.Where(x => x.Name == mappedDirectoryAccount).FirstOrDefault();
                if (analyticsAccount != null)
                {
                    foreach (var directoryAccountGroup in directoryAccount.AccountGroups) {
                        var analyticsAccountGroup = analyticsAccount.AccountGroups.Where(x=> x.Name == directoryAccountGroup.Name).FirstOrDefault();
                        //If Nobody is in this role we create a new group to not break the code
                        if (analyticsAccountGroup == null) {
                            analyticsAccountGroup = new AccountGroup { Name = directoryAccountGroup.Name };
                        }
                        foreach (var directoryAccountGroupMember in directoryAccountGroup.AccountGroupMembers) {
                            if (analyticsAccountGroup.AccountGroupMembers.Where(x=> x.Email == directoryAccountGroupMember.Email).FirstOrDefault() == null) {
                                //add member
                                Console.WriteLine($"Adding {directoryAccountGroupMember.Email} to Group {analyticsAccountGroup.Name} of Account {GetMappedName(analyticsAccount.Name)}");
                                upsertActions.Add(new UpsertAction {Action="Add", Email = directoryAccountGroupMember.Email, Group = analyticsAccountGroup.Name, Account= analyticsAccount.Name});
                            }
                        }
                        foreach (var accountGroupMemberin in analyticsAccountGroup.AccountGroupMembers) {
                            if (directoryAccountGroup.AccountGroupMembers.Where(x => x.Email == accountGroupMemberin.Email).FirstOrDefault() == null)
                            {
                                //add member
                                Console.WriteLine($"Removing {accountGroupMemberin.Email} from Group {analyticsAccountGroup.Name} of Account {GetMappedName(analyticsAccount.Name)}");
                                upsertActions.Add(new UpsertAction { Action = "Remove", Email = accountGroupMemberin.Email, Group = analyticsAccountGroup.Name, Account = analyticsAccount.Name });
                            }

                        }
                    }
                }
            }
            return upsertActions;
        }

        private string GetMappedAccount(string name)
        {
            switch (name)
            {
                case "nl":
                    return "accounts/196069204";
                case "en":
                    return "accounts/270720121";
                default:
                    LogCollection.Add(new logItem {type="error",message=$"account not found for: {name}" });
                    return "";
            }
        }
        private string GetMappedName(string name)
        {
            switch (name)
            {
                case "accounts/196069204":
                    return "nl";
                case "accounts/270720121":
                    return "en";
                default:
                    return "";
            }
        }
        public Hiearchy GroupAnalyticsUsers() {
            Hiearchy hiearchy = new Hiearchy();
            foreach (var analyticsAccounts in AnalyticsUsersOverview.AnalyticsAccounts)
            {
                var account = new Account();
                account.Name = analyticsAccounts.name;
                foreach (var groupMember in analyticsAccounts.AnalyticsUsers) {
                    foreach (var directRole in groupMember.DirectRoles)
                    {
                        var groupCount = account.AccountGroups.Where(x => x.Name == ConvertDirectRole(directRole)).Count();
                        if (groupCount == 0)
                        {
                            account.AccountGroups.Add(new AccountGroup() { Name = ConvertDirectRole(directRole) });
                        }
                        account.AccountGroups.FirstOrDefault(x => x.Name == ConvertDirectRole(directRole)).AccountGroupMembers.Add(new AccountGroupMember{ Email=groupMember.Email});
                    }
                }
                hiearchy.Accounts.Add(account);
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
                foreach (var directoryGroup in DirectoryGroupsResult.DirectoryGroups) {
                    if (directoryGroup.Name.StartsWith($"{accountGroup}_")) {
                        var group = new AccountGroup { Name = directoryGroup.Name.Replace($"{accountGroup}_", "") };
                        foreach (var groupMember in directoryGroup.GroupMembers) {
                            group.AccountGroupMembers.Add(new AccountGroupMember { Email=groupMember.Email });
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
                if (directoryGroup.Name.Contains("_")) {
                    var groupname = directoryGroup.Name.Split("_").ToList().FirstOrDefault();
                    if (!accountResult.Contains(groupname)) {
                        accountResult.Add(groupname);
                    }
                }
            }
            return accountResult;
        }

        private string ConvertDirectRole(string directRole)
        {
            switch (directRole)
            {
                case "predefinedRoles/viewer":
                    return "viewer";
                case "predefinedRoles/analyst":
                    return "analyst";
                 case "predefinedRoles/editor":
                    return "editor";
                  case "predefinedRoles/admin":
                    return "admin";
                case "Blank":
                    return "Blank";
                case "predefinedRoles/no-cost-data":
                    return "no-cost-data";
                case "predefinedRoles/no-revenue-data":
                    return "no-revenue-data";
                default:
                    return "";
            }
        }

        public DirectoryUsersOverview DirectoryGroupsResult { get; set; }
        public AnalyticsUsersOverview AnalyticsUsersOverview { get; set; }
        public Hiearchy AnalyticsHiearchy { get; set; }
        public Hiearchy DirectoryHiearchy { get; set; }
        public List<logItem> LogCollection { get; private set; } = new List<logItem>();
    }

    public class logItem
    {
        public string type;
        public string message;
    }
}
