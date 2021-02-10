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
            fi2.SetAttribute("SymlinkAttr", "mySymlinkAttr");


            foreach ((string fName, long fSize) in fi2.EnumerateAttributeInfos())
            {
                string fString = Encoding.UTF8.GetString(fi2.GetAttribute(fName));
                Console.WriteLine($"{fName}: {fSize} -- {fString}");
            }

            fi2.DeleteAttribute("SymlinkAttr");
            Console.WriteLine("deleted..");
            foreach ((string fName, long fSize) in fi.EnumerateAttributeInfos())
            {
                string fString = Encoding.UTF8.GetString(fi2.GetAttribute(fName));
                Console.WriteLine($"{fName}: {fSize} -- {fString}");
            }

            Console.ReadKey();
        }
    }
}
