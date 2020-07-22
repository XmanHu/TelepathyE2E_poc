using System;
using System.Threading.Tasks;
using EchoService;
using Google.Protobuf;
using SessionAPI;

namespace EchoClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string uri = "https://localhost:8008";
            var client = new SessionClient(uri);

            await client.SendTask("echo.Echoer","Echo", new EchoRequest(){Name = "Jialiang"});


            var result = await client.GetResult<EchoReply>();

            Console.WriteLine(result.Message);
        }
    }
}
