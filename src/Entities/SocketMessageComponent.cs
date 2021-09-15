#if !DNETLABS
using System;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    // Created to avoid overusing #ifs
    internal class SocketMessageComponent : SocketInteraction
    {
        public Task UpdateAsync(Action<MessageProperties> _1, RequestOptions? _2 = null)
            => throw new NotSupportedException();
    }
}
#endif