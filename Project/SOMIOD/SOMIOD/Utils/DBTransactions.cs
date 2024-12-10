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
        string connstr = Properties.Settings.Default.ConString;
        public bool nameExists(string name, string table)
        {
            if (name == null || table == null) throw new ArgumentNullException();
            if (!Enum.IsDefined(typeof(TableName), table)) throw new InvalidOperationException();

            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();

                    string query = $"SELECT COUNT(1) FROM {table} WHERE name = @name";

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


    }
}