namespace pandemic.server
{
    public record Request(string type);

    public record ApplyActionRequest : Request
    {
        public int action { get; init; }
        public string state_str { get; init; }

        public ApplyActionRequest(int action, string stateStr) : base("apply_action")
        {
            this.action = action;
            this.state_str = stateStr;
        }
    }

    public record StateResponse(
        string current_player,
        int[] legal_actions,
        bool is_terminal,
        bool is_chance_node,
        double[] returns,
        string state_str,
        string pretty_str
    );
}
