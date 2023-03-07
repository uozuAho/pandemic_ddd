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

namespace pandemic.testx;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// Fuzzing with Xunit is slower, but at least it captures console output
    /// </summary>
    [Fact]
    public void run_FuzzXunit_30_times()
    {
        for (var i = 0; i < 30; i++)
        {
            FuzzXunit();
        }
    }

    public void FuzzXunit()
    {
        var options = NewGameOptionsGenerator.RandomOptions();

        _testOutputHelper.WriteLine(options.ToString());

        // bigger numbers here slow down the test, but check for more improper behaviour
        const int illegalCommandsToTryPerTurn = 10;
        var random = new Random();
        var (game, events) = PandemicGame.CreateNewGame(options);
        var allPossibleCommands = AllPlayerCommandGenerator.AllPossibleCommands(game).ToList();
        var agent = new GreedyAgent();

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
            // var command = random.Choice(legalCommands);
            var command = agent.BestCommand(game, legalCommands);
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
    public class RepeatAttribute : DataAttribute
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
            return Enumerable.Repeat(new object[0], _count);
        }
    }
}
