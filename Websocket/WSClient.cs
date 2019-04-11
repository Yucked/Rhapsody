using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Frostbyte.Websocket
{
    public sealed class WSClient
    {
        public event Func<Task> OnClosed;
        public readonly ConcurrentDictionary<ulong, object> _guildConnections;

        private readonly int _shards;
        private readonly ulong _userId;
        private readonly HttpListenerWebSocketContext _wsContext;

        public WSClient(HttpListenerWebSocketContext socketContext, ulong userId, int shards)
        {
            _wsContext = socketContext;
            _userId = userId;
            _shards = shards;
        }

        public async Task ReceiveAsync()
        {
        }
    }
}