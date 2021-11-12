using System.Collections.Generic;
using Newtonsoft.Json;

namespace pandemic.server.test.utils
{
    public class NetworkState
    {
        public bool IsTerminal => _state.is_terminal;

        private readonly NetworkGame _client;
        private StateResponse _state;

        public NetworkState(NetworkGame client, StateResponse state)
        {
            _client = client;
            _state = state;
        }

        public IEnumerable<int> LegalActions()
        {
            return _state.legal_actions;
        }

        public void ApplyAction(int action)
        {
            var request = new ApplyActionRequest(action, _state.state_str);
            var response = _client.Send(request);
            _state = JsonConvert.DeserializeObject<StateResponse>(response);
        }
    }
}
