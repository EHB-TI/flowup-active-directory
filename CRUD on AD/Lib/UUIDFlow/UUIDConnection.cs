using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Lib.UUIDFlow
{
    public class UUIDConnection
    {
        public MySqlConnection Conn { get; set; }

        private const string IP = "10.3.56.10";
        private const string DB_NAME = "FlowUpDB";
        private const string USERNAME = "ActiveD";
        private const string PASSWORD = "ActiveD2021";

        public UUIDConnection()
        {
            Conn = new MySqlConnection($"Data Source={IP};Initial Catalog={DB_NAME};User ID={USERNAME};password={PASSWORD}; connection timeout=30");
            Conn.Open();
        }
    }
}