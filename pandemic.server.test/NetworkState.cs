using System.Collections.Generic;

namespace pandemic.server.test
{
    public class NetworkState
    {
        public bool IsTerminal { get; set; } = true;
        private readonly GameClient _client;

        public NetworkState(GameClient client)
        {
            _client = client;
        }

        public IEnumerable<int> LegalActions()
        {
            yield return 1;
        }

        public void ApplyAction(int action)
        {
            _client.Send(new
            {
                type = "apply_action",
                action,
                state_str = "todo: implement me"
            });
        }
    }
}
