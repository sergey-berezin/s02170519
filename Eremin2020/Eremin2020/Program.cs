using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eremin2020
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
            var tasks = net.ProcessDirectory("pictures", c.Token);

            var query = from i in tasks
                        where !i.IsCompleted
                        select i;
            while (query.Count() > 0)
            {
                await Task.WhenAny(query);
                var result = net.GetResult();
                Console.WriteLine(result.FileName + ": " + result.Label);
            }
        }
    }
}
