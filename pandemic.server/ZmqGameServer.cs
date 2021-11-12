using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using pandemic.Aggregates;
using pandemic.GameData;
using pandemic.server.Dto;
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

            var keepServing = true;
            while (keepServing)
            {
                var req = server.ReceiveFrameString();
                keepServing = Handle(req, server);
            }
        }

        private bool Handle(string? req, ResponseSocket? server)
        {
            var reqD = JsonConvert.DeserializeObject<Request>(req);
            if (reqD == null) throw new InvalidOperationException("doh");
            switch (reqD.type)
            {
                case "exit":
                    server.SendFrame("shutting down server...");
                    return false;
                case "apply_action":
                    var response = HandleApplyAction(req);
                    server.SendFrame(JsonConvert.SerializeObject(response));
                    break;
                case "new_initial_state":
                    var response2 = HandleNewInitialState();
                    server.SendFrame(JsonConvert.SerializeObject(response2));
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled request type '{reqD.type}");
            }

            return true;
        }

        private StateResponse HandleNewInitialState()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            return new StateResponse(
                game.CurrentPlayerIdx.ToString(),
                ToIntArray(_commandGenerator.LegalCommands(game)),
                game.IsOver,
                false,
                new double[] { 1 },
                JsonConvert.SerializeObject(SerializablePandemicGame.From(game)),
                "asdf"
            );
        }

        private StateResponse HandleApplyAction(string req)
        {
            var applyActionRequest = JsonConvert.DeserializeObject<ApplyActionRequest>(req);
            if (applyActionRequest == null) throw new InvalidOperationException("doh");

            var state = SerializablePandemicGame
                .Deserialise(applyActionRequest.state_str)
                .ToPandemicGame(new StandardGameBoard());

            var newState = DoAction(state, applyActionRequest.action);

            return new StateResponse(
                newState.CurrentPlayerIdx.ToString(),
                ToIntArray(_commandGenerator.LegalCommands(newState)),
                newState.IsOver,
                false,
                new double[] { 1 },
                JsonConvert.SerializeObject(SerializablePandemicGame.From(newState)),
                "todo: pretty string of game state"
            );
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
