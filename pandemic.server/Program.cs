namespace pandemic.server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ZmqGameServer("tcp://*:5555");
            server.Run();
        }
    }
}
