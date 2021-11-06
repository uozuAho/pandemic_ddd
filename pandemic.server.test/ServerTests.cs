using System.Threading;
using NUnit.Framework;

namespace pandemic.server.test
{
    public class ServerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            new Thread(() =>
            {
                var server = new ZmqGameServer();
                server.Run();
            }).Start();

            using var client = new RandomClient();
        }
    }
}
