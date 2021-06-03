using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    /**
     *  Class: A custom parser to convert an user object into an Intra-, Extra-, AD- or DirectoryEntry-user
     */
    public static class UserParser
    {
        public static IntraUser ADObjectToIntraUserObject(this ADUser adUser) =>
            new IntraUser
            {
                UserData = new UserData
                {
                    FirstName = adUser.GivenName,
                    LastName = adUser.SN,
                    Email = adUser.Mail,
                    Role = adUser.Role,
                    Study = adUser.Study,
                    BirthDay = adUser.BirthDay,
                    Password = adUser.UserPassword
                },
                MetaData = new MetaData
                {
                    GUID = adUser.ObjectGUID,
                    Version = adUser.ObjectVersion
                }
            };
        public static ExtraUser ConvertIntraToExtra(this IntraUser user) =>
            new ExtraUser
            {
                UserData = new XUserData
                {
                    FirstName = user.UserData.FirstName,
                    LastName = user.UserData.LastName,
                    Email = user.UserData.Email,
                    Role = user.UserData.Role,
                    Study = user.UserData.Study,
                    BirthDay = user.UserData.BirthDay
                },
                MetaData = new MetaData
                {
                    GUID = user.MetaData.GUID,
                    Version = user.MetaData.Version,
                }
            };
        public static ADUser IntraUserObjectToADObject(this IntraUser user) =>
            new ADUser
            {
                CN = $"CN={user.UserData.FirstName} {user.UserData.LastName}",
                SN = user.UserData.LastName,
                Name = $"{user.UserData.FirstName} {user.UserData.LastName}",
                DisplayName = $"{user.UserData.FirstName} {user.UserData.LastName}",
                GivenName = user.UserData.FirstName,
                UserPrincipalName = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}@desideriushogeschool.be",
                Mail = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}@desideriushogeschool.be",
                SAMAccountName = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}",
                Role = user.UserData.Role,
                ObjectGUID = user.MetaData.GUID,
                Study = user.UserData.Study,
                BirthDay = user.UserData.BirthDay,
                ObjectVersion = user.MetaData.Version,
                UserPassword = user.UserData.Password
            };
        public static ADUser ExtraUserObjectToADObject(this ExtraUser user) =>
            new ADUser
            {
                CN = $"CN={user.UserData.FirstName} {user.UserData.LastName}",
                SN = user.UserData.LastName,
                Name = $"{user.UserData.FirstName} {user.UserData.LastName}",
                DisplayName = $"{user.UserData.FirstName} {user.UserData.LastName}",
                GivenName = user.UserData.FirstName,
                UserPrincipalName = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}@desideriushogeschool.be",
                Mail = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}@desideriushogeschool.be",
                SAMAccountName = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}",
                Role = user.UserData.Role,
                ObjectGUID = user.MetaData.GUID,
                Study = user.UserData.Study,
                BirthDay = user.UserData.BirthDay,
                ObjectVersion = user.MetaData.Version
            };
        public static void AssignADObjectAttributesToDirectoryEntry(this ADUser adUser, DirectoryEntry entry)
        {
            entry.Properties["displayName"].Value = adUser.Name;
            entry.Properties["givenName"].Value = adUser.GivenName;
            entry.Properties["sn"].Value = adUser.SN;
            entry.Properties["mail"].Value = adUser.Mail;
            entry.Properties["role"].Value = adUser.Role;
            entry.Properties["sAMAccountName"].Value = adUser.SAMAccountName;
            entry.Properties["userPrincipalName"].Value = adUser.UserPrincipalName;
            entry.Properties["study"].Value = adUser.Study;
            entry.Properties["birthday"].Value = adUser.BirthDay;
            entry.Properties["objectVersion"].Value = adUser.ObjectVersion;
        }
        public static ADUser DirectoryEntryToADObject(this DirectoryEntry entry) =>
            new ADUser
            {
                CN = (string)entry.Properties["cn"].Value,
                SN = (string)entry.Properties["sn"].Value,
                Name = (string)entry.Properties["name"].Value,
                GivenName = (string)entry.Properties["givenName"].Value,
                UserPrincipalName = (string)entry.Properties["userPrincipalName"].Value,
                Mail = (string)entry.Properties["mail"].Value,
                DisplayName = (string)entry.Properties["displayName"].Value,
                SAMAccountName = (string)entry.Properties["sAMAccountName"].Value,
                //Custom Attributes can be empty in some users, check is needed to avoid NullPointers
                Role = ((string)entry.Properties["role"].Value is null) ? "Not Set" : (string)entry.Properties["role"].Value ,
                Study = ((string)entry.Properties["study"].Value is null) ? "Not Set" : (string)entry.Properties["study"].Value,
                BirthDay = ((string)entry.Properties["birthday"].Value is null) ? "Not Set" : (string)entry.Properties["birthday"].Value,
                ObjectVersion = (int?)entry.Properties["objectVersion"].Value is null? -1 : (int)entry.Properties["objectVersion"].Value,
                ObjectGUID = new Guid((byte[])entry.Properties["objectGUID"].Value).ToString()
            };
    }
}
