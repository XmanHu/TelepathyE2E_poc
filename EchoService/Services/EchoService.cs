
namespace EchoService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Microsoft.Extensions.Logging;

    public class EchoService : Echoer.EchoerBase
    {
        private readonly ILogger<EchoService> _logger;
        public EchoService(ILogger<EchoService> logger)
        {
            _logger = logger;
        }

        public override Task<EchoReply> Echo(EchoRequest request, ServerCallContext context)
        {
            return Task.FromResult(new EchoReply { Message = "Hi " + request.Name });
        }
    }
}
