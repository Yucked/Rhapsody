using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Frostbyte.Websocket
{
    public sealed class WSClient : IAsyncDisposable
    {
        private readonly Socket _socket;

        public string Id { get; set; }

        public WSClient(Socket socket)
        {
            _socket = socket;
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}