using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;


namespace Lib
{
    public class CRUD
    {
        public DirectoryEntry RootOU { get; set; }
        public Connection Connection { get; set; }

        public void Binding(Connection conn)
        {
            Connection = conn;

            if (conn == Connection.LOCAL)
            {
                RootOU = new DirectoryEntry             //SSL on AD DS is standard
                    ("LDAP://AD-S1-Desiderius-Hogeschool.desideriushogeschool.be/CN=Users,DC=desideriushogeschool,DC=be",
                    "Administrator",
                    "Student1",
                    AuthenticationTypes.Secure
                    );
            }
            if (conn == Connection.LDAP)
            {                                           //SSL connection on port 50001 with ldp.exe works; NOT HERE in C#;
                RootOU = new DirectoryEntry
                    ("LDAP://localhost:50001/CN=User,CN=User,DC=anakin,DC=local", //Example
                    "Administrator",
                    "Student1",
                    AuthenticationTypes.Secure
                    );
            }
        }


        public bool CreateUser(ADUser adUser)
        {
            //Check in AD and UUID for duplicates
            if (IsUserNotInAD(adUser.CN))   
            {
                //Create User object
                //Set:  Name, CN,
                //Not Set:  SN, sAMAccountName, Email, Role, GivenName, DisplayName
                var entry = RootOU.Children.Add(adUser.CN, "user");
                entry.Invoke("SetPassword", new object[] { adUser.UserPassword });
                entry.Properties["LockOutTime"].Value = 0; //unlock account
                adUser.AssignADObjectAttributesToDirectoryEntry(entry);

                entry.CommitChanges();

                Debug.WriteLine("User Creation Succeeded!");
                return true;
            }
            return false;
        }

        public bool DeleteUser(string CN)
        {
            if (IsUserInAD(CN))
            {
                DirectoryEntry objUser = SetupSearcher($"(&(objectCategory=Person)({CN}))").FindOne().GetDirectoryEntry();
                RootOU.Children.Remove(objUser);
                return true;
            }
            return false;
        }

        public ADUser FindADUser(string CN)
        {
            if (IsUserInAD(CN))
            {
                DirectoryEntry objUser = SetupSearcher($"(&(objectCategory=Person)({CN}))", true).FindOne().GetDirectoryEntry();
                return objUser.DirectoryEntryToADObject();
            }
            return null;
        }

        public bool UpdateUser(ADUser oldUser, ADUser newUser)
        {
            if (IsUserInAD(oldUser.CN))
            {
                var objUser = SetupSearcher($"(&(objectCategory=Person)({oldUser.CN}))", true).FindOne().GetDirectoryEntry();

                objUser.Rename(newUser.CN);
                newUser.AssignADObjectAttributesToDirectoryEntry(objUser);

                objUser.UsePropertyCache = true;
                objUser.CommitChanges();

                Console.WriteLine("User succesfully updated!");
                return true;
            }
            return false;
        }

        public List<ADUser> GetADUsers()
        {
            var lstADUsers = new List<ADUser>();
            SearchResultCollection resultCol = SetupSearcher("(&(objectCategory=Person)(objectClass=user))", true).FindAll();

            if (resultCol != null)
            {
                for (int counter = 0; counter < resultCol.Count; counter++)
                {
                    var result = resultCol[counter];
                    var entry = result.GetDirectoryEntry();

                    if (result.Properties.Contains("givenname"))
                    {
                        lstADUsers.Add(entry.DirectoryEntryToADObject());
                    }
                }
            }
            return lstADUsers;
        }

        private DirectorySearcher SetupSearcher(string filter, bool loadAttributes=false) 
        {
            var Searcher = new DirectorySearcher(RootOU); 
            Searcher.Filter = filter; //(&(objectCategory=Person)(objectClass=user))

            if (loadAttributes)
            {
                Searcher.PropertiesToLoad.Add("displayname");
                Searcher.PropertiesToLoad.Add("mail");
                Searcher.PropertiesToLoad.Add("role");
                Searcher.PropertiesToLoad.Add("samaccountname");
                Searcher.PropertiesToLoad.Add("userpassword");
                Searcher.PropertiesToLoad.Add("userprincipalname");
                Searcher.PropertiesToLoad.Add("sn");
                Searcher.PropertiesToLoad.Add("cn");
                Searcher.PropertiesToLoad.Add("givenname");
            }
            return Searcher;
        }
        public bool IsUserInAD(string CN)
        {
            if (SetupSearcher($"(&(objectCategory=Person)({CN}))").FindAll().Count == 0)
            {
                throw new Exception("User does not exists in Active Directory!");
            }
            return true;
        }
        public bool IsUserNotInAD(string CN)
        {
            if (SetupSearcher($"(&(objectCategory=Person)({CN}))").FindAll().Count != 0)
            {
                throw new Exception("User exists in Active Directory!");
            }
            return true;
        }
    }
}
