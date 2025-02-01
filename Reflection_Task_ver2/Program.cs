using BLL;
using DAL;

namespace PL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var EV = new ExcelDataMapper<Car>("car.xlsx");

            (bool validated, string errorMsg) = EV.ValidateColumnHeaders();

            if (validated)
            {
                Console.WriteLine("Headers validated.");
                InterActionWithDatabase.UploadData(EV.MapExcelToObjects());
                Console.WriteLine("Data Uploaded!!.");
            }
            else
                Console.WriteLine(errorMsg);

        }
    }
}
