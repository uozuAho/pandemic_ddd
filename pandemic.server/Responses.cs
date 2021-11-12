namespace pandemic.server
{
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
