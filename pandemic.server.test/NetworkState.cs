using System.Collections.Generic;

namespace pandemic.server.test
{
    public class NetworkState
    {
        public bool IsTerminal => _stateResponse.is_terminal;

        private readonly NetworkGame _client;
        private readonly StateResponse _stateResponse;

        public NetworkState(NetworkGame client, StateResponse stateResponse)
        {
            _client = client;
            _stateResponse = stateResponse;
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
