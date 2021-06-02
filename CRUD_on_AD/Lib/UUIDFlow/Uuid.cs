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
        public static void Update(IntraUser user)
        {
            ExtraUser outUser = user.ConvertIntraToExtra();
            outUser.MetaData = new MetaData
            {
                GUID = user.MetaData.GUID,
                Methode = user.MetaData.Methode,
                Version = user.MetaData.Version,
                Origin = "AD",
                TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss%K")
            };

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

            ProducerV2.send(message, Severity.UUID.ToString());
        }
    }
}
