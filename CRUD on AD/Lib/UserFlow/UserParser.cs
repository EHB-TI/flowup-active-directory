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
        public static DirectoryEntry AssignADObjectAttributesToDirectoryEntry(this ADUser adUser, DirectoryEntry entry)
        {
            entry.Properties["displayName"].Add(adUser.Name);
            entry.Properties["givenName"].Add(adUser.GivenName);
            entry.Properties["sn"].Add(adUser.SN);
            entry.Properties["mail"].Add(adUser.Mail);
            entry.Properties["role"].Add(adUser.Role);
            entry.Properties["sAMAccountName"].Add(adUser.SAMAccountName);
            entry.Properties["userPrincipalName"].Add(adUser.UserPrincipalName);
            return entry;
        }
        public static ADUser DirectoryEntryToADObject(this DirectoryEntry entry)
        {
            var adUser = new ADUser();
            adUser.CN = (string)entry.Properties["cn"].Value;
            adUser.SN = (string)entry.Properties["sn"].Value;
            adUser.Role = (string)entry.Properties["role"].Value;
            adUser.Name = (string)entry.Properties["name"].Value;
            adUser.GivenName = (string)entry.Properties["givenName"].Value;
            adUser.UserPrincipalName = (string)entry.Properties["userPrincipalName"].Value;
            adUser.Mail = (string)entry.Properties["mail"].Value;
            adUser.DisplayName = (string)entry.Properties["displayName"].Value;
            adUser.SAMAccountName = (string)entry.Properties["sAMAccountName"].Value;
            return adUser;
        }
    }
}
