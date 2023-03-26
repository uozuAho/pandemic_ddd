using pandemic.perftest;

// Console.WriteLine("Running random games...");
// var stats = GameRunner.RunRandomGames(new RunConfig
// {
//     TotalRunTime = TimeSpan.FromSeconds(2),
//     Rng = new Random(1234)
// });
// Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");

// Console.WriteLine("Running greedy games...");
// var stats = GameRunner.RunGreedyGames(new RunConfig
// {
//     TotalRunTime = TimeSpan.FromSeconds(2),
//     Rng = new Random(1234)
// });
// Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");

// Console.WriteLine("Running greedy BFS...");
// var stats = GameRunner.RunGreedyBfsGames(new RunConfig
// {
//     TotalRunTime = TimeSpan.FromSeconds(2),
//     Rng = new Random(1234)
// });
// Console.WriteLine($"Games played: {stats.GamesPlayed}, commands executed: {stats.CommandsExecuted}");

GreedyBenchmark.Run();
