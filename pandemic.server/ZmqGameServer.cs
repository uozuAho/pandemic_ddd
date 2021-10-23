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
            var msg = server.ReceiveFrameString();
            Console.WriteLine($"From client: {msg}");
            server.SendFrame("boop");
        }
    }
}
