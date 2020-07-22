

using System.Linq;

namespace HostAgent
{
    using System;
    using System.Threading.Tasks;
    using Grpc.Core;
    using HostAgent.Service;
    using Telepathy;

    class Program
    {
        static async Task Main(string[] args)
        {
            var address = new Uri("http://localhost:8002");

            var target = "https://localhost:5001";

            Server server = new Server
            {
                Services =
                {
                    Backend.BindService(new BackendService(target))
                },
                Ports =
                {
                    { "localhost", 8002, ServerCredentials.Insecure}
                }
            };

            server.Start();
            Console.WriteLine("Application started.");
            Console.WriteLine("Server ip = {0}", address);
            Console.WriteLine($"{server.Ports.FirstOrDefault().Host} {server.Ports.FirstOrDefault().Port}");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            await server.ShutdownAsync();
        }
    }
}
