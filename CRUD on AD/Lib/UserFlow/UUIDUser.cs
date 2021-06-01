using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lib.UserFlow
{
    [XmlRoot("user")]
    public class UUIDUser
    {

        [XmlElement("header")]
        public XMetaData MetaData { get; set; }
        [XmlElement("body")]
        public UserData UserData { get; set; }

        public UUIDUser()
        {
            UserData = new UserData { FirstName = "Not Set", LastName = "Not Set", Email = "Not Set", Role = "Not Set", Password = "Student1", BirthDay = "1/1/2021", Study = "Not Set" };
            MetaData = new XMetaData { UUIDMaster = "Not Set", Methode = CRUDMethode.NOTSET, Origin = "Not Set", TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K")};
        }
    }


    public class XMetaData
    {
        [XmlElement("UUID")]
        public string UUIDMaster { get; set; }
        [XmlElement("method")]
        public CRUDMethode Methode { get; set; }
        [XmlElement("origin")]
        public string Origin { get; set; }
        [XmlElement("version")]
        public string TimeStamp { get; set; }
    }
}
