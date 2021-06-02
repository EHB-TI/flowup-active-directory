using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lib.UserFlow
{
    public static class ListUsers
    {
        [XmlArray("users")]
        public static List<ADUser> List { get; set; }
    }
}
