using System;
using System.Collections.Generic;
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
            return new NetworkState();
        }

        private string Send(string msg)
        {
            _client.SendFrame(msg);
            return _client.ReceiveFrameString();
        }
    }

    public class NetworkState
    {
        public bool IsTerminal { get; set; } = true;

        public IEnumerable<int> LegalActions()
        {
            yield return 1;
        }
    }
}
