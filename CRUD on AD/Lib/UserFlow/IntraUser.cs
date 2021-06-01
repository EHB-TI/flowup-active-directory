using System;
using System.Xml.Serialization;

namespace Lib
{
    [XmlRoot("user")]
    public class IntraUser
    {
        [XmlElement("header")]
        public MetaData MetaData { get; set; }
        [XmlElement("body")]
        public UserData UserData { get; set; }
        public IntraUser()
        {
            UserData = new UserData { FirstName = "Not Set", LastName = "Not Set", Email = "Not Set", Role = "Not Set", Password = "Student1", BirthDay = "1/1/2021", Study="Not Set" };
            MetaData = new MetaData { UUIDMaster = "Not Set", Methode = CRUDMethode.NOTSET, Origin = "Not Set", TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K"), Version = -1 };
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
        [XmlElement("version")]
        public int Version { get; set; }
        [XmlElement("sourceEntityId")]
        public string GUID { get; set; }    
        [XmlElement("timestamp")]
        public string TimeStamp { get; set; }
    }

    public class UserData
    {
        [XmlElement("firstname")]
        public string FirstName { get; set; }
        [XmlElement("lastname")]
        public string LastName { get; set; }
        [XmlElement("password")]
        public string Password { get; set; }
        [XmlElement("email")]
        public string Email { get; set; }
        [XmlElement("birthday")]
        public string BirthDay { get; set; }
        [XmlElement("role")]
        public string Role { get; set; }
        [XmlElement("study")]
        public string Study { get; set; }
    }   

    public enum CRUDMethode
    {
        CREATE, UPDATE, DELETE, READ, NOTSET, READ_ALL
    }
}
