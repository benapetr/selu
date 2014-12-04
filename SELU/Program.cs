using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SELU
{
    class SuspiciousEdit
    {
        public int id;
        public int revid;
        public int score;
        public string wiki;
        public DateTime date;
        public string summary;
        public string page;
    }

    class Program
    {
        private static void _log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + ": " + message);
        }

        public static void Log(string message)
        {
            _log(message);
        }

        static void Main(string[] args)
        {
            try
            {
                Log("Connecting to database");
                Npgsql.NpgsqlConnectionStringBuilder connection_sb = new Npgsql.NpgsqlConnectionStringBuilder();
                connection_sb.Database = "wl";
                connection_sb.UserName = "selu";
                connection_sb.Password = "selu";
                connection_sb.Host = "localhost";
                Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(connection_sb);
                connection.Open();
                Log("Getting a list of edits from db");
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand("select * from suspicious_edits_p where is_top = true;", connection);


            }
            catch (Exception fail)
            {
                Console.WriteLine("EXCEPTION: " + fail.Message + "\n Stack trace: " + fail.StackTrace);
            }

        }
    }
}