using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCI_Function_App.Classes
{
    public class DirectoryUsersOverview
    {
        public DirectoryUsersOverview()
        {
            DirectoryGroups = new List<DirectoryGroup>();
        }
        public List<DirectoryGroup> DirectoryGroups { get; set; }
    }
    public class DirectoryGroup
    {
        public string Name;
        public string GroupId;

        public DirectoryGroup()
        {
            GroupMembers = new List<GroupMember>();
        }
        public List<GroupMember> GroupMembers { get; set; }
    }
    public class GroupMember
    {
        public string Email;
        public string Id;
    }
}
