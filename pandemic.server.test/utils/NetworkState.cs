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
            yield return 1;
        }

        public void ApplyAction(int action)
        {
            var request = new ApplyActionRequest(action, _state.state_str);
            var response = _client.Send(request);
            _state = JsonConvert.DeserializeObject<StateResponse>(response);
        }
    }
}
