using Frostbyte.Attributes;
using Frostbyte.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Frostbyte.Websocket
{
    [Service(ServiceLifetime.Singleton)]
    public sealed class WSServer : IAsyncDisposable
    {
        public event Func<WSClient, ValueTask> OnConnected;
        public event Func<WSClient, ValueTask> OnDisconnected;
        public event Func<WSClient, string, ValueTask> OnMessageReceived;
        public event Func<WSClient, string, ValueTask> OnMessageDelivered;

        public IPEndPoint EndPoint { get; private set; }
        private readonly Socket _socket;

        public WSServer(ConfigEntity config)
        {
            EndPoint = IPAddress.TryParse(config.Host, out var address)
                ? new IPEndPoint(address, config.Port)
                : new IPEndPoint(IPAddress.Any, config.Port);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            _socket.Bind(EndPoint);
            _socket.Listen(0);
            _socket.BeginAccept(AcceptCallback, null);
            _socket.BeginReceive(new byte[512], 0, 0, SocketFlags.None, ReceiveCallback, null);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            if (!result.IsCompleted)
                return;

            _socket.EndAccept(result);
            _socket.Receive(new byte[512]);

            var client = new WSClient(_socket);
            OnConnected?.Invoke(client);
            _socket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            if (!result.IsCompleted)
                return;

            _socket.EndReceive(result);

            _socket.BeginReceive(new byte[512], 0, 0, SocketFlags.None, ReceiveCallback, null);
        }

        private string HandshakeResponse(string key)
        {
            return
                $"HTTP/1.1 101 Switching Protocols\n" +
                $"Upgrade: WebSocket\n" +
                $"Connection: Upgrade\n" +
                $"Sec-WebSocket-Accept: {key}";
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}