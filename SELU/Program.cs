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
        public static List<SuspiciousEdit> edits = new List<SuspiciousEdit>();
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
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand("select id, revid, score, wiki, date, summary, page from suspicious_edits_p where is_top = true and wiki = 'en.wikipedia';", connection);
                Npgsql.NpgsqlDataReader dr = query.ExecuteReader();
                while (dr.Read())
                {
                    if (dr.FieldCount < 7)
                    {
                        throw new Exception("Wrong number of result columns");
                    }
                    SuspiciousEdit edit = new SuspiciousEdit();
                    edit.id = (int)dr[0];
                    edit.revid = (int)dr[1];
                    edit.score = (int)dr[2];
                    edit.wiki = dr[3].ToString();
                    edit.date = (DateTime)dr[4];
                    edit.summary = dr[5].ToString();
                    edit.page = dr[6].ToString();
                    edits.Add(edit);
                }
                Log("Logging in to wiki");
                DotNetWikiBot.Site wiki = new DotNetWikiBot.Site("en.wikipedia.org");
                foreach (SuspiciousEdit ed in edits)
                {
                    Log("Processing edit to " + ed.page);
                }
                connection.Close();

            }
            catch (Exception fail)
            {
                Console.WriteLine("EXCEPTION: " + fail.Message + "\n Stack trace: " + fail.StackTrace);
            }

        }
    }
}