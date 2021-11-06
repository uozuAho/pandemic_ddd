using System;
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
        PlayerCommandGenerator gen = new PlayerCommandGenerator();

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
                Console.WriteLine($"From client: {req}");
                var reqD = JsonConvert.DeserializeObject<Request>(req);
                // var response = reqD.RequestType switch
                // {
                //     RequestType.GetLegalActions => HandleGetLegalActions(JsonConvert.DeserializeObject<LegalActionsRequest>(req)),
                //     RequestType.DoAction => HandleDoAction(JsonConvert.DeserializeObject<DoActionRequest>(req))
                // };

                // string response;
                // switch (req)
                // {
                //     case "a":
                //         var gen = new PlayerCommandGenerator();
                //         var numActions = gen.LegalCommands(game).Count();
                //         response = string.Join(',', Enumerable.Range(0, numActions));
                //         break;
                //     case "s":
                //         response = JsonConvert.SerializeObject(game);
                //         break;
                //     case "q":
                //         response = "bye!";
                //         done = true;
                //         break;
                //     default:
                //         var isAction = int.TryParse(req, out var action);
                //         if (!isAction)
                //             throw new InvalidOperationException($"unhandled request {req}");
                //         game = DoAction(game, action);
                //         response = JsonConvert.SerializeObject(game);
                //         break;
                // };
                // server.SendFrame(response);
            }
        }

        private GameStateResponse HandleDoAction(DoActionRequest req)
        {
            var game = JsonConvert.DeserializeObject<PandemicGame>(req.SerialisedState);

            var nextState = DoAction(game, req.Action);

            return new GameStateResponse
            {
                IsTerminal = nextState.IsOver,
                SerialisedState = JsonConvert.SerializeObject(nextState)
            };
        }

        private LegalActionsResponse HandleGetLegalActions(LegalActionsRequest req)
        {
            var game = JsonConvert.DeserializeObject<PandemicGame>(req.SerialisedState);
            var numActions = gen.LegalCommands(game).Count();

            return new LegalActionsResponse
            {
                LegalActions = Enumerable.Range(0, numActions).ToArray()
            };
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
