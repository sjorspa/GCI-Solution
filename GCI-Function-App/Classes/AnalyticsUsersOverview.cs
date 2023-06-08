using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCI_Function_App.Classes
{
    public class AnalyticsUsersOverview
    {
        public AnalyticsUsersOverview()
        {
            AnalyticsAccounts = new List<AnalyticsAccount>();
        }
        public List<AnalyticsAccount> AnalyticsAccounts { get; set; }
    }
    public class AnalyticsAccount
    {
        public string name;
        public string DisplayName;

        public AnalyticsAccount()
        {
            AnalyticsUsers = new List<AnalyticsUser>();
        }
        public List<AnalyticsUser> AnalyticsUsers { get; set; }
    }
    public class AnalyticsUser
    {
        public string Email;
        public IList<string> DirectRoles;
        public string Name;
    }
}
