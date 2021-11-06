using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.server
{
    public class ZmqGameServer
    {
        readonly PlayerCommandGenerator _commandGenerator = new();

        public void Run()
        {
            using var server = new ResponseSocket();
            server.Bind("tcp://*:5555");
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] {Role.Medic, Role.Scientist}
            });

            var done = false;
            while (!done)
            {
                var req = server.ReceiveFrameString();
                var reqD = JsonConvert.DeserializeObject<Request>(req);
                switch (reqD.type)
                {
                    case "exit":
                        done = true;
                        server.SendFrame("aasdf");
                        break;
                    case "apply_action":
                        var state = new StateResponse(
                            game.CurrentPlayerIdx.ToString(),
                            ToIntArray(_commandGenerator.LegalCommands(game)),
                            game.IsOver,
                            false,
                            new double[] { 1 },
                            "asdf"
                        );
                        server.SendFrame(JsonConvert.SerializeObject(state));
                        break;
                    case "new_initial_state":
                        var state2 = new StateResponse(
                            game.CurrentPlayerIdx.ToString(),
                            ToIntArray(_commandGenerator.LegalCommands(game)),
                            game.IsOver,
                            false,
                            new double[] { 1 },
                            "asdf"
                        );
                        server.SendFrame(JsonConvert.SerializeObject(state2));
                        break;
                    default:
                        throw new InvalidOperationException($"Unhandled request type '{reqD.type}");
                }
            }
        }

        private static int[] ToIntArray(IEnumerable<PlayerCommand> commands)
        {
            return Enumerable.Range(0, commands.Count()).ToArray();
        }


        private PandemicGame DoAction(PandemicGame game, int actionIdx)
        {
            var gen = new PlayerCommandGenerator();
            var action = gen.LegalCommands(game).ToList()[actionIdx];
            return ApplyAction(game, action);
        }

        public PandemicGame ApplyAction(PandemicGame game, PlayerCommand action)
        {
            switch (action)
            {
                case DriveFerryCommand command:
                    (game, _) = game.DriveOrFerryPlayer(command.Role, command.City);
                    return game;
                case DiscardPlayerCardCommand command:
                    (game, _) = game.DiscardPlayerCard(command.Card);
                    return game;
                case BuildResearchStationCommand command:
                    (game, _) = game.BuildResearchStation(command.City);
                    return game;
                case DiscoverCureCommand command:
                    (game, _) = game.DiscoverCure(command.Cards);
                    return game;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported action: {action}");
            }
        }
    }
}
