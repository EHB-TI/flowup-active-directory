using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Lib.UUIDFlow
{
    public static class UUIDParser
    {
        public static string GetGUIDFromUUID(MySqlConnection conn, string uuid)
        {
            var query = "SELECT";
            //var command = new MySqlCommand(query, conn);
            //var reader = command.ExecuteReader();
            //Console.WriteLine("");
            return "";
        }
        public static bool IsUserInUUID(UUIDConnection conn, string uuid)
        {
            return true;
        }
        public static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }
        public static bool UpdateUUID(User user)
        {
            /* Change:  
                -Version number
            */
            return true;
        }
        public static bool CheckUUID()
        {
            return false;
        }
    }
}
