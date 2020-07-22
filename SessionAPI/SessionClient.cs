



using System;
using System.Net.Http;

namespace SessionAPI
{
    using System.Threading.Tasks;
    using Grpc.Net.Client;
    using Telepathy;
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;

    public class SessionClient
    {
        private Frontend.FrontendClient client;

        public SessionClient(string uri, int connection = 1)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.UseProxy = false;
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channels = GrpcChannel.ForAddress(uri, new GrpcChannelOptions() { HttpHandler = httpClientHandler });
            client = new Frontend.FrontendClient(channels);


        }

        public async Task SendTask(string serviceName, string methodName, IMessage request)
        {
            var inner = new InnerRequest { ServiceName = serviceName, MethodName = methodName, Msg = request.ToByteString() };
            await client.SendTaskAsync(inner);
        }

        public async Task<T> GetResult<T>() where T : Google.Protobuf.IMessage<T>, new()
        {
            var r = await client.GetResultAsync(new Empty());
            var v = new MessageParser<T>(() => new T());
            var result = v.ParseFrom(r.Msg);
            return result;


        }
    }
}
