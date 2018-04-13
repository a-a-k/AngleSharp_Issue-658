using System;
using System.Collections.Generic;
using System.Text;
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
            /* !!! */
            // If make this enable, all the stuff works fine without any relation to executions order. But doesn't if called in the ctor of PiluliAdapter class directly.
            // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            #region Variant with foreach() - does not work

            foreach (var adapter in Adapters)
            {
                var service = (IAdapter)Activator.CreateInstance(adapter.type);
                var cmd = new SearchCommand
                {
                    Query = "но-шпа",
                    ZoneId = adapter.zone,
                };

                var result = service.Execute(cmd).Result;
                Console.WriteLine(service);
                Console.WriteLine(result.SourceZoneName);
                Console.WriteLine("---------------------------------------------------");
            }

            #endregion

            #region Variant with for() - does not work

            //for (var i = 1; i <= 3; i++)
            //{
            //    IAdapter service = null;
            //    var zone = string.Empty;
            //    switch (i)
            //    {
            //        case 1:
            //            service = new Ru366Adapter();
            //            zone = "78";
            //            break;
            //        case 2:
            //            service = new PiluliAdapter();
            //            zone = "volgograd";
            //            break;
            //        case 3:
            //            service = new EaptekaAdapter();
            //            zone = "tver";
            //            break;
            //    }

            //    var cmd = new SearchCommand
            //    {
            //        Query = "но-шпа",
            //        ZoneId = zone,
            //    };

            //    var result = service.Execute(cmd).Result;
            //    Console.WriteLine(service);
            //    Console.WriteLine(result.SourceZoneName);
            //    Console.WriteLine("---------------------------------------------------");
            //}

            #endregion

            #region Variant without any loop - works fine

            //var service1 = (IAdapter)Activator.CreateInstance(typeof(EaptekaAdapter)); // or simple new EaptekaAdapter() instead
            //var service2 = (IAdapter)Activator.CreateInstance(typeof(PiluliAdapter)); // or simple new PiluliAdapter(); instead
            //var service3 = (IAdapter)Activator.CreateInstance(typeof(Ru366Adapter)); // or simple new Ru366Adapter(); instead
            //var cmd1 = new SearchCommand
            //{
            //    Query = "но-шпа",
            //    ZoneId = "tver",
            //};
            //var cmd2 = new SearchCommand
            //{
            //    Query = "но-шпа",
            //    ZoneId = "volgograd",
            //};
            //var cmd3 = new SearchCommand
            //{
            //    Query = "но-шпа",
            //    ZoneId = "78",
            //};

            //var result1 = service1.Execute(cmd1).Result;
            //Console.WriteLine(service1);
            //Console.WriteLine(result1.SourceZoneName);
            //Console.WriteLine("---------------------------------------------------");

            //var result2 = service2.Execute(cmd2).Result;
            //Console.WriteLine(service2);
            //Console.WriteLine(result2.SourceZoneName);
            //Console.WriteLine("---------------------------------------------------");

            //var result3 = service3.Execute(cmd3).Result;
            //Console.WriteLine(service3);
            //Console.WriteLine(result3.SourceZoneName);
            //Console.WriteLine("---------------------------------------------------");

            #endregion

            Console.Read();
        }
    }
}
