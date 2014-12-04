using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SELU
{
    class SuspiciousEdit
    {
        public string id;
        public string revid;
        public string score;
        public string wiki;
        public string date;
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
                Npgsql.NpgsqlCommand query = new Npgsql.NpgsqlCommand("select id, revid, score, wiki, date, summary, page from suspicious_edits_p where is_top = true and wiki = 'en.wikipedia.org';", connection);
                Npgsql.NpgsqlDataReader dr = query.ExecuteReader();
                while (dr.Read())
                {
                    if (dr.FieldCount < 7)
                    {
                        throw new Exception("Wrong number of result columns");
                    }
                    SuspiciousEdit edit = new SuspiciousEdit();
                    edit.id = dr[0].ToString();
                    edit.revid = dr[1].ToString();
                    edit.score =  dr[2].ToString();
                    edit.wiki = dr[3].ToString();
                    //edit.date = (DateTime)dr[4];
                    edit.summary = dr[5].ToString();
                    edit.page = dr[6].ToString();
                    edits.Add(edit);
                }
                Log("Processing: " + edits.Count);
                foreach (SuspiciousEdit ed in edits)
                {
                    Log("Processing edit to " + ed.page);
                    string result = new System.Net.WebClient().DownloadString("http://en.wikipedia.org/w/api.php?action=query&prop=revisions&titles=" + System.Web.HttpUtility.UrlEncode(ed.page) + "&rvprop=timestamp|user|comment|content&format=xml");
                    if (!result.Contains("<rev revid=\""))
                    {
                        Log("There is no revid for " + ed.page);
                        continue;
                    }
                    string revid = result.Substring(result.IndexOf("<rev revid=\"") + "<rev revid=\"".Length);
                    revid = revid.Substring(0, revid.IndexOf("\""));
                    if (revid != ed.revid)
                    {
                        Console.WriteLine("NOT " + ed.page);
                        query = new Npgsql.NpgsqlCommand("update se set is_top=false where id=" + ed.id + ";");
                        int r = query.ExecuteNonQuery();
                        if (r != 1)
                            Log("ERROR: Affected " + r + " rows!!"); 
                    }
                    else
                    {
                        Console.WriteLine("Ignoring " + ed.page);
                    }
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