using GCI_Function_App.Business;
using GCI_Function_App.Classes;
using LogAnalytics.Client;
using Newtonsoft.Json;

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
            var directoryUsersOverview = JsonConvert.DeserializeObject<DirectoryUsersOverview>(File.ReadAllText(@"testfiles\default\directoryUsersOverview.json"));
            var analyticsUsersOverview = JsonConvert.DeserializeObject<AnalyticsUsersOverview>(File.ReadAllText(@"testfiles\default\analyticsUsersOverview.json"));
            var configurationObject = JsonConvert.DeserializeObject<ConfigurationObject>(File.ReadAllText(@"testfiles\default\configurationObject.json"));
            directoryComparer = new DirectoryComparer(directoryUsersOverview, analyticsUsersOverview, configurationObject);
            upsertActions = directoryComparer.GetComparisonResult();
        }

        [Test]
        public void OneUserAddedAsAdmin()
        {
            Assert.That(upsertActions.Where(x => x.Action == "Add" && x.Email == "user2@mail.com").Count, Is.EqualTo(1));
        }
        [Test]
        public void OneUserRemovedAsAdmin()
        {
            Assert.That(upsertActions.Where(x => x.Action == "Remove" && x.Email == "usertoremove@mail.com").Count, Is.EqualTo(1));
        }
        [Test]
        public void OneGroupShouldNotMapConfiguration()
        {
            Assert.That(directoryComparer.LogCollection.Where(x => x.type == "error" && x.message == "No account found for nonexistingitem in configuration").Count, Is.EqualTo(1));
        }

    }
}