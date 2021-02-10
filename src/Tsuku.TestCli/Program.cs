using System;
using System.IO;
using System.Text;

namespace Tsuku.TestCli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var fi = new FileInfo("tst.txt");
            fi.Create();

            fi.SetTsukuAttribute("Hesllo", Encoding.UTF8.GetBytes("hello world"));


            foreach (var f in fi.GetTsukuAttributeInfos(true))
            {
                string fString = Encoding.UTF8.GetString(fi.GetTsukuAttribute(f.Name, true));
                Console.WriteLine($"{f.Name}: {f.Size} -- {fString}");
            }

            Console.ReadKey();
        }
    }
}
