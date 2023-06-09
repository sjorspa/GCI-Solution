using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Collections.Generic;

namespace GCI_Function_App.Classes
{
    public class ConfigurationObject
    {
        public ConfigurationObject() { }
        public List<AccountInfo> AccountInfos { get; set; } = new List<AccountInfo>();
        public List<Role> Roles { get; set; } = new List<Role>();


    }
    public class AccountInfo
    {
        public string FriendlyName;
        public string AnalyticsID;
    }
    public class Role
    {
        public string FriendlyName;
        public string Rolename;
    }
}
