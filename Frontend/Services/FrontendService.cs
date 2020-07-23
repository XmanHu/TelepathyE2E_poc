

using System.Threading;

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
                ResponseQueue.queue.Enqueue(await backendClient.dispatchAsync(request));
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

        public override async Task<Empty> SendTaskStream(IAsyncStreamReader<InnerRequest> requestStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                Task.Run(async () =>
                {
                    ResponseQueue.queue.Enqueue(await backendClient.dispatchAsync(request));
                });
            }

            return new Empty();
        }

        public override async Task GetResultStream(AskNumber request, IServerStreamWriter<InnerResponse> responseStream, ServerCallContext context)
        {
            int count = 0;
            while (!context.CancellationToken.IsCancellationRequested && count < request.Number)
            {
                if (ResponseQueue.queue.TryDequeue(out var temp))
                {
                    count++;
                    await responseStream.WriteAsync(temp);
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }
    }
}
