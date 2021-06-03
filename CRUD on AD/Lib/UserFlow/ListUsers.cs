using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lib.UserFlow
{
    /**
     *  Class: Wrapper to give the GUI all the users in an 'XML Array' from the Active Directory (Only used with the method READ_ALL)
     */
    public static class ListUsers
    {
        [XmlArray("users")]
        public static List<ADUser> List { get; set; }
    }
}
