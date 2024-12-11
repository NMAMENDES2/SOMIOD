using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using SOMIOD.Enums;

namespace SOMIOD.Utils
{
    public class DBTransactions
    {
        public static string connstr = Properties.Settings.Default.ConString;
        public static bool nameExists(string name, string table)
        {
            if (name == null || table == null) throw new ArgumentNullException();
            if (!Enum.IsDefined(typeof(TableName), table))
            {
                return false;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();

                    string query = $"SELECT * FROM {table} WHERE name = @name";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            return reader.HasRows;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"SQL Error: {ex.Message}");
                return false;
            }

            catch (Exception ex) { 
                Console.Error.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public static bool doesContainerBelongToApplication(string application, string container)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();

                string query = @"SELECT COUNT(1)
                FROM Application AS app
                INNER JOIN Container AS cont ON cont.parent = app.Id
                WHERE app.name = @name AND cont.name = @containerName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", application);
                    cmd.Parameters.AddWithValue("@containerName", container);

                    int result = (int)cmd.ExecuteScalar();

                    return result > 0;
                }


            }

        }

    }
}