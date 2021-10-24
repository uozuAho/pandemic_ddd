using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;

namespace pandemic.client
{
    internal class ZmqGameState : ISpielState
    {
        public bool IsTerminal { get; }
        private readonly ZmqGameClient _client;
        private readonly string _stateString;

        public ZmqGameState(ZmqGameClient client, string stateString)
        {
            _client = client;
            _stateString = stateString;
        }

        public void ApplyAction(int action)
        {
            _client.Send(action.ToString());
        }

        public IEnumerable<int> LegalActions()
        {
            return _client.Send("a").Split(',').Select(int.Parse);
        }
    }

    /// <summary>
    /// This is just an example: the real client will need to be implemented
    /// in Python/C++ to be used by OpenSpiel bots.
    /// </summary>
    internal class ZmqGameClient : IDisposable
    {
        public ISpielGame Game { get; }
        public ISpielState State { get; }

        private readonly RequestSocket _client;

        public ZmqGameClient()
        {
            _client = new RequestSocket();
            _client.Connect("tcp://localhost:5555");
            var stateString = Send("s");
            State = new ZmqGameState(this, stateString);
        }

        public string Send(string msg)
        {
            _client.SendFrame(msg);
            return _client.ReceiveFrameString();
        }

        private void ReleaseUnmanagedResources()
        {
            _client.Close();
            _client.Dispose();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~ZmqGameClient()
        {
            ReleaseUnmanagedResources();
        }

        public int GetGameType()
        {
            return 0;
        }

        public int MaxUtility()
        {
            return 0;
        }
    }
}
