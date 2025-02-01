using OfficeOpenXml;
using System.Linq;
using System.Reflection;
using DAL;

namespace BLL
{

    /// <summary>
    /// This class is responsible for checking the compatibility between an Excel file and a given class `T`,
    /// parsing the Excel data into a list of objects of type `T`, and uploading the data to a database.
    /// </summary>
    /// <typeparam name="T">The class type to map Excel data to. Must have a parameterless constructor.</typeparam>
    public class ExcelDataMapper<T> where T : new()
    {
        
        private string _excelFilePath; // Path to the Excel file.
        private Type _targetType; // Type of the Class 'T'.
        private Dictionary<int, PropertyInfo> _columnToPropertyMap; // Dictionary used to map Excel columns to the corresponding class properties.

        /// <summary>
        /// Initializes a new instance of the ExcelToObject class.
        /// </summary>
        /// <param name="filePath">Path to the Excel file.</param>
        public ExcelDataMapper(string filePath)
        {
            this._excelFilePath = filePath;
            this._targetType = typeof(T);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set EPPlus package license to non-commercial.
        }

        /// <summary>
        /// Checks if the Excel file's column headers match the properties of the class `T`.
        /// </summary>
        /// <returns>A tuple indicating success (true/false) and an error message if any.</returns>
        public (bool, string) ValidateColumnHeaders()
        {
            _columnToPropertyMap = new();
            Dictionary<string, PropertyInfo> nameInfoPair = new();
            foreach (var item in _targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                nameInfoPair.Add(item.Name.ToLower(), item);

            using (var package = new ExcelPackage(this._excelFilePath))
            {
                // Select the first worksheet
                using (var sheet = package.Workbook.Worksheets[0])
                {
                    // Iterate through the cells of the first column
                    for (int i = 1; i <= sheet.Dimension.Columns; i++)
                    {
                        string cellValue = sheet.Cells[1, i].Value.ToString().ToLower().Trim();

                        if (string.IsNullOrEmpty(cellValue))
                            continue;
                        else if (!nameInfoPair.Keys.Contains(cellValue))
                            return (false, $"'{cellValue}' was not found in '{_targetType.Name}' class.");

                        this._columnToPropertyMap.Add(i, nameInfoPair[cellValue]);

                    }
                }
            }

            return (true, null);
        }

        /// <summary>
        /// Parses the Excel data into a list of objects of type `T`.
        /// </summary>
        /// <returns>A list of objects of type `T` populated with Excel data.</returns>
        public List<T> MapExcelToObjects()
        {
            List<T> newList = new List<T>();

            using (var package = new ExcelPackage(this._excelFilePath))
            {
                // Select the first worksheet
                using (var sheet = package.Workbook.Worksheets[0])
                {
                    // Iterate through the cells of the first column
                    for (int i = 2; i <= sheet.Dimension.Rows; i++)
                    {

                        T newObject = new T();

                        foreach (var item in _columnToPropertyMap)
                        {
                            item.Value.SetValue(newObject, sheet.Cells[i, item.Key].Value);
                        }

                        newList.Add(newObject);
                    }
                }
            }

            return newList;
        }

    }

    public static class InterActionWithDatabase
    {
        public static void UploadData<T>(List<T> data)
        {
            Database db = new();
            db.AddDataToTable(data);
        }
    }
}
