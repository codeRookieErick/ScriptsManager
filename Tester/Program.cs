using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptsManager.Utils;
namespace ScriptsManager.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = Console.ReadLine();
            using (MyRemoteDesktopServer server = new MyRemoteDesktopServer(10001, 10002))
            {
                while (input != "exit")
                {
                    input = Console.ReadLine();
                }
            }
        }


        static void Tester()
        {
            ShellProccess shellProccess = new ShellProccess();
            shellProccess.OutputReceived += ShellProccess_OutputReceived;
            string input = Console.ReadLine();
            while (input != "exit")
            {
                shellProccess.Exec(input);
                input = Console.ReadLine();
            }
            shellProccess.Kill();
        }

        private static void ShellProccess_OutputReceived(object sender, string e)
        {
            Console.WriteLine(e);
        }
    }
}
