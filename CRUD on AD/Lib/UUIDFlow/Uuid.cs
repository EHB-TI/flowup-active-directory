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
        public static void createUser()
        {

            string message = "<user><header>" +
                    "<UUID></UUID>" +
                    "<method>CREATE</method>" +
                    "<origin>AD</origin>" +
                    "<version>1</version>" +
                    "<sourceEntityId>abc1</sourceEntityId>" +
                    "<timestamp>" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K") + "</timestamp>" +
                    "</header>" +
                    "<body>" +
                    "<firstname>Tibo</firstname>" +
                    "<lastname>De Munck</lastname>" +
                    "<email>tibo.de.munck@student.dhb.be</email>" +
                    "<birthday>1998-06-03</birthday>" +
                    "<role>student</role>" +
                    "<study>Dig-X</study>" +
                    "</body></user >";



            Task task = new Task(() => ProducerV2.send(message, Severity.UUID.ToString()));

            task.Start();
            ConsumerV2.getMessage();
        }
        public static void deleteUser()
        {
            string message = "<user><header>" +
                    "<UUID></UUID>" +
                    "<method>DELETE</method>" +
                    "<origin>AD</origin>" +
                    "<version></version>" +
                    "<sourceEntityId>abc1</sourceEntityId>" +
                    "<timestamp>" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K") + "</timestamp>" +
                    "</header>" +
                    "<body>" +
                    "<firstname>Tibo</firstname>" +
                    "<lastname>De Munck</lastname>" +
                    "<email>tibo.de.munck@student.dhb.be</email>" +
                    "<birthday>1998-06-03</birthday>" +
                    "<role>student</role>" +
                    "<study>Dig-X</study>" +
                    "</body></user >";



            Task task = new Task(() => ProducerV2.send(message, Severity.UUID.ToString()));
            task.Start();
            ConsumerV2.getMessage();
        }

        public static void UpdateUser()
        {
            string message = "<user><header>" +
                    "<UUID></UUID>" +
                    "<method>UPDATE</method>" +
                    "<origin>AD</origin>" +
                    "<version></version>" +
                    "<sourceEntityId>abc1</sourceEntityId>" +
                    "<timestamp>" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K") + "</timestamp>" +
                    "</header>" +
                    "<body>" +
                    "<firstname>Tibo</firstname>" +
                    "<lastname>De Munck</lastname>" +
                    "<email>tibo.de.munck@student.dhb.be</email>" +
                    "<birthday>1998-06-03</birthday>" +
                    "<role>student</role>" +
                    "<study>Dig-X</study>" +
                    "</body></user >";



            Task task = new Task(() => ProducerV2.send(message, Severity.UUID.ToString()));
            task.Start();
            ConsumerV2.getMessage();
        }
    }
}
