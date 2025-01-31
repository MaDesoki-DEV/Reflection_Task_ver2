
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DAL
{
    public class Database
    {
        string _connectionString = @"Data Source=DESKTOP-83GJMKS\SQLEXPRESS;Initial Catalog=Reflection_Task;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";
        
        public void CreateTable<T>()
        {
            Type type = typeof(T);

            string tableName = type.Name;
            List<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string columns = "";

                foreach (var item in properties)
                {
                    string dataType = item.PropertyType.Name.ToLower() switch
                    {
                        "string" => "nvarchar(max)",
                        "int32" => "int",
                        "int16" => "smallint",
                        "int64" => "bigint",
                        "decimal" => "money",
                        "double" => "float",
                        "single" => "real",
                        "datetime" => "datetime",
                        "boolean" => "bit",
                        "guid" => "uniqueidentifier",
                        _ => "nvarchar(max)" // Default case for unsupported or unknown types
                    };

                    columns += $"{item.Name.ToLower()} {dataType},";
                    
                }

                string query = $"CREATE TABLE {tableName} ( {columns} )";

                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(ex.Message);
                    }
                    
                }
            }
        }

        public void AddDataToTable<T>(T data)
        {
            Type type = typeof(T);
            string tableName = type.Name;
            List<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            List<string> columnNames = properties.Select(p => p.Name.ToLower()).ToList();
            List<string> columnValues = properties.Select(p => $"'{p.GetValue(data).ToString()}'").ToList();


            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                ///INSERT INTO table_name (column1, column2, column3, ...) VALUES (value1, value2, value3, ...);

                string query = $"INSERT INTO {tableName} ({string.Join(",", columnNames)}) VALUES ({string.Join(",", columnValues)})";
                //string query = $"INSERT INTO {tableName} VALUES ({string.Join(",", columnValues)})";
                using (var command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
