using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using pandemic.Aggregates.Game;
using pandemic.Commands;
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

            var responseObj = reqD.type switch
            {
                "exit" => "shutting down server...",
                "apply_action" => HandleApplyAction(req),
                "new_initial_state" => HandleNewInitialState(),
                "game_type" => HandleGameType(),
                "game_info" => HandleGameInfo(),
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

        private static object HandleGameInfo()
        {
            return new
            {
                // todo: how many distinct actions are there?
                num_distinct_actions = 1000,
                max_chance_outcomes = 0, // N/A for deterministic games
                num_players = -1, // todo: game doesn't know the number of players. How is this supposed to work?
                // todo: game is win/lose. What should utility be?
                min_utility = -1.0,
                max_utility = 1.0,
                utility_sum = 0.0, // todo: dunno about this
                max_game_length = 1000, // todo: what is the actual max game length? Does it matter?
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
            var returns = Enumerable.Repeat(0.0, game.Players.Count).ToArray();
            if (game.IsWon) returns = Enumerable.Repeat(1.0, game.Players.Count).ToArray();
            if (game.IsLost) returns = Enumerable.Repeat(-1.0, game.Players.Count).ToArray();

            return new StateResponse(
                game.CurrentPlayerIdx,
                ToIntArray(_commandGenerator.LegalCommands(game)),
                game.IsOver,
                false,
                returns,
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
            (game, _) = game.Do(action);
            return game;
        }
    }
}
