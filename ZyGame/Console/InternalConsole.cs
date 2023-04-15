using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZyGame
{
    internal class InternalConsole
    {

        public static void WriteLine(object message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(DateTime.Now.ToString("G") + " | INFO | ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message + "\n");
            Console.ForegroundColor = color;
        }

        public static void WriteWarning(object message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now.ToString("G") + " | WARN | ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message + "\n");
            Console.ForegroundColor = color;
        }

        public static void WriteError(object message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(DateTime.Now.ToString("G") + " | ERRO | ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message + "\n");
            Console.ForegroundColor = color;
        }
    }
}
