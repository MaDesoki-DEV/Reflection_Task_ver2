using BL;
using DAL;

namespace PL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var EV = new ExcelToObject<Car>("car.xlsx");

            

            Console.WriteLine(EV.Check().Item1 ? "true" : EV.Check().Item2);

            EV.UploadToDataBase();
        }
    }
}
