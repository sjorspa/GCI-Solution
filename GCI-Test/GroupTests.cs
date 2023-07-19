using GCI_Function_App.Business;
using GCI_Function_App.Classes;
using LogAnalytics.Client;
using Newtonsoft.Json;

namespace GCI_Test
{
    public class GroupTests
    {
        List<UpsertAction> upsertActions;
        DirectoryComparer directoryComparer;
        [OneTimeSetUp]
        public void Setup()
        {
            var directoryUsersOverview = JsonConvert.DeserializeObject<DirectoryUsersOverview>(File.ReadAllText(@"testfiles\grouptests\directoryUsersOverview.json"));
            var analyticsUsersOverview = JsonConvert.DeserializeObject<AnalyticsUsersOverview>(File.ReadAllText(@"testfiles\grouptests\analyticsUsersOverview.json"));
            var configurationObject = JsonConvert.DeserializeObject<ConfigurationObject>(File.ReadAllText(@"testfiles\grouptests\configurationObject.json"));
            directoryComparer = new DirectoryComparer(directoryUsersOverview, analyticsUsersOverview, configurationObject);
            upsertActions = directoryComparer.GetComparisonResult();
        }

        [Test]
        public void SixGroupsShouldNotBeFound()
        {
            Assert.That(directoryComparer.LogCollection.Where(x => x.type == "error" && x.message.StartsWith("No account found")).Count(), Is.EqualTo(6));
        }

        [Test]
        public void NoUpsertActionsShouldBeInTheDirectoryComparerResult()
        {
            Assert.That(upsertActions.Count, Is.EqualTo(0));
        }

    }
}