using OfficeOpenXml;
using System.Linq;
using System.Reflection;
using DAL;

namespace BL
{
    /// <summary>
    /// This class is responsible for checking the compatability between excel file and a given class.
    /// </summary>
    public class ExcelToObject<T> where T : new()
    {
        
        string _filePath;
        Type _type;
        Dictionary<int, PropertyInfo> _columnsMapping = new();

        public ExcelToObject(string filePath)
        {
            this._filePath = filePath;
            this._type = typeof(T);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public (bool, string) Check()
        {
            Dictionary<string, PropertyInfo> nameInfoPair = new();
            foreach (var item in _type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                nameInfoPair.Add(item.Name.ToLower(), item);

            using (var package = new ExcelPackage(this._filePath))
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
                            return (false, $"'{cellValue}' was not found in '{_type.Name}' class.");

                        this._columnsMapping.Add(i, nameInfoPair[cellValue]);

                    }
                }
            }

            return (true, null);
        }

        private List<T> ParseData()
        {
            List<T> newList = new List<T>();

            using (var package = new ExcelPackage(this._filePath))
            {
                // Select the first worksheet
                using (var sheet = package.Workbook.Worksheets[0])
                {
                    // Iterate through the cells of the first column
                    for (int i = 2; i <= sheet.Dimension.Rows; i++)
                    {

                        T newObject = new T();

                        foreach (var item in _columnsMapping)
                        {
                            item.Value.SetValue(newObject, sheet.Cells[i, item.Key].Value);
                        }

                        newList.Add(newObject);
                    }
                }
            }

            return newList;
        }

        public void UploadToDataBase()
        {
            Database dataBase = new();
            dataBase.CreateTable<T>();
            this.ParseData().ForEach(obj => dataBase.AddDataToTable(obj));
        }
    }
}
