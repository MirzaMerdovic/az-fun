using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace FooFun
{
    public class Program
    {
        public static Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            return host.RunAsync(); ;
        }
    }
}
