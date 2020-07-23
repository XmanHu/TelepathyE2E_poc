
namespace EchoClient
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using EchoService;
    using SessionAPI;

    class Program
    {
        private static int n = 50000;

        static async Task Main(string[] args)
        {
            //await PerfTest();
            //await NormalE2E();
            //await StreamE2E();
            await StreamPerfTest();
        }

        static async Task NormalE2E()
        {
            string uri = "https://localhost:8008";
            var client = new SessionClient(uri);

            await client.SendTask("echo.Echoer", "Echo", new EchoRequest() { Name = "Jialiang" });

            var result = await client.GetResult<EchoReply>();

            Console.WriteLine(result.Message);
        }

        static async Task StreamE2E()
        {
            string uri = "https://localhost:8008";
            var client = new SessionClient(uri);

            var call = client.CreateRequestStreamCall();

            await call.SendTaskStream("echo.Echoer", "Echo", new EchoRequest() {Name = "Jialiang Stream"});
            await call.EndOfRequest();

            var resultCall = client.CreateResponseStreamCall<EchoReply>();
            var result = await resultCall.GetResultStream();
            foreach (var r in result)
            {
                Console.WriteLine(r.Message);
            }
        }

        static async Task PerfTest()
        {
            string uri = "https://localhost:8008";
            var client = new SessionClient(uri, 100);
            var sw = new Stopwatch();
            Task[] tasks = new Task[n - 1];
            await client.SendTask("echo.Echoer", "Echo", new EchoRequest() { Name = "Jialiang" });
            sw.Start();

            for (int i = 0; i < n - 1; i++)
            {
                tasks[i] = client.SendTask("echo.Echoer", "Echo", new EchoRequest() { Name = "Jialiang" });
            }

            await Task.WhenAll(tasks);
            sw.Stop();

            var sendInterval = sw.ElapsedMilliseconds;

            await Task.Delay(5000);

            var result = await client.GetResult<EchoReply>();
            sw.Restart();

            for (int i = 0; i < n - 1; i++)
            {
                tasks[i] = Task.Run(async () => await client.GetResult<EchoReply>());
            }

            await Task.WhenAll(tasks);
            sw.Stop();

            Console.WriteLine("request throughput: {0:N0}", (double)(n - 1) / sendInterval * 1000);
            Console.WriteLine("response throughput: {0:N0}", (double)(n - 1) / sw.ElapsedMilliseconds * 1000);
        }

        static async Task StreamPerfTest()
        {
            string uri = "https://localhost:8008";
            var client = new SessionClient(uri, 1);
            var sw = new Stopwatch();

            var call = client.CreateRequestStreamCall();

            sw.Start();

            for (int i = 0; i < n; i++)
            {
                await call.SendTaskStream("echo.Echoer", "Echo", new EchoRequest() { Name = "Jialiang Stream" + i});
            }

            await call.EndOfRequest();

            sw.Stop();

            var sendInterval = sw.ElapsedMilliseconds;
            await Task.Delay(5000);
            var resultCall = client.CreateResponseStreamCall<EchoReply>(n);

            sw.Restart();

            var result = await resultCall.GetResultStream();
            sw.Stop();

            //foreach (var echoReply in result)
            //{
            //    Console.WriteLine(echoReply.Message);
            //}

            Console.WriteLine("request throughput: {0:N0}", (double)n / sendInterval * 1000);
            Console.WriteLine("response throughput: {0:N0}", (double)(n) / sw.ElapsedMilliseconds * 1000);
        }
    }
}
