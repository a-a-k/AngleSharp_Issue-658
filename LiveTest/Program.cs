using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using LiveTest.Adapters;
using LiveTest.Search;

namespace LiveTest
{
    class Program
    {
        private static readonly List<(Type type, string zone)> Adapters = new List<(Type, string)>
        {
            //(typeof(PiluliAdapter), "volGograd"), // Okay if running first
            (typeof(EaptekaAdapter), "tver"),
            (typeof(AptekaruAdapter), "30"),
            (typeof(ZdravcityAdapter), "samara"),
            (typeof(PiluliAdapter), "volGograd"), // Encoding issue if running in another order
            (typeof(Ru366Adapter), "78"),
        };

        static void Main(string[] args)
        {
            foreach (var adapter in Adapters)
            {
                var services = new ServiceCollection()
                    .AddTransient(adapter.type)
                    .BuildServiceProvider();

                var service = (IAdapter)services.GetRequiredService(adapter.type);

                var cmd = new SearchCommand
                {
                    Query = "но-шпа",
                    ZoneId = adapter.zone,
                };

                var result = service.Execute(cmd).Result;

                Console.WriteLine(adapter);
                Console.WriteLine(result.SourceZoneName);
                var item = result.Items[0];
                Console.WriteLine(item.Name);
                Console.WriteLine("---------------------------------------------------");
            }

            Console.Read();
        }
    }
}
