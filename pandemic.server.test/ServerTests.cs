using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using pandemic.server.test.utils;

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
        public void Play_one_game_with_random_moves()
        {
            new Thread(() =>
            {
                var server = new ZmqGameServer("tcp://*:5555");
                server.Run();
            }).Start();

            using var game = new NetworkGame("tcp://localhost:5555");

            var state = game.NewInitialState();
            while (!state.IsTerminal)
            {
                var action = RandomChoice(state.LegalActions());
                state.ApplyAction(action);
            }
            game.Exit();
        }

        private int RandomChoice(IEnumerable<int> ints)
        {
            var list = ints.ToList();
            return list[_random.Next(list.Count)];
        }
    }
}
