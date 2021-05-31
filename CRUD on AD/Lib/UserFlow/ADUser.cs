using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lib
{
    [XmlRoot("user")]
    public class ADUser
    {
        [XmlElement("cn")]
        public string CN { get; set; } //Container Name
        [XmlElement("displayName")]
        public string DisplayName { get; set; } //Username
        [XmlElement("name")]
        public string Name { get; set; } //Full Name
        [XmlElement("givenName")]
        public string GivenName { get; set; } //First Name
        [XmlElement("userPrincipalName")]
        public string UserPrincipalName { get; set; } //Logon Name
        [XmlElement("userPassword")]
        public string UserPassword { get; set; } //Logon Password
        [XmlElement("sn")]
        public string SN { get; set; } //Last Name
        [XmlElement("mail")]
        public string Mail { get; set; } //Email
        [XmlElement("role")]
        public string Role { get; set; } //Student or Docent
        [XmlElement("sAMAccountName")]
        public string SAMAccountName { get; set; } //AccountNameIdentifier before 2000
        [XmlElement("objectGuid")]
        public string ObjectGUID { get; set; } //Unique Identifier
        [XmlElement("birthday")]
        public string BirthDay { get; set; } //Birthday
        [XmlElement("objectVersion")]
        public int ObjectVersion { get; set; }  //Iterative Version ID
        [XmlElement("study")]
        public string Study { get; set; }   //Study Direction
    }
}
