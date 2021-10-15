#if !DNETLABS
using System;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    // Created to avoid overusing #ifs
    internal abstract class SocketInteraction
    {
        public virtual Task ModifyOriginalResponseAsync(Action<MessageProperties> _1, RequestOptions? _2 = null)
            => throw new NotSupportedException();

        public virtual Task DeferAsync() => throw new NotSupportedException();

        public virtual bool IsValidToken => false;
    }
}
#endif