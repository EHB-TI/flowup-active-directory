using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public static class UserParser
    {
        public static User ADObjectToUserObject(this ADUser adUser)
        {
            return new User
            {
                UserData = new UserData
                {
                    FirstName = adUser.GivenName,
                    LastName = adUser.SN,
                    Email = adUser.Mail,
                    Role = adUser.Role
                },
                MetaData = new MetaData
                {
                    GUID = adUser.ObjectGUID,
                }
            };
        }
        public static ADUser UserObjectToADObject(this User user)
        {
            return new ADUser
            {
                CN = $"CN={user.UserData.FirstName} {user.UserData.LastName}",
                SN = user.UserData.LastName,
                Name = $"{user.UserData.FirstName} {user.UserData.LastName}",
                DisplayName = $"{user.UserData.FirstName} {user.UserData.LastName}",
                GivenName = user.UserData.FirstName,
                UserPrincipalName = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}@desideriushogeschool.be",
                Mail = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}@desideriushogeschool.be",
                SAMAccountName = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}",
                Role = user.UserData.Role
            };
        }
        public static void AssignADObjectAttributesToDirectoryEntry(this ADUser adUser, DirectoryEntry entry)
        {
            entry.Properties["displayName"].Value = adUser.Name;
            entry.Properties["givenName"].Value = adUser.GivenName;
            entry.Properties["sn"].Value = adUser.SN;
            entry.Properties["mail"].Value = adUser.Mail;
            entry.Properties["role"].Value = adUser.Role;
            entry.Properties["sAMAccountName"].Value = adUser.SAMAccountName;
            entry.Properties["userPrincipalName"].Value = adUser.UserPrincipalName;
        }
        public static ADUser DirectoryEntryToADObject(this DirectoryEntry entry)
        {
            return new ADUser
            {
                CN = (string)entry.Properties["cn"].Value,
                SN = (string)entry.Properties["sn"].Value,
                Role = (string)entry.Properties["role"].Value,
                Name = (string)entry.Properties["name"].Value,
                GivenName = (string)entry.Properties["givenName"].Value,
                UserPrincipalName = (string)entry.Properties["userPrincipalName"].Value,
                Mail = (string)entry.Properties["mail"].Value,
                DisplayName = (string)entry.Properties["displayName"].Value,
                SAMAccountName = (string)entry.Properties["sAMAccountName"].Value
            };
        }
    }
}
