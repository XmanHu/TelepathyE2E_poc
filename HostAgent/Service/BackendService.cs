using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace HostAgent.Service
{
    using Telepathy;
    class BackendService : Backend.BackendBase
    {
        private GrpcChannel channel;

        public BackendService(string uri)
        {
            var path = new Uri(uri);
            channel = GrpcChannel.ForAddress(
                "https://localhost:5001"); //new Channel($"{path.Host}:{path.Port}", ChannelCredentials.Insecure);
        }

        public override async Task<InnerResponse> dispatch(InnerRequest innerRequest, ServerCallContext context)
        {
            //Console.WriteLine(innerRequest.ServiceName + " " + innerRequest.MethodName);
            try
            {
                var callInvoker = channel.CreateCallInvoker();

                var request = new RawMessage(innerRequest.Msg);
                var rawMethod = new RawMethod(innerRequest.ServiceName, innerRequest.MethodName);
                var result = await callInvoker.AsyncUnaryCall(rawMethod.method, null, new CallOptions(), request);

                return new InnerResponse { Msg = ByteString.CopyFrom(result.msg) };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
