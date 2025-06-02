namespace pandemic.test.fuzz;

using agents;
using Aggregates.Game;
using Commands;
using Shouldly;
using Utils;
using utils;
using Values;
using Xunit.Abstractions;

/// <summary>
/// Using XUnit for fuzz tests, since it seems to more reliably
/// capture console output.
/// </summary>
public class FuzzTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void Fuzz_test_games()
    {
        const int numGames = 30;

        for (var nGame = 0; nGame < numGames; nGame++)
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
                {
                    legalCommands.ShouldAllBe(c =>
                        c is DiscardPlayerCardCommand || c.IsSpecialEvent
                    );
                }

                // try a bunch of illegal commands
                foreach (
                    var illegalCommand in allPossibleCommands
                        .Except(legalCommands)
                        .OrderBy(_ => random.Next())
                        .Take(illegalCommandsToTryPerTurn)
                )
                {
                    try
                    {
                        _ = game.Do(illegalCommand);
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
    }

    private void Log()
    {
        Log("");
    }

    private void Log(object obj)
    {
        _testOutputHelper.WriteLine(obj.ToString());
    }
}
