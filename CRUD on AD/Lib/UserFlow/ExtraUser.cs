using System;
using System.Xml.Serialization;

namespace Lib
{
    [XmlRoot("user")]
    public class ExtraUser
    {
        [XmlElement("header")]
        public MetaData MetaData { get; set; }
        [XmlElement("body")]
        public XUserData UserData { get; set; }

        public ExtraUser()
        {
            UserData = new XUserData { FirstName = "Not Set", LastName = "Not Set", Email = "Not Set", Role = "Not Set", BirthDay = "1/1/2021", Study = "Not Set" };
            MetaData = new MetaData { UUIDMaster = "Not Set", Methode = CRUDMethode.NOTSET, Origin = "Not Set", TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K"), Version = -1 };
        }

        public override string ToString()
        {
            return $"Account Name: {UserData.FirstName} {UserData.LastName} - {UserData.Email} - {UserData.Role}";
        }
    }

    public class XUserData
    {
        [XmlElement("firstname")]
        public string FirstName { get; set; }
        [XmlElement("lastname")]
        public string LastName { get; set; }
        [XmlElement("email")]
        public string Email { get; set; }
        [XmlElement("birthday")]
        public string BirthDay { get; set; }
        [XmlElement("role")]
        public string Role { get; set; }
        [XmlElement("study")]
        public string Study { get; set; }
    }
}
