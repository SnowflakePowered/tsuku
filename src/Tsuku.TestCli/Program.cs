using System;
using System.IO;
using System.Text;
using Tsuku.Extensions;

namespace Tsuku.TestCli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var fi = new FileInfo("tst.txt");
            var fi2 = new FileInfo("tst.symlink.txt");
            fi.Create();

            fi.SetAttribute("Hesllo", Encoding.UTF8.GetBytes("hello world"));
            fi2.SetAttribute("SymlinkAttr", "mySymlinkAttr", false);


            foreach ((string fName, long fSize) in fi2.EnumerateAttributeInfos(true))
            {
                string fString = Encoding.UTF8.GetString(fi2.GetAttribute(fName, true));
                Console.WriteLine($"{fName}: {fSize} -- {fString}");
            }
            Console.WriteLine("symlink===");
            foreach ((string fName, long fSize) in fi2.EnumerateAttributeInfos(false))
            {
                string fString = Encoding.UTF8.GetString(fi2.GetAttribute(fName, false));
                Console.WriteLine($"{fName}: {fSize} -- {fString}");
            }


            Console.ReadKey();
        }
    }
}
