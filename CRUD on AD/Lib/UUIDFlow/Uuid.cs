using Lib.UserFlow;
using Lib.XMLFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.UUIDFlow
{
    class Uuid
    {
        public static void Update(string xml)
        {
            ProducerV2.send(xml, Severity.UUID.ToString());
        }
    }
}
