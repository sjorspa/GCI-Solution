using GCI_Function_App.Business;
using GCI_Function_App.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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





            DirectoryGroup directoryGroup1 = new DirectoryGroup { Name = "nl_admin"};
            directoryGroup1.GroupMembers.Add(new GroupMember { Email = "user1@mail.com" });
            directoryGroup1.GroupMembers.Add(new GroupMember { Email = "user2@mail.com" });
            directoryGroup1.GroupMembers.Add(new GroupMember { Email = "user3@mail.com" });
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup1);

            DirectoryGroup directoryGroup2 = new DirectoryGroup { Name = "en_admin" };
            directoryGroup2.GroupMembers.Add(new GroupMember { Email = "user1@mail.com" });
            directoryGroup2.GroupMembers.Add(new GroupMember { Email = "user2@mail.com" });
            directoryGroup2.GroupMembers.Add(new GroupMember { Email = "user3@mail.com" });
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup2);

            DirectoryGroup directoryGroup3 = new DirectoryGroup { Name = "xxxxxxxxxxxx_admin" };
            directoryGroup3.GroupMembers.Add(new GroupMember { Email = "user1@mail.com" });
            directoryGroup3.GroupMembers.Add(new GroupMember { Email = "user2@mail.com" });
            directoryGroup3.GroupMembers.Add(new GroupMember { Email = "user3@mail.com" });
            directoryUsersOverview.DirectoryGroups.Add(directoryGroup3);

            AnalyticsUsersOverview analyticsUsersOverview = new AnalyticsUsersOverview();
            AnalyticsAccount analyticsAccount1 = new AnalyticsAccount { name = "accounts/196069204" };
            analyticsAccount1.AnalyticsUsers.Add(new AnalyticsUser { Email = "user1@mail.com", DirectRoles = "predefinedRoles/admin".Split(',').ToList() });
            analyticsAccount1.AnalyticsUsers.Add(new AnalyticsUser { Email = "user2@mail.com", DirectRoles = "predefinedRoles/editor".Split(',').ToList() });
            analyticsAccount1.AnalyticsUsers.Add(new AnalyticsUser { Email = "usertoremove@mail.com", DirectRoles = "predefinedRoles/admin".Split(',').ToList() });
            analyticsUsersOverview.AnalyticsAccounts.Add(analyticsAccount1);

            directoryComparer = new DirectoryComparer(directoryUsersOverview, analyticsUsersOverview);
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
            Assert.That(directoryComparer.LogCollection.Count,Is.EqualTo(1));
        }
    }
}