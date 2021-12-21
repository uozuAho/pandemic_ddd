using System;
using System.Collections.Generic;
using System.Diagnostics;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates;
using pandemic.Values;

namespace pandemic.console
{
    internal class BfsRunner
    {
        public static void Run()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist }
            });
            var searchProblem = new PandemicSearchProblem(game);
            var evaluator = new GameEvaluator();
            var searcher = new GreedyBestFirstSearch<PandemicGame, PlayerCommand>(
                searchProblem, state => -evaluator.Evaluate(state));

            Console.WriteLine("Searching...");
            var steps = 0;
            var sw = Stopwatch.StartNew();

            while (!searcher.IsFinished)
            {
                searcher.Step();
                steps++;

                if (sw.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine($"Steps: {steps}");
                    sw.Restart();
                }
            }
        }
    }

    internal class PandemicSearchProblem : ISearchProblem<PandemicGame, PlayerCommand>
    {
        public PandemicGame InitialState { get; }

        private readonly PlayerCommandGenerator _commandGenerator;

        public PandemicSearchProblem(PandemicGame initialState)
        {
            InitialState = initialState;
            _commandGenerator = new PlayerCommandGenerator();
        }

        public IEnumerable<PlayerCommand> GetActions(PandemicGame state)
        {
            return _commandGenerator.LegalCommands(state);
        }

        public PandemicGame DoAction(PandemicGame state, PlayerCommand action)
        {
            PandemicGame newState;

            switch (action)
            {
                case DriveFerryCommand command:
                    (newState, _) = state.DriveOrFerryPlayer(command.Role, command.City);
                    return newState;
                case DiscardPlayerCardCommand command:
                    (newState, _) = state.DiscardPlayerCard(command.Card);
                    return newState;
                case BuildResearchStationCommand command:
                    (newState, _) = state.BuildResearchStation(command.City);
                    return newState;
                case DiscoverCureCommand command:
                    (newState, _) = state.DiscoverCure(command.Cards);
                    return newState;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported action: {action}");
            }
        }

        public bool IsGoal(PandemicGame state)
        {
            return state.IsWon;
        }

        public double PathCost(PandemicGame state, PlayerCommand action)
        {
            return 0.0;
        }
    }
}
