using System;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace pandemic.server.test.utils
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
            var stateStr = Send(new Request("new_initial_state"));
            var state = JsonConvert.DeserializeObject<StateResponse>(stateStr);
            return new NetworkState(this, state);
        }

        public void Exit()
        {
            Send(new Request("exit"));
        }

        public string Send(object message)
        {
            _client.SendFrame(JsonConvert.SerializeObject(message));
            return _client.ReceiveFrameString();
        }
    }
}
