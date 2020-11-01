using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var c = new CancellationTokenSource();
            var net = new MyLibrary.ResNet();
            Console.WriteLine("Press the ENTER key to cancel...");
            Task cancelTask = Task.Run(() =>
            {
                while (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                    Console.WriteLine("Press the ENTER key to cancel...");
                }
                Console.WriteLine("\nENTER key pressed: cancelling downloads.\n");
                c.Cancel();
            });
            
            net.OnProcessedImage += () =>
            {
                var result = net.GetResult();
                Console.WriteLine(result.FileName + ": " + result.Label);
            };

            await net.ProcessDirectory("t", c.Token);
        }
    }
}
