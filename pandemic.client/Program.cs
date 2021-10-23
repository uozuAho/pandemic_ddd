using System;
using NetMQ;
using NetMQ.Sockets;

namespace pandemic.client
{
    class Program
    {
        static void Main(string[] args)
        {
            using var client = new RequestSocket();
            client.Connect("tcp://localhost:5555");
            client.SendFrame("Hello");
            var msg = client.ReceiveFrameString();
            Console.WriteLine($"From Server: {msg}");
        }
    }
}
