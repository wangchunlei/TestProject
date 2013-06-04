using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Caching;

namespace AppFabricTest
{
    class Program
    {
        private static DataCacheFactory _factory = null;
        private static DataCache _cache = null;

        static void Main(string[] args)
        {
            var cache = GetCache();
            var value = cache.Get("time");
            Console.WriteLine(value);
            Console.ReadKey(false);
        }
        public static DataCache GetCache()
        {
            if (_cache != null)
                return _cache;

            //Define Array for 1 Cache Host
            List<DataCacheServerEndpoint> servers = new List<DataCacheServerEndpoint>(1);

            //Specify Cache Host Details
            //  Parameter 1 = host name
            //  Parameter 2 = cache port number
            servers.Add(new DataCacheServerEndpoint("192.168.1.31", 22233));

            //Create cache configuration
            DataCacheFactoryConfiguration configuration = new DataCacheFactoryConfiguration();

            //Set the cache host(s)
            configuration.Servers = servers;

            //Set default properties for local cache (local cache disabled)
            configuration.LocalCacheProperties = new DataCacheLocalCacheProperties();

            //Disable tracing to avoid informational/verbose messages on the web page
            DataCacheClientLogManager.ChangeLogLevel(System.Diagnostics.TraceLevel.Off);

            //Pass configuration settings to cacheFactory constructor
            _factory = new DataCacheFactory(configuration);

            //Get reference to named cache called "default"
            _cache = _factory.GetCache("default");

            return _cache;
        }
    }
}
