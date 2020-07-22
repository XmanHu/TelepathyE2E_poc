
using Google.Protobuf;

namespace HostAgent
{
    using Grpc.Core;

    public class RawMethod
    {
        public readonly Method<RawMessage, RawMessage> method;

        public RawMethod(string serviceName, string methodName)
        {
            method = new Method<RawMessage, RawMessage>(
                MethodType.Unary,
                serviceName,
                methodName,
                Marshallers.Create(RawMessage.Serialize, RawMessage.Deserialize),
                Marshallers.Create(RawMessage.Serialize, RawMessage.Deserialize));
        }

    }

    public class RawMessage
    {
        public byte[] msg;

        public RawMessage(ByteString input)
        {
            msg = input.ToByteArray();
        }

        public RawMessage(byte[] input)
        {
            msg = input;
        }

        public static byte[] Serialize(RawMessage req)
        {
            return req.msg;
        }

        public static RawMessage Deserialize(byte[] bytes)
        {
            return new RawMessage(bytes);
        }
    }

}
