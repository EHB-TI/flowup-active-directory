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
                    ("LDAP://AD-S1-Desiderius-Hogeschool.desideriushogeschool.be/CN=User,DC=desideriushogeschool,DC=be",
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


        public bool CreateUser(User user)
        {
            //Create user info
            DirectoryEntry objUser;
            var objName = $"CN={user.UserData.FirstName} {user.UserData.LastName}";
            var name = $"{user.UserData.FirstName} {user.UserData.LastName}";

            //Check in AD and UUID for duplicates
            if (SetupSearcher($"(&(objectCategory=Person)({objName}))").FindAll().Count != 0)
            {
                throw new Exception("User allready exists!");
            }

            //Create User object
            //Set:  Name, CN,
            //Not Set:  SN, sAMAccountName, Email, Role, GivenName, DisplayName
            objUser = RootOU.Children.Add(objName, "user");
            objUser.Properties["displayName"].Add(name);
            objUser.Properties["givenName"].Add(user.UserData.FirstName);
            objUser.Properties["sn"].Add(user.UserData.LastName);
            objUser.Properties["mail"].Add(user.UserData.Email);
            objUser.Properties["role"].Add(user.UserData.Role);
            objUser.Properties["sAMAccountName"].Add($"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant()}");
            objUser.Properties["userPrincipalName"].Add($"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}@desideriushogeschool.be");
            objUser.Properties["userPassword"].Add(user.UserData.Password);

            //Enable Account -- Cannot change account state from another machine
            const int UF_ACCOUNTDISABLE = 0x0002;
            const int UF_PASSWD_NOTREQD = 0x0020;
            const int UF_PASSWD_CANT_CHANGE = 0x0040;
            const int UF_NORMAL_ACCOUNT = 0x0200;
            const int UF_DONT_EXPIRE_PASSWD = 0x10000;
            const int UF_SMARTCARD_REQUIRED = 0x40000;
            const int UF_PASSWORD_EXPIRED = 0x800000;
            //objUser.Properties["userAccountControl"].Value = (UF_NORMAL_ACCOUNT);

            objUser.CommitChanges();

            Debug.WriteLine("User Creation Succeeded!");
            return true;
        }

        public bool DeleteUser(string name)
        {
            DirectoryEntry objUser = SetupSearcher($"(&(objectCategory=Person)({name}))").FindOne().GetDirectoryEntry(); ;
            RootOU.Children.Remove(objUser);

            return true;
        }

        public User FindADUser(string v)
        {
            DirectoryEntry objUser = SetupSearcher($"(&(objectCategory=Person)({v}))", true).FindOne().GetDirectoryEntry();

            User user = new User();
            user.UserData.FirstName = (String)objUser.Properties["givenname"].Value;
            user.UserData.LastName = (String)objUser.Properties["sn"].Value;
            user.UserData.Email = (String)objUser.Properties["userprincipalname"].Value;
            user.UserData.Role = (String)objUser.Properties["role"].Value;
            //user.UserData.Password = new String('*', objUser.Properties["userpassword"].Capacity);

            return user;
        }

        public bool UpdateUser(string oldName, User user)
        {
            var objUser = SetupSearcher($"(&(objectCategory=Person)({oldName}))", true).FindOne().GetDirectoryEntry();

            var objName = $"CN={user.UserData.FirstName} {user.UserData.LastName}";
            var name = $"{user.UserData.FirstName} {user.UserData.LastName}";

            objUser.Rename(objName);
            objUser.Properties["givenname"][0] = user.UserData.FirstName;
            objUser.Properties["userPrincipalName"][0] = $"{(user.UserData.Email.Contains("@") ? user.UserData.Email.Substring(0, user.UserData.Email.IndexOf("@")) : user.UserData.Email)}@desideriushogeschool.be";
            //objUser.Properties["userPassword"][0] = user.UserData.Password;
            objUser.Properties["displayName"][0] = name; //Multi
            objUser.Properties["sn"][0] = user.UserData.LastName;
            objUser.Properties["mail"][0] = user.UserData.Email;
            objUser.Properties["role"][0] = user.UserData.Role;
            objUser.Properties["sAMAccountName"][0] = $"{user.UserData.FirstName.ToLowerInvariant()}.{user.UserData.LastName.ToLowerInvariant().Replace(" ", ".")}";

            objUser.UsePropertyCache = true;
            objUser.CommitChanges();
            Console.WriteLine("User succesfully updated!");

            return true;
        }

        public List<String> GetADUsers()
        {
            List<String> lstADUsers = new List<String>();
            SearchResult result;
            SearchResultCollection resultCol = SetupSearcher("(&(objectCategory=Person)(objectClass=user))", true).FindAll();

            if (resultCol != null)
            {
                for (int counter = 0; counter < resultCol.Count; counter++)
                {
                    result = resultCol[counter];
                    var res = result.GetDirectoryEntry();

                    if (result.Properties.Contains("givenname"))
                    {
                        lstADUsers.Add((String)res.Properties["cn"].Value);
                    }
                }
            }
            return lstADUsers;
        }

        public DirectorySearcher SetupSearcher(string filter, bool loadAttributes=false) 
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
    }
}
