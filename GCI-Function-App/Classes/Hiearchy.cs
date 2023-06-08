using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCI_Function_App.Classes
{
    public class Hiearchy
    {
        public Hiearchy()
        {
            Accounts = new List<Account>();
        }
        public List<Account> Accounts { get; set; }
    }
    public class Account
    {
        public Account()
        {
            AccountGroups = new List<AccountGroup>();
        }
        public List<AccountGroup> AccountGroups { get; set; }
        internal string Name;
    }
    public class AccountGroup
    {
        internal string Name;
        internal string GroupId;

        public AccountGroup()
        {
            AccountGroupMembers = new List<AccountGroupMember>();
        }
        public List<AccountGroupMember> AccountGroupMembers { get; set; }
    }
    public class AccountGroupMember
    {
        internal string Email;
        internal string Id;
    }
}
