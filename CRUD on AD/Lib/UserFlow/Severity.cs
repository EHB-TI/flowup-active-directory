using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.UserFlow
{
    /**
     *  Class: Enumeration of all the Queue's in RabbitMQ that can be used
     */
    public enum Severity
    {
        UUID, user, AD, GUI, logging
    }
}
