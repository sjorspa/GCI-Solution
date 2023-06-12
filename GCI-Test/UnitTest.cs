using GCI_Function_App.Business;
using GCI_Function_App.Classes;
namespace GCI_Test
{
    public class Tests
    {
        List<UpsertAction> upsertActions;
        DirectoryUsersOverview directoryUsersOverview = new DirectoryUsersOverview { };
        DirectoryComparer directoryComparer;
        [OneTimeSetUp]
        public void Setup()
        {



            DirectoryGroup directoryGroup1 = new DirectoryGroup { Name = "nl_admin" };
            directoryGroup1.GroupMembers.Add(new GroupMember { Email = "user1@mail.com" });
            directoryGroup1.GroupMembers.Add(new GroupMember { Email = "user2@mail.com" });
            directoryGroup1.GroupMembers.Add(new GroupMember { Email = "user3@mail.com" });
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup1);

            DirectoryGroup directoryGroup2 = new DirectoryGroup { Name = "en_admin" };
            directoryGroup2.GroupMembers.Add(new GroupMember { Email = "user1@mail.com" });
            directoryGroup2.GroupMembers.Add(new GroupMember { Email = "user2@mail.com" });
            directoryGroup2.GroupMembers.Add(new GroupMember { Email = "user3@mail.com" });
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup2);

            //Invalid Domain
            DirectoryGroup directoryGroup3 = new DirectoryGroup { Name = "xxxxxxxxxxxx_admin" };
            directoryGroup3.GroupMembers.Add(new GroupMember { Email = "user1@mail.com" });
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup3);

            //Invalid Domain no Underscores
            DirectoryGroup directoryGroup5 = new DirectoryGroup { Name = "xxxxxxadmin" };
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup5);

            //Invalid Domain Multiple Underscores
            DirectoryGroup directoryGroup6 = new DirectoryGroup { Name = "_xxxxx_xxxxxxx_admin" };
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup5);


            //Invalid Role
            DirectoryGroup directoryGroup4 = new DirectoryGroup { Name = "nl_invalidrole" };
            directoryGroup3.GroupMembers.Add(new GroupMember { Email = "user1@mail.com" });
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup4);


            AnalyticsUsersOverview analyticsUsersOverview = new AnalyticsUsersOverview();
            AnalyticsAccount analyticsAccount1 = new AnalyticsAccount { name = "accounts/196069204" };
            analyticsAccount1.AnalyticsUsers.Add(new AnalyticsUser { Email = "user1@mail.com", DirectRoles = "predefinedRoles/admin".Split(',').ToList() });
            analyticsAccount1.AnalyticsUsers.Add(new AnalyticsUser { Email = "user2@mail.com", DirectRoles = "predefinedRoles/editor".Split(',').ToList() });
            analyticsAccount1.AnalyticsUsers.Add(new AnalyticsUser { Email = "usertoremove@mail.com", DirectRoles = "predefinedRoles/admin".Split(',').ToList() });
            analyticsUsersOverview.AnalyticsAccounts.Add(analyticsAccount1);


            ConfigurationObject configurationObject = new ConfigurationObject();
            configurationObject.AccountInfos.Add(new AccountInfo { AnalyticsID = "accounts/196069204", FriendlyName = "nl" });
            configurationObject.AccountInfos.Add(new AccountInfo { AnalyticsID = "accounts/270720121", FriendlyName = "en" });
            configurationObject.Roles.Add(new Role { FriendlyName = "viewer", Rolename = "predefinedRoles/viewer" });
            configurationObject.Roles.Add(new Role { FriendlyName = "analyst", Rolename = "predefinedRoles/analyst" });
            configurationObject.Roles.Add(new Role { FriendlyName = "admin", Rolename = "predefinedRoles/admin" });
            configurationObject.Roles.Add(new Role { FriendlyName = "editor", Rolename = "predefinedRoles/editor" });
            configurationObject.Roles.Add(new Role { FriendlyName = "Blank", Rolename = "Blank" });
            configurationObject.Roles.Add(new Role { FriendlyName = "no-cost-data", Rolename = "predefinedRoles/no-cost-data" });
            configurationObject.Roles.Add(new Role { FriendlyName = "no-revenue-data", Rolename = "predefinedRoles/no-revenue-data" });
            directoryComparer = new DirectoryComparer(directoryUsersOverview, analyticsUsersOverview, configurationObject);
            upsertActions = directoryComparer.GetComparisonResult();
        }

        [Test]
        public void Adding2UsersShouldResultIn2AddedUpsertActions()
        {
            Assert.That(upsertActions.Where(x => x.Action == "Add").Count, Is.EqualTo(2));
        }
        [Test]
        public void Removing1UserShouldResultInOneAddedUpsertActionOfTypeRemove()
        {
            Assert.That(upsertActions.Where(x => x.Action == "Remove").Count, Is.EqualTo(1));
            Assert.That(upsertActions.Where(x => x.Email == "usertoremove@mail.com").Count, Is.EqualTo(1));
        }
        [Test]
        public void AddingInvalidDomainPrefixShouldResultInAErrorObject()
        {
            Assert.That(directoryComparer.LogCollection.Count, Is.EqualTo(5));
        }
    }
}