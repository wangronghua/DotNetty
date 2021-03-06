﻿using System.Threading.Tasks;

namespace DotNetty.Rpc.Client
{
    using DotNetty.Common.Concurrency;
    using DotNetty.Rpc.Protocol;

    internal class RequestContext
    {
        public RequestContext(TaskCompletionSource<RpcMessage> tcs, IScheduledTask timeOutTimer)
        {
            this.TaskCompletionSource = tcs;
            this.TimeOutTimer = timeOutTimer;
        }

        public TaskCompletionSource<RpcMessage> TaskCompletionSource { get; private set; }

        public IScheduledTask TimeOutTimer { get; private set; }
    }
}
