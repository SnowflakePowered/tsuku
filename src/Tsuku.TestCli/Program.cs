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

            fi.SetTsukuAttribute("Hello", Encoding.UTF8.GetBytes("hello world"));

            foreach (var f in fi.GetTsukuAttributeInfos())
            {
                Console.WriteLine($"{f.Name}: {f.Size}");
            }
            Console.ReadKey();
        }
    }
}
