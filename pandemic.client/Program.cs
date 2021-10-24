using System;

namespace pandemic.client
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new RandomBot();
            using var game = new ZmqGameClient();

            while (!game.State.IsTerminal)
            {
                var action = bot.Step(game.State);
                game.State.ApplyAction(action);
            }
        }

        static void ZmqDemo()
        {
            using var client = new ZmqGameClient();
            var response = client.Send("Hello");
            Console.WriteLine($"From Server: {response}");
        }
    }
}
