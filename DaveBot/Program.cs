using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DaveBot
{
    static class Program
    {
        public static DapperBot dbot;
        static void Main(string[] args)
        {
            if (Type.GetType("Mono.Runtime") == null)
            {
                AllocConsole();
            } else
            {
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
                Console.SetError(new StreamWriter(Console.OpenStandardError()));
                Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            }

            dbot = new DapperBot();
        }


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);
    }
}
