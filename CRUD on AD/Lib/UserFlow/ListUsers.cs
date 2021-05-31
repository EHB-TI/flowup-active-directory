using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lib.UserFlow
{
    static class ListUsers
    {
        [XmlElement("users")]
        public static List<ADUser> List { get; set; }
    }
}
