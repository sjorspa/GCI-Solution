using Google.Apis.Admin.Directory.directory_v1.Data;
using System;
using System.Collections.Generic;

namespace GCI_Function_App.Classes
{
    public class ConfigurationObject
    {
        public ConfigurationObject() { }
        public List<AnalyticsAccountInfo> AccountInfos { get; set; } = new List<AnalyticsAccountInfo>();
        public List<Role> Roles { get; set; } = new List<Role>();
        public List<GroupRoleMapping> GroupRoleMappings { get; set; } = new List<GroupRoleMapping>();

        public List<String> ProtectedAccunts { get; set; } = new List<String>();

    }
    public class AnalyticsAccountInfo
    {
        public string FriendlyName;
        public string AnalyticsID;
    }

    public class GroupRoleMapping
    {
        public string GroupName;
        public string RoleName;
        public string Account;
    }

    public class Role
    {
        public string FriendlyName;
        public string Rolename;
    }
}
