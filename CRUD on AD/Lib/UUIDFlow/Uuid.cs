using Lib.UserFlow;
using Lib.XMLFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.UUIDFlow
{
    /**
     *  Class: Parser and sender of data to the UUID 
     */
    class Uuid
    {
        /**
         *  Methode: Update the UUID based of the method within the user object
         */
        public static void Update(IntraUser outUser)
        {
            //Replace the necessairy properties with the right Values to send over the queue
            outUser.MetaData.Origin = "AD";
            outUser.MetaData.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K");

            /**
             * When some properties of the user are empty, the xml tags alsow dissapear.
             * Even tho, the UUID checks for these empty tags, thus giving an error.
             * So the xml message is HARDCODED with dynamic user data.
             */
            string message = "<user><header>" +
                "<UUID></UUID>" +
                "<method>" + outUser.MetaData.Methode + "</method>" +
                "<origin>" + outUser.MetaData.Origin + "</origin>" +
                "<version></version>" +
                "<sourceEntityId>" + outUser.MetaData.GUID + "</sourceEntityId>" +
                "<timestamp>" + outUser.MetaData.TimeStamp + "</timestamp>" +
                "</header>" +
                "<body>" +
                "<firstname>" + outUser.UserData.FirstName + "</firstname>" +
                "<lastname>" + outUser.UserData.LastName + "</lastname>" +
                "<email>" + outUser.UserData.Email + "</email>" +
                "<birthday>" + outUser.UserData.BirthDay + "</birthday>" +
                "<role>" + outUser.UserData.Role + "</role>" +
                "<study>" + outUser.UserData.Study + "</study>" +
                "</body></user>";

            //Produce a message on the UUID queue
            if (!ProducerV2.Send(message, Severity.UUID.ToString()))
            {
                Console.WriteLine("##################################################");
                Console.WriteLine($"# Producing Message on the UUID Queue has FAILED #");
                Console.WriteLine("##################################################");
            }
        }
    }
}
