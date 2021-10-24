using System;
using NetMQ;
using NetMQ.Sockets;

namespace pandemic.server
{
    public class ZmqGameServer
    {
        public void Run()
        {
            using var server = new ResponseSocket();
            server.Bind("tcp://*:5555");

            var done = false;
            while (!done)
            {
                var req = server.ReceiveFrameString();
                Console.WriteLine($"From client: {req}");
                string response;
                switch (req)
                {
                    case "a":
                        response = "b";
                        break;
                    // case "s":
                    //     response = JsonConvert
                    default:
                        throw new InvalidOperationException($"unhandled request {req}");
                };
                server.SendFrame(response);
            }
        }
    }
}
