using pandemic.perftest;

// Console.WriteLine("Running random games...");
// var stats = GameRunner.RunRandomGames(TimeSpan.FromSeconds(2));
// Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");
//
Console.WriteLine("Running greedy games...");
var stats = GameRunner.RunGreedyGames(TimeSpan.FromSeconds(2));
Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");

// Console.WriteLine("Running greedy BFS...");
// var stats = GameRunner.RunGreedyBfsGames(TimeSpan.FromSeconds(2));
// Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");
