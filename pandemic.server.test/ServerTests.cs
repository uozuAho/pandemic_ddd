using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            using var gameClient = new NetworkGame("tcp://localhost:5555");

            var state = gameClient.NewInitialState();
            while (!state.IsTerminal)
            {
                var action = RandomChoice(state.LegalActions());
                state.ApplyAction(action);
            }
        }

        private int RandomChoice(IEnumerable<int> ints)
        {
            var list = ints.ToList();
            return list[_random.Next(list.Count)];
        }
    }
}
