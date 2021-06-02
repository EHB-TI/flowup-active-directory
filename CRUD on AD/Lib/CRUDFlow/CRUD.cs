using Lib.UserFlow;
using Lib.XMLFlow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Lib
{
    public class CRUD
    {
        public DirectoryEntry RootOU { get; set; }  
        public DirectoryEntry UsersOU { get; set; }
        public Connection Connection { get; set; }

        public void Binding(Connection conn)
        {
            Connection = conn;

            if (conn == Connection.LOCAL)
            {
                RootOU = new DirectoryEntry             //SSL on AD DS is standard
                    ("LDAP://AD-S1-Desiderius-Hogeschool.desideriushogeschool.be/DC=desideriushogeschool,DC=be",
                    "Administrator",
                    "Student1",
                    AuthenticationTypes.Secure
                    );
                UsersOU = RootOU.Children.Find("CN=Users");
            }
            if (conn == Connection.LDAP)
            {                                           //SSL connection on port 50001 with ldp.exe works; NOT HERE in C#;
                UsersOU = new DirectoryEntry
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
                var entry = UsersOU.Children.Add(adUser.CN, "user");
                entry.Properties["LockOutTime"].Value = 0; //unlock account

                adUser.AssignADObjectAttributesToDirectoryEntry(entry);
                

                
                entry.CommitChanges();
                
                //Enable Account -- Cannot change account state from another machine
                const int UF_ACCOUNTDISABLE = 0x0002;
                const int UF_PASSWD_NOTREQD = 0x0020;
                const int UF_PASSWD_CANT_CHANGE = 0x0040;
                const int UF_NORMAL_ACCOUNT = 0x0200;
                const int UF_DONT_EXPIRE_PASSWD = 0x10000;
                const int UF_PASSWORD_EXPIRED = 0x800000;
                //Minimum 7 characters; and not the same password; else Password Policy will interfere
                try
                {
                    entry.Invoke("SetPassword", new object[] { adUser.UserPassword }); //Handle Error
                    AddUserToGroup(entry);
                    Console.WriteLine("User added!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    entry.Invoke("SetPassword", new object[] { "Student1" });
                    Console.WriteLine("#######Password was rejected from the System######");
                    Console.WriteLine($"#   Old Password = {adUser.UserPassword}");
                    Console.WriteLine($"#   New Password = Student1");
                    Console.WriteLine("##################################################");
                }
                
                entry.Properties["userAccountControl"].Value = (UF_NORMAL_ACCOUNT); // (UF_NORMAL_ACCOUNT | UF_ACCOUNTDISABLE) == 0x0202
                entry.CommitChanges();
                
                return true;
            }
            string error = "<error>"+
                "<header>"+
                "<code>3003</code>" +
                "<origin>AD</origin>"+
                "<timestamp>"+DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K")+"</timestamp>"+
                "</header>"+
                "<body>"+
                "<objectUUID></objectUUID>"+
                "<objectSourceId></objectSourceId>"+
                "<objectOrigin>AD</objectOrigin>"+
                "<description>The user could not be added to the AD</description>" +
                "</body>"+
                "</error>";
            ProducerGUI.send(error, Severity.logging.ToString());

            return false;
        }

        public bool DeleteUser(string CN)
        {
            if (IsUserInAD(CN))
            {
                DirectoryEntry objUser = SetupSearcher($"(&(objectCategory=Person)({CN}))").FindOne().GetDirectoryEntry();
                UsersOU.Children.Remove(objUser);
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
            if (IsUserInAD("CN="+oldUser.CN))
            {
                var objUser = SetupSearcher($"(&(objectCategory=Person)(CN={oldUser.CN}))", true).FindOne().GetDirectoryEntry();

                objUser.Rename(newUser.CN);
                if (!newUser.UserPassword.Equals(string.Empty))
                {
                    try
                    {
                        objUser.Invoke("SetPassword", new object[] { newUser.UserPassword });
                    }
                    catch (Exception)
                    {
                        objUser.Invoke("SetPassword", new object[] { "Student1" });
                        Console.WriteLine("#######Password was rejected from the System######");
                        Console.WriteLine($"#   Old Password = {newUser.UserPassword}");
                        Console.WriteLine($"#   New Password = Student1");
                        Console.WriteLine("##################################################");
                    }
                }
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
            var Searcher = new DirectorySearcher(UsersOU); 
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
        public void AddUserToGroup (DirectoryEntry deUser)
        {
            DirectoryEntry grp;
            var de = RootOU.Children.Find("OU=Groups");
            grp = de.Children.Find("CN=licenseUsers", "group");

            Console.WriteLine("First Groep: " + grp.Name + "  ||  " + grp.Parent);

            if (grp != null) { grp.Invoke("Add", new object[] { deUser.Path.ToString() }); }

            grp.Close();

            //var userName = cn;

            //PrincipalContext systemContext;
            //try
            //{
            //    Console.WriteLine("Building System Information");
            //    systemContext = new PrincipalContext(ContextType.Machine, null);
            //}
            //catch (Exception E)
            //{
            //    Console.WriteLine("Failed to create System Context.");
            //    Console.WriteLine("Exception: " + E);

            //    Console.WriteLine();
            //    Console.WriteLine("Press Any Key to Continue");
            //    Console.ReadLine();
            //    return;
            //}

            ////Check if user object already exists
            //Console.WriteLine("Checking if User Exists.");

            //var pcontext = new PrincipalContext(ContextType.Domain, "LDAP://AD-S1-Desiderius-Hogeschool.desideriushogeschool.be/CN=Users,DC=desideriushogeschool,DC=be", "Administrator", "Student1");
            //UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(pcontext, userName);
            //if (userPrincipal != null)
            //{
            //    Console.WriteLine(userName + " exists. Exiting!!");
            //    Console.ReadLine();
            //    return;
            //}    

            //GroupPrincipal groupPrincipal = null;
            //var sAdministrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount)).Value;
            //groupPrincipal = GroupPrincipal.FindByIdentity(systemContext, "licenseUsers");

            //    if (groupPrincipal != null)
            //    {
            //        //check if user is a member
            //        Console.WriteLine("Checking if itadmin is part of Administrators Group");
            //        if (groupPrincipal.Members.Contains(systemContext, IdentityType.SamAccountName, userName))
            //        {
            //            Console.WriteLine("Administrators already contains " + userName);
            //            return;
            //        }
            //        //Adding the user to the group
            //        Console.WriteLine("Adding itadmin to Administrators Group");
            //        groupPrincipal.Members.Add(userPrincipal);
            //        groupPrincipal.Save();
            //        return;
            //    }
            //    else
            //    {
            //        Console.WriteLine("Could not find the group Administrators");
            //    }



            //Console.WriteLine("Cleaning Up");

            //Console.WriteLine();
            //Console.WriteLine("Press Any Key to Continue");
            //Console.ReadLine();
            //return;



        }
    }
}
