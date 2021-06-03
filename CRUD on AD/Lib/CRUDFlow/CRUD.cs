using Lib.UserFlow;
using Lib.XMLFlow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Lib
{
    /**
     *  Class: All CRUD (Create|Read|Update|Delete) Functionality, to interact with the users
     *         in the Active Directory
     */
    public class CRUD
    {
        //Root Container with all objects/containers in Active Directory
        public DirectoryEntry RootOU { get; set; }
        //The 'Users' Container where all users are stored
        public DirectoryEntry UsersOU { get; set; }

        /**
         *  Methode: Make a connection with the Active Directory Database with the protocol LDAP
         */
        public void Binding()
        {
            try
            {
                //Using the Admin credentials
                RootOU = new DirectoryEntry
                    ("LDAP://AD-S1-Desiderius-Hogeschool.desideriushogeschool.be/DC=desideriushogeschool,DC=be",
                    "Administrator",
                    "Student1",
                    AuthenticationTypes.Secure
                    );
                try
                {
                    UsersOU = RootOU.Children.Find("CN=Users");
                }
                catch (Exception)
                {
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#        Finding 'Users' Container FAILED        #");
                    Console.WriteLine("##################################################");
                    Environment.Exit(-1);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The connection could not be made to the AD Database: " + e.Message);
                Thread.Sleep(2000);
                Console.WriteLine("Retrying Connection!");
                Binding();
            }
        }

        /**
         *  Methode: Create a new user in the Active Directory Database
         *      @param1 => Incoming user from GUI
         */
        public bool CreateUser(ADUser adUser)
        {
            //Check in Active Directory DB if the user DOES NOT exist
            if (!IsUserInAD(adUser.CN))
            {
                //Create new AD User and add to Users Container
                var entry = UsersOU.Children.Add(adUser.CN, "user");
                entry.Properties["LockOutTime"].Value = 0;

                //Assign user data to the Active Directory user object
                adUser.AssignADObjectAttributesToDirectoryEntry(entry);
                entry.CommitChanges();

                //Enable Account -- Most used flags
                //const int UF_ACCOUNTDISABLE = 0x0002;
                //const int UF_PASSWD_NOTREQD = 0x0020;
                const int UF_NORMAL_ACCOUNT = 0x0200;
                entry.Properties["userAccountControl"].Value = (UF_NORMAL_ACCOUNT); // (UF_NORMAL_ACCOUNT | UF_ACCOUNTDISABLE) == 0x0202


                //Minimum 7 characters; and not the same password; else the Password Policy will interfere
                try
                {
                    entry.Invoke("SetPassword", new object[] { adUser.UserPassword });
                }
                catch (Exception)
                {
                    //If current password gets rejected, a default password is used
                    entry.Invoke("SetPassword", new object[] { "Student1" });
                    Console.WriteLine("#######Password was rejected from the System######");
                    Console.WriteLine($"#   Old Password = {adUser.UserPassword}");
                    Console.WriteLine($"#   New Password = Student1");
                    Console.WriteLine("##################################################");
                }
                //Adding users to a security group called "licenseUsers"
                AddUserToGroup(entry);

                try
                {
                    //Commit Changes into the Active Directory DB
                    entry.CommitChanges();
                }
                catch (Exception)
                {
                    //If the commit has failed the CRUD has been halted
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#            Commit Changes FAILED               #");
                    Console.WriteLine("##################################################");
                    return false;
                }
                return true;
            }
            else
            {
                Console.WriteLine("User allready exists with the given name!");
                return false;
            }
        }

        /**
         *  Methode: Delete an user in the Active Directory Database
         *      @param1 => Container name of AD user
         */
        public bool DeleteUser(string CN)
        {
            //Check in Active Directory DB if the user DOES exists
            if (IsUserInAD(CN))
            {
                //Find the AD user by Container name; and use this varibale to delete the user from the Active Directory DB
                DirectoryEntry objUser = SetupSearcher($"(&(objectCategory=Person)({CN}))").FindOne().GetDirectoryEntry();
                try
                {
                    UsersOU.Children.Remove(objUser);
                }
                catch (Exception)
                {
                    //If the system rejects the Deletion;
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#   Removing user from Active Directory FAILED   #");
                    Console.WriteLine("##################################################");
                }
                return true;
            }
            else
            {
                Console.WriteLine("User does not exist in the current Active Directory DB!");
                return false;
            }
        }

        /**
        *  Methode: Update a user in the Active Directory Database
        *      @param1 => The old version of the user
        *      @param2 => The updated version of the user
        */
        public bool UpdateUser(ADUser oldUser, ADUser newUser)
        {
            //Check in Active Directory DB if the user DOES exists
            if (IsUserInAD(oldUser.CN))
            {
                //Find the Active Directory object with the Container name of the old user
                var objUser = SetupSearcher($"(&(objectCategory=Person)({oldUser.CN}))", true).FindOne().GetDirectoryEntry();

                try
                {
                    //Rename current Active Directory object
                    objUser.Rename(newUser.CN);
                }
                catch (Exception)
                {
                    //If the system rejects, renaming the object;
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#   Renaming user from Active Directory FAILED   #");
                    Console.WriteLine($"#       => Name probably allready exists         #");
                    Console.WriteLine("##################################################");
                }
                

                //If password is empty; than leave the previous password
                if (!newUser.UserPassword.Equals(string.Empty))
                {
                    try
                    {
                        //Invoke the 'Setpassword' function to change the password
                        objUser.Invoke("SetPassword", new object[] { newUser.UserPassword });
                    }
                    catch (Exception)
                    {
                        //If current password gets rejected, the previous password is used
                        Console.WriteLine("#######Password was rejected from the System######");
                        Console.WriteLine($"#         Previous password reinstated           #");
                        Console.WriteLine("##################################################");
                    }
                }
                //Assign user data to the Active Directory user object
                newUser.AssignADObjectAttributesToDirectoryEntry(objUser);
                objUser.UsePropertyCache = true;
                try
                {
                    //Commit Changes into the Active Directory DB
                    objUser.CommitChanges();
                }
                catch (Exception)
                {
                    //If the commit has failed the CRUD has been halted
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#            Commit Changes FAILED               #");
                    Console.WriteLine("##################################################");
                    return false;
                }
                return true;
            }
            else
            {
                Console.WriteLine("User does not exist in the current Active Directory DB!");
                return false;
            }
        }

        /**
        *  Methode: Find an individual user in the Active Directory Database
        *      @param1 => Container name of AD user
        */
        public ADUser FindADUser(string CN)
        {
            //Check in Active Directory DB if the user DOES exists
            if (IsUserInAD(CN))
            {
                //Find the AD user by Container name; and use this varibale to return the user from the Active Directory DB
                DirectoryEntry objUser = SetupSearcher($"(&(objectCategory=Person)({CN}))", true).FindOne().GetDirectoryEntry();
                return objUser.DirectoryEntryToADObject();
            }
            else
            {
                Console.WriteLine("User does not exist in the current Active Directory DB!");
                return null;
            }
        }

        /**
        *  Methode: Get all users from the Active Directory Database
        */
        public List<ADUser> GetADUsers()
        {
            //Initialize List to catch incoming users
            var lstADUsers = new List<ADUser>();

            //Find all users
            SearchResultCollection resultCol = SetupSearcher("(&(objectCategory=Person)(objectClass=user))", true).FindAll();

            //Check if results are not null
            if (resultCol != null)
            {
                //Parse each Active Directory object into a workable user object
                for (int counter = 0; counter < resultCol.Count; counter++)
                {
                    var result = resultCol[counter];
                    var entry = result.GetDirectoryEntry();

                    if (result.Properties.Contains("givenname"))
                    {
                        lstADUsers.Add(entry.DirectoryEntryToADObject());
                    }
                }
                return lstADUsers;
            }
            else
            {
                Console.WriteLine("No users found in the current Active Directory DB!");
                return null;
            }
        }

        /**
        *  Methode: Setup the DirectorySearcher to find a user and its attributes
        *      @param1 => Filter to find a user, based on Container name
        *      @param2 => a bool to get all attributes associated with the user, or not
        */
        private DirectorySearcher SetupSearcher(string filter, bool loadAttributes = false)
        {
            //Initialize Searcher
            var Searcher = new DirectorySearcher(UsersOU)
            {
                Filter = filter //(&(objectCategory=Person)(objectClass=user))
            };

            //Add attributes to search if bool=true
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

        /**
        *  Methode: Check if an an individual user exists in the Active Directory Database
        *      @param1 => Container name of AD user
        */
        public bool IsUserInAD(string CN)
        {
            return SetupSearcher($"(&(objectCategory=Person)({CN}))").FindAll().Count != 0;
        }

        /**
        *  Methode: Add a user in a security group in Active Directory
        *      @param1 => The Active Directory object/user
        */
        public void AddUserToGroup(DirectoryEntry deUser)
        {
            try
            {
                //Get the Groups Container and find the group
                var de = RootOU.Children.Find("OU=Groups");
                var grp = de.Children.Find("CN=licenseUsers", "group");
                try
                {
                    //Check if group is not null 
                    if (grp != null) 
                    {
                        //Add the Distinguished Name of the user to the group
                        grp.Invoke("Add", new object[] { deUser.Path.ToString() }); 
                    }
                    grp.Close();
                }
                catch (Exception)
                {
                    Console.WriteLine("##################################################");
                    Console.WriteLine($"#          Adding User to group FAILED           #");
                    Console.WriteLine("##################################################");
                    grp.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("##################################################");
                Console.WriteLine($"#         Finding Group Container FAILED         #");
                Console.WriteLine("##################################################");
            }
        }
    }
}
