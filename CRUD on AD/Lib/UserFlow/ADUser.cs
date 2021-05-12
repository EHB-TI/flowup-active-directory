using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class ADUser
    {
        public string CN { get; set; } //Container Name
        public string DisplayName { get; set; } //Username
        public string Name { get; set; } //Full Name
        public string GivenName { get; set; } //First Name
        public string UserPrincipalName { get; set; } //Logon Name
        public string UserPassword { get; set; } //Logon Password
        public string SN { get; set; } //Last Name
        public string Mail { get; set; } //Email
        public string Role { get; set; } //Student or Docent
        public string SAMAccountName { get; set; } //AccountNameIdentifier before 2000
        public string ObjectGUID { get; set; } //Unique Identifier
    }
}
