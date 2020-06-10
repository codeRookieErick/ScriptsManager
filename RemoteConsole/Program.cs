using ScriptsManager.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RemoteConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int managerPort = -1;
            int consolePort = -1;
            bool showHead = true;
            string host = "";
            Console.Title = "Remote Console";
            Print("Scripts Manager Remote Console", ConsoleColor.Red);
            Console.WriteLine();

            if (
                !(args.Length >= 2 && int.TryParse(args[1], out managerPort))
                && !int.TryParse(ConfigurationManager.AppSettings["managerPort"], out managerPort)
            )
            {
                Print("Can't set manager port", ConsoleColor.Red);
                Console.ReadKey();
                return;
            }

            if (
                !(args.Length >= 3 && int.TryParse(args[2], out consolePort))
                && !int.TryParse(ConfigurationManager.AppSettings["consolePort"], out consolePort)

            )
            {
                Print("Can't set console port", ConsoleColor.Red);
                Console.WriteLine();
                Console.ReadKey();
                return;
            }

            if (args.Length >= 4 && args[3] == "nohead") showHead = false;

            if (string.IsNullOrEmpty(host) && args.Length > 0) host = args[0];
            if (string.IsNullOrEmpty(host)) host = ConfigurationManager.AppSettings["host"];
            if (string.IsNullOrEmpty(host)) host = Dns.GetHostName();
            if (string.IsNullOrEmpty(host))
            {
                Print("Can't set the host", ConsoleColor.Red);
                Console.ReadKey();
                return;
            }

            SocketsLayer socketsLayer = new SocketsLayer(managerPort, consolePort, host);
            socketsLayer.PacketReceived += (sender, packet) => {
                Console.Title = $"{DateTime.Now.ToString("hh:mm:ss")} # {packet.SenderHostame}";
                if (showHead)
                {
                    Print(DateTime.Now.ToString("hh:mm:ss"), ConsoleColor.Yellow);
                    Print(" # ", ConsoleColor.White);
                    Print(packet.SenderHostame, ConsoleColor.Green);
                    Console.WriteLine();
                    Console.WriteLine(Encoding.UTF8.GetString(packet.Data));
                    Console.WriteLine();
                    InitInput();
                }
                else
                {
                    Console.WriteLine(Encoding.UTF8.GetString(packet.Data));
                }
            };

            InitInput();
            string input = Console.ReadLine();
            while (input != "exit")
            {
                socketsLayer.Send(Encoding.UTF8.GetBytes(input));
                input = Console.ReadLine();
                if (input == "clear")
                {
                    Console.Clear();
                    InitInput();
                }
            }
            socketsLayer.Dispose();
        }

        static void InitInput()
        {
            Print(" >> ", ConsoleColor.Red);
        }
        static void Print(string data, ConsoleColor consoleColor = default)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = (consoleColor == default) ? oldColor: consoleColor;
            Console.Write(data);
            Console.ForegroundColor = oldColor;
        }

    }
}
