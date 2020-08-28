using FastMember;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace BradescoPGPConsole
{
    public static class Bulk
    {
        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> list)
        {
            if (!list.Any())
                return;

            using (var connection = new SqlConnection(dbContext.Database.Connection.ConnectionString))
            {
                connection.Open();

                using (var bcp = new SqlBulkCopy(connection))
                {
                    using (var reader = ObjectReader.Create(list, GetProperties(typeof(T))))
                    {
                        try
                        {
                            bcp.DestinationTableName = typeof(T).Name;
                            bcp.BulkCopyTimeout = 120;
                            bcp.WriteToServer(reader);
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                            {
                                string pattern = @"\d+";
                                Match match = Regex.Match(ex.Message.ToString(), pattern);
                                var index = Convert.ToInt32(match.Value) - 1;

                                FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
                                var sortedColumns = fi.GetValue(bcp);
                                var items = (Object[])sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

                                FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
                                var metadata = itemdata.GetValue(items[index]);

                                var column = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                                var length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                                throw new Exception(String.Format("Column: {0} contains data with a length greater than: {1}", column, length));
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        private static String[] GetProperties(Type type)
        {
            return type.GetProperties().Where(p => !p.PropertyType.IsClass && !p.PropertyType.IsAbstract || p.PropertyType == typeof(string)).Select(p => p.Name).ToArray();
        }
    }
}
