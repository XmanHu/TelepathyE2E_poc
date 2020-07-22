

namespace Frontend
{

    using System.Threading.Tasks;
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using Telepathy;
    using System.Collections.Concurrent;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Net.Client;

    public class FrontendService : Frontend.FrontendBase
    {
        private readonly ILogger<FrontendService> _logger;
        private Backend.BackendClient backendClient;

        public FrontendService(ILogger<FrontendService> logger)
        {
            _logger = logger;

            //var channel = GrpcChannel.ForAddress("https://localhost:8002");
            var channel = new Channel("localhost:8002", ChannelCredentials.Insecure);
            backendClient = new Backend.BackendClient(channel);
        }

        public override async Task<Empty> SendTask(InnerRequest request, ServerCallContext context)
        {
            Task.Run(async () =>
            {
                var result = await backendClient.dispatchAsync(request);
                ResponseQueue.queue.Enqueue(result);
            });
            return new Empty();
        }

        public override async Task<InnerResponse> GetResult(Empty request, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (ResponseQueue.queue.TryDequeue(out var temp))
                {
                    return temp;
                }

                await Task.Delay(500);
            }

            return new InnerResponse();
        }
    }
}
