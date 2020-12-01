using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        ResNet resNet = new ResNet();
        LibraryContext libraryContext;
        public MainController(LibraryContext l)
        {
            libraryContext = l;
        }
        [HttpGet]
        public SendData[] Get()
        {
            return libraryContext.GetAll().ToArray();
        }
        [HttpPut]
        public string Put(SendData input)
        {
            if (!string.IsNullOrEmpty(input.TypeName))
            {
                int i = 0;
                Console.WriteLine(input.Data.GetHashCode());
                lock (libraryContext)
                    i = libraryContext.GetStatistic(input.Data.GetHashCode());
                return i.ToString();
            }
            Console.WriteLine(input.Data.GetHashCode());
            lock (libraryContext)
                input.TypeName = libraryContext.GetFile(input);
            if (string.IsNullOrEmpty(input.TypeName))
            {
                try
                {
                    input.TypeName = resNet.ProcessImage(input.Data);
                    lock (libraryContext)
                        libraryContext.AddResNetResult(input);
                }
                catch
                {
                    input.TypeName = "";
                }
            }
            return input.TypeName;
        }
        [HttpDelete]
        public void Delete()
        {
            lock(libraryContext)
                libraryContext.Clear();
        }
    }
}
