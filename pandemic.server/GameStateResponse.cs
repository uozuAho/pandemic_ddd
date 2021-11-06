namespace pandemic.server
{
    public enum RequestType
    {
        GetLegalActions,
        DoAction
    }

    public record Request
    {
        public RequestType RequestType { get; init; }

        public Request(RequestType type)
        {
            RequestType = type;
        }
    }

    public record LegalActionsRequest : Request
    {
        public LegalActionsRequest() : base(RequestType.GetLegalActions) { }

        public string SerialisedState { get; init; }
    }

    public record DoActionRequest : Request
    {
        public DoActionRequest() : base(RequestType.DoAction) { }

        public int Action { get; init; }
        public string SerialisedState { get; init; }
    }

    public record GameStateResponse
    {
        public string SerialisedState { get; init; }
        public bool IsTerminal { get; init; }
    }

    public record LegalActionsResponse
    {
        public int[] LegalActions { get; init; }
    }
}
