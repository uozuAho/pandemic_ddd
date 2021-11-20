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
    public class ZmqGameServer : IDisposable
    {
        private readonly string _url;
        private readonly PlayerCommandGenerator _commandGenerator = new();
        private readonly ResponseSocket _server;

        public ZmqGameServer(string url)
        {
            _url = url;
            _server = new ResponseSocket();
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public void Run()
        {
            _server.Bind(_url);
            var keepServing = true;
            while (keepServing)
            {
                var req = _server.ReceiveFrameString();
                keepServing = Handle(req, out var response);
                _server.SendFrame(response);
            }
            _server.Close();
        }

        private bool Handle(string req, out string response)
        {
            var reqD = JsonConvert.DeserializeObject<Request>(req);
            if (reqD == null) throw new InvalidOperationException("doh");

            object responseObj = reqD.type switch
            {
                "exit" => "shutting down server...",
                "apply_action" => HandleApplyAction(req),
                "new_initial_state" => HandleNewInitialState(),
                "game_type" => HandleGameType(),
                _ => throw new InvalidOperationException($"Unhandled request type '{reqD.type}")
            };

            response = JsonConvert.SerializeObject(responseObj);

            var keepServing = reqD.type != "exit";
            return keepServing;
        }

        private static object HandleGameType()
        {
            return new
            {
                short_name = "Pandemic",
                long_name = "Pandemic",
                // kSequential: https://github.com/deepmind/open_spiel/blob/dbfb14322c8c3ebc089310032a56bfaed0dc4c01/open_spiel/spiel.h#L60
                dynamics = 1,
                // kDeterministic: https://github.com/deepmind/open_spiel/blob/dbfb14322c8c3ebc089310032a56bfaed0dc4c01/open_spiel/spiel.h#L73
                chance_mode = 0,
                // kPerfectInformation: https://github.com/deepmind/open_spiel/blob/dbfb14322c8c3ebc089310032a56bfaed0dc4c01/open_spiel/spiel.h#L84
                information = 1,
                // kIdentical: https://github.com/deepmind/open_spiel/blob/dbfb14322c8c3ebc089310032a56bfaed0dc4c01/open_spiel/spiel.h#L94
                utility = 3,
                // kTerminal: https://github.com/deepmind/open_spiel/blob/dbfb14322c8c3ebc089310032a56bfaed0dc4c01/open_spiel/spiel.h#L103
                reward_model = 1,
                max_num_players = 4,
                min_num_players = 2,
                provides_information_state_string = false,
                provides_information_state_tensor = false,
                provides_observation_string = false,
                provides_observation_tensor = false,
                parameter_specification = new Dictionary<string, string>()
            };
        }

        private StateResponse HandleNewInitialState()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Medic, Role.Scientist }
            });

            return NewStateResponse(game);
        }

        private StateResponse HandleApplyAction(string req)
        {
            var applyActionRequest = JsonConvert.DeserializeObject<ApplyActionRequest>(req);
            if (applyActionRequest == null) throw new InvalidOperationException("doh");

            var state = SerializablePandemicGame
                .Deserialise(applyActionRequest.state_str)
                .ToPandemicGame(new StandardGameBoard());

            var newState = DoAction(state, applyActionRequest.action);

            return NewStateResponse(newState);
        }

        private StateResponse NewStateResponse(PandemicGame game)
        {
            return new StateResponse(
                game.CurrentPlayerIdx.ToString(),
                ToIntArray(_commandGenerator.LegalCommands(game)),
                game.IsOver,
                false,
                new double[] { 1 },
                JsonConvert.SerializeObject(SerializablePandemicGame.From(game)),
                $"win: {game.IsWon}, loss reason: {game.LossReason}"
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
