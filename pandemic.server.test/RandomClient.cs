using System;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace pandemic.server.test
{
    internal class RandomClient : IDisposable
    {
        private readonly RequestSocket _client;

        public RandomClient()
        {
            _client = new RequestSocket();
            _client.Connect("tcp://localhost:5555");
            var request = new Request(RequestType.DoAction);
            Send(JsonConvert.SerializeObject(request));
        }

        public string Send(string msg)
        {
            _client.SendFrame(msg);
            return _client.ReceiveFrameString();
        }

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }
    }
}
