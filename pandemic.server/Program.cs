using System;

namespace pandemic.server
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = args[0];
            var server = new ZmqGameServer(url);
            Console.WriteLine($"Listening at {url}");
            server.Run();
        }
    }
}
