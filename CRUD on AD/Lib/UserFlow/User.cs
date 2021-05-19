using System;
using System.Xml.Serialization;

namespace Lib
{
    [XmlRoot("user")]
    public class User
    {
        [XmlElement("header")]
        public MetaData MetaData { get; set; }
        [XmlElement("body")]
        public UserData UserData { get; set; }

        public User()
        {
            UserData = new UserData { FirstName = "Not Set", LastName = "Not Set", Email = "Not Set", Role = "Not Set", Password = "Student1" };
            MetaData = new MetaData { UUIDMaster = "Not Set", Methode = CRUDMethode.NOTSET, Origin = "Not Set", TimeStamp = DateTime.MinValue };
        }

        public override string ToString()
        {
            return $"Account Name: {UserData.FirstName} {UserData.LastName} - {UserData.Email} - {UserData.Role}";
        }
    }

    public class MetaData
    {
        [XmlElement("UUID")]
        public string UUIDMaster { get; set; }
        [XmlElement("method")]
        public CRUDMethode Methode { get; set; }
        [XmlElement("origin")]
        public string Origin { get; set; }
        [XmlElement("timestamp")]
        public DateTime TimeStamp { get; set; }
        [XmlIgnore]
        public string GUID { get; set; }    
    }

    public class UserData
    {
        [XmlElement("firstname")]
        public string FirstName { get; set; }
        [XmlElement("lastname")]
        public string LastName { get; set; }
        [XmlIgnore]
        public string Password { get; set; }
        [XmlElement("email")]
        public string Email { get; set; }
        [XmlElement("role")]
        public string Role { get; set; }
    }

    public enum CRUDMethode
    {
        CREATE, UPDATE, DELETE, READ, NOTSET
    }
}
