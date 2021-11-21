using System;
using System.Collections.Generic;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic
{
    /// <summary>
    /// Provides an OpenSpiel compatible interface for playing Pandemic.
    /// Work in progress. See todos.
    /// </summary>
    public class PandemicSpielGameState
    {
        public PandemicGame Game;

        public bool IsTerminal => Game.IsOver;
        public bool IsWin => Game.IsWon;
        public bool IsLoss => Game.IsLost;
        public int CurrentPlayerIdx => Game.CurrentPlayerIdx;
        /// this doesn't conform to spiel state. Use CurrentPlayerIdx for the player idx.
        public Player CurrentPlayer => Game.CurrentPlayer;

        private readonly PlayerCommandGenerator _commandGenerator = new ();

        public PandemicSpielGameState(PandemicGame game)
        {
            Game = game;
        }

        public PandemicSpielGameState Clone()
        {
            return new PandemicSpielGameState(Game with { });
        }

        public override string ToString()
        {
            return Game.ToString();
        }

        public IEnumerable<int> LegalActionsInt()
        {
            // todo: how to map from meaningful actions to ints? Looks like OpenSpiel
            // wants a way to do this too, see https://github.com/deepmind/open_spiel/blob/master/docs/contributing.md
            // point 'Structured Action Spaces'
            throw new NotImplementedException();
        }

        public IEnumerable<PlayerCommand> LegalActions()
        {
            return _commandGenerator.LegalCommands(Game);
        }

        public string ActionToString(int currentPlayer, int action)
        {
            return "todo: implement me";
        }

        public void ApplyActionInt(int action)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEvent> ApplyAction(PlayerCommand action)
        {
            IEnumerable<IEvent> events;

            switch (action)
            {
                case DriveFerryCommand command:
                    (Game, events) = Game.DriveOrFerryPlayer(command.Role, command.City);
                    return events;
                case DiscardPlayerCardCommand command:
                    (Game, events) = Game.DiscardPlayerCard(command.Card);
                    return events;
                case BuildResearchStationCommand command:
                    (Game, events) = Game.BuildResearchStation(command.City);
                    return events;
                case DiscoverCureCommand command:
                    (Game, events) = Game.DiscoverCure(command.Cards);
                    return events;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported action: {action}");
            }
        }
    }
}
