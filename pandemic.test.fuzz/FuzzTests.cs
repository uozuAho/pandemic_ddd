using System.Reflection;
using pandemic.agents;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.test.Utils;
using pandemic.Values;
using Shouldly;
using utils;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace pandemic.test.fuzz;

/// <summary>
/// Using XUnit for fuzz tests, since it seems to more reliably
/// capture console output.
/// </summary>
public class FuzzTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FuzzTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [Repeat(30)]
    public void Fuzz_test_games()
    {
        var options = NewGameOptionsGenerator.RandomOptions();
        var random = new Random();

        var agent = random.Choice(new ILiveAgent[] { new GreedyAgent(), new RandomAgent() });

        _testOutputHelper.WriteLine(options.ToString());

        const int illegalCommandsToTryPerTurn = 10;
        var (game, events) = PandemicGame.CreateNewGame(options);
        var allPossibleCommands = AllPlayerCommandGenerator.AllPossibleCommands(game).ToList();

        for (var i = 0; i < 1000 && !game.IsOver; i++)
        {
            var legalCommands = game.LegalCommands().ToList();

            if (game.Players.Any(p => p.Hand.Count > 7))
                legalCommands.ShouldAllBe(c => c is DiscardPlayerCardCommand || c.IsSpecialEvent);

            // try a bunch of illegal commands
            foreach (var illegalCommand in allPossibleCommands
                         .Except(legalCommands)
                         .OrderBy(_ => random.Next())
                         .Take(illegalCommandsToTryPerTurn))
            {
                try
                {
                    game.Do(illegalCommand);
                    Log(game);
                    Log();
                    Log("Events, in reverse:");
                    Log(string.Join('\n', events.Reversed()));
                    Assert.Fail($"Expected {illegalCommand} to throw");
                }
                catch (GameRuleViolatedException)
                {
                    // do nothing: we want an exception thrown!
                }
                catch (Exception)
                {
                    Log($"Chosen illegal command: {illegalCommand}");
                    Log(game);
                    Log();
                    Log("Events, in reverse:");
                    Log(string.Join('\n', events.Reversed()));
                    throw;
                }
            }

            legalCommands.Count.ShouldBePositive(game.ToString());
            var command = agent.NextCommand(game);
            try
            {
                (game, var tempEvents) = game.Do(command);
                events.AddRange(tempEvents);
            }
            catch (Exception)
            {
                Log($"Chosen command: {command}");
                Log(game);
                Log();
                Log("Events, in reverse:");
                Log(string.Join('\n', events.Reversed()));
                throw;
            }
        }
    }

    private void Log()
    {
        Log("");
    }

    private void Log(object obj)
    {
        _testOutputHelper.WriteLine(obj.ToString());
    }

    /// <summary>
    /// Hacking Xunit to repeat tests
    /// </summary>
    private class RepeatAttribute : DataAttribute
    {
        private readonly int _count;

        public RepeatAttribute(int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    "Repeat count must be greater than 0.");
            }
            _count = count;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return Enumerable.Repeat(Array.Empty<object>(), _count);
        }
    }
}
