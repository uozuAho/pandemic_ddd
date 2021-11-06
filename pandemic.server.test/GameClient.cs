using System;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace pandemic.server.test
{
    public class GameClient : IDisposable
    {
        private readonly RequestSocket _client;

        public GameClient()
        {
            _client = new RequestSocket();
            _client.Connect("tcp://localhost:5555");
            var request = new Request(RequestType.DoAction);
            Send(JsonConvert.SerializeObject(request));
        }

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }

        public NetworkState NewInitialState()
        {
            return new NetworkState(this);
        }

        public string Send(object message)
        {
            _client.SendFrame(JsonConvert.SerializeObject(message));
            return _client.ReceiveFrameString();
        }
    }
}
