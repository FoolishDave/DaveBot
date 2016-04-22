using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DaveBot
{
    static class Program
    {
        public static DaveBot dbot;
        static Task botTask;
        static CancellationTokenSource cancelTokenSource;
        static Window mainWindow;
        static void Main(string[] args)
        {
            mainWindow = new Window();
            Application.EnableVisualStyles();
            Application.Run(mainWindow);
        }

        public static void startBot(String token, Window window)
        {
            cancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancelTokenSource.Token;
            botTask = new Task(() =>
            {
                dbot = new DaveBot(token,window);
            },cancelToken);
            botTask.Start();
            
        }

        public static void connected()
        {
            mainWindow.updateStatus("Connected", Color.Green);
        }

        public static async 
        Task
disconnect(string reason)
        {
            Console.WriteLine("Disconnect Called");
            if (dbot != null)
                await dbot.dc();


            mainWindow.updateStatus(reason, Color.Red);
        }
    }
}
