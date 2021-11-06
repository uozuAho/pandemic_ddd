using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;

namespace pandemic.server.test
{
    public class ServerTests
    {
        private readonly Random _random = new();

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

            using var gameClient = new GameClient();

            var state = gameClient.NewInitialState();
            while (!state.IsTerminal)
            {
                var action = RandomChoice(state.LegalActions());
                state.ApplyAction(action);
            }
        }

        [Test]
        public void asdf()
        {
            var request = new Request("asdf");
            var serRequest = JsonConvert.SerializeObject(request);
            var desRequest = JsonConvert.DeserializeObject<Request>(serRequest);
            Assert.AreEqual(request, desRequest);
        }

        private int RandomChoice(IEnumerable<int> ints)
        {
            var list = ints.ToList();
            return list[_random.Next(list.Count)];
        }
    }
}
