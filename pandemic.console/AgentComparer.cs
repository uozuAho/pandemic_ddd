using System;
using System.Collections.Generic;
using System.Diagnostics;
using pandemic.Aggregates;
using pandemic.Commands;
using pandemic.Values;

namespace pandemic.console
{
    internal static class AgentComparer
    {
        public static void Run()
        {
            var agents = new[] { new PandemicAgent() };

            foreach (var agent in agents)
            {
                var stats = RunAgent(agent);
                Console.WriteLine(stats);
            }
        }

        private static AgentStats RunAgent(PandemicAgent agent)
        {
            var sw = Stopwatch.StartNew();
            var stats = new AgentStats();

            while (sw.Elapsed < TimeSpan.FromSeconds(60) && stats.GamesPlayed < 20)
            {
                var game = NewGame();
                var result = FindWin(game, agent, TimeSpan.FromSeconds(2));
                stats.GamesPlayed++;
                Console.Write('.');
            }

            return stats;
        }

        private static AgentRunResult FindWin(
            PandemicGame game,
            PandemicAgent agent,
            TimeSpan timeLimit)
        {
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed < timeLimit)
            {
                var command = agent.GetCommand(game);
                (game, _) = game.Do(command);
                if (game.IsWon)
                    return AgentRunResult.FoundWin;
            }

            return AgentRunResult.Timeout;
        }

        private static PandemicGame NewGame()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Introductory,
                // todo: try more players
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist }
            });

            return game;
        }
    }

    internal class PandemicAgent
    {
        public PlayerCommand GetCommand(PandemicGame game)
        {
            throw new NotImplementedException();
        }
    }

    internal record AgentStats
    {
        public int GamesPlayed { get; set; }
        public int NumWins { get; set; }
        public int NumTimeouts { get; set; }

        private List<TimeSpan> GameTimes = new();
        private List<TimeSpan> WinTimes = new();
    }

    enum AgentRunResult
    {
        Timeout,
        FoundWin
    }
}
