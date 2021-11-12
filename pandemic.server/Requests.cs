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
}
