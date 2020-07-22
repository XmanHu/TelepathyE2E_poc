using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telepathy;

namespace Frontend
{
    public class ResponseQueue
    {
        public static ConcurrentQueue<InnerResponse> queue = new ConcurrentQueue<InnerResponse>();
    }
}
