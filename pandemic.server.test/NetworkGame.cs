using System;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace pandemic.server.test
{
    public class NetworkGame : IDisposable
    {
        private readonly RequestSocket _client;

        public NetworkGame(string url)
        {
            _client = new RequestSocket();
            _client.Connect(url);
        }

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }

        public NetworkState NewInitialState()
        {
            Send(new Request("apply_action"));
            return new NetworkState(this);
        }

        public string Send(object message)
        {
            _client.SendFrame(JsonConvert.SerializeObject(message));
            return _client.ReceiveFrameString();
        }
    }
}
