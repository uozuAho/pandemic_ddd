using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NUnit.Framework;

namespace pandemic.server.test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            new Thread(() =>
            {
                var server = new ZmqGameServer();
                server.Run();
            }).Start();

            using var client = new RandomClient();
        }
    }

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
