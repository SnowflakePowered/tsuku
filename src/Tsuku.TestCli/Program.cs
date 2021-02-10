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
            var fi2 = new FileInfo("tst.symlink.txt");

            fi.SetTsukuAttribute("Hesllo", Encoding.UTF8.GetBytes("hello world"));
            fi2.SetTsukuAttribute("Wosrld", Encoding.UTF8.GetBytes("goodbye worldshjhd"));


            foreach (var f in fi2.GetTsukuAttributeInfos(false))
            {
                Console.WriteLine($"{f.Name}: {f.Size}");
            }

            //foreach (var f in fi2.GetTsukuAttributeInfos(true))
            //{
            //    Console.WriteLine($"{f.Name}: {f.Size}");
            //}

            Console.ReadKey();
        }
    }
}
