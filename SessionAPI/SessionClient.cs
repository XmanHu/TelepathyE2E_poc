

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Google.Protobuf.Reflection;
using Grpc.Core;

namespace SessionAPI
{
    using System.Threading.Tasks;
    using Grpc.Net.Client;
    using Telepathy;
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;

    public class SessionClient
    {
        private List<Frontend.FrontendClient> clients = new List<Frontend.FrontendClient>();

        private int index = 0;

        public SessionClient(string uri, int connection = 1)
        {
            for (int i = 0; i < connection; i++)
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.UseProxy = false;
                httpClientHandler.AllowAutoRedirect = false;
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var channel = GrpcChannel.ForAddress(uri, new GrpcChannelOptions() { HttpHandler = httpClientHandler });
                clients.Add(new Frontend.FrontendClient(channel));
            }
        }

        public async Task SendTask(MethodDescriptor methodDescriptor, IMessage request)
        {
            var inner = new InnerRequest
                {ServiceName = methodDescriptor.Service.FullName, MethodName = methodDescriptor.Name, Msg = request.ToByteString()};
            await GetClient().SendTaskAsync(inner);
        }

        public async Task<T> GetResult<T>() where T : IMessage<T>, new()
        {
            var r = await GetClient().GetResultAsync(new Empty());
            var v = new MessageParser<T>(() => new T());
            var result = v.ParseFrom(r.Msg);
            return result;
        }

        public StreamRequestCall CreateRequestStreamCall()
        {
            return new StreamRequestCall(GetClient());
        }

        public StreamResponseCall<T> CreateResponseStreamCall<T>(int number = 1) where T : IMessage<T>, new()
        {
            return new StreamResponseCall<T>(GetClient(), number);
        }

        private Frontend.FrontendClient GetClient()
        {
            var i = Interlocked.Increment(ref index) % clients.Count;
            return clients[i];
        }

    }

    public class StreamRequestCall
    {
        private AsyncClientStreamingCall<InnerRequest, Empty> call;

        public StreamRequestCall(Frontend.FrontendClient client)
        {
            call = client.SendTaskStream();
        }

        public async Task SendTaskStream(MethodDescriptor methodDescriptor, IMessage request)
        {
            var inner = new InnerRequest
                { ServiceName = methodDescriptor.Service.FullName, MethodName = methodDescriptor.Name, Msg = request.ToByteString() };
            await call.RequestStream.WriteAsync(inner);
        }

        public async Task EndOfRequest()
        {
            await call.RequestStream.CompleteAsync();
            await call.ResponseAsync;
        }
    }

    public class StreamResponseCall<T> where T : IMessage<T>, new()
    {
        private AsyncServerStreamingCall<InnerResponse> call;

        public StreamResponseCall(Frontend.FrontendClient client, int number)
        {
            call = client.GetResultStream(new AskNumber { Number = number});
        }

        public async Task<IEnumerable<T>> GetResultStream()
        {
            var result = new List<T>();

            await foreach (var res in call.ResponseStream.ReadAllAsync())
            {
                var v = new MessageParser<T>(() => new T());
                result.Add(v.ParseFrom(res.Msg));
            }

            return result;
        }
    }
}
