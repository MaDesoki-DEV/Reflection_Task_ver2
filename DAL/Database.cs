
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;

namespace DAL
{
    /// <summary>
    /// The class provides functionality to interact with a SQL Server database.
    /// </summary>
    /// <remarks>
    /// It dynamically creates tables based on the properties of a given class and inserts data into those tables.
    /// </remarks>
    public class Database
    {
        string _connectionString = @"Data Source=DESKTOP-83GJMKS\SQLEXPRESS;Initial Catalog=Reflection_Task;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";

        /// <summary>
        /// Dynamically creates a table in the database based on the properties of the class <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The class type whose properties will define the table columns.</typeparam>
        /// <param name="tableName">The name of the table to be created.</param>
        /// <exception cref="InvalidOperationException">Thrown if the table creation query fails.</exception>
        /// <remarks>
        /// This method uses reflection to retrieve the properties of the class <typeparamref name="T"/>.
        /// It maps each property to its corresponding SQL Server data type and constructs a CREATE TABLE query.
        /// </remarks>
        private void CreateTable<T>(string tableName)
        {
            Type type = typeof(T);

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

        /// <summary>
        /// Inserts a list of objects of type <typeparamref name="T"/> into a dynamically created table.
        /// </summary>
        /// <typeparam name="T">The class type of the objects to be inserted.</typeparam>
        /// <param name="data">The list of objects to be inserted into the table.</param>
        /// <remarks>
        /// This method generates a unique table name using the class name and the current timestamp.
        /// It creates the table using the <see cref="CreateTable{T}"/> method and inserts the data into the table.
        /// </remarks>
        public void AddDataToTable<T>(List<T> data)
        {
            Type type = typeof(T);
            string tableName = $"{type.Name}_{DateTime.Now.ToString("ddMMyyyHHmmss")}";
            CreateTable<T>(tableName);


            List<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            List<string> columnNames = properties.Select(p => p.Name.ToLower()).ToList();


            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var item in data)
                {
                    List<string> columnValues = properties.Select(p => $"'{p.GetValue(item).ToString()}'").ToList();

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
}
