namespace pandemic.Values
{
    public record Player
    {
        public Role Role { get; init; }
        public string Location { get; init; } = "Atlanta";
    }
}
