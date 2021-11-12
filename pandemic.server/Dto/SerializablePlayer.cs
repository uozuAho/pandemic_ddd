using System.Collections.Immutable;
using System.Linq;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.server.Dto
{
    public record SerializablePlayer
    {
        public Role Role { get; init; }
        public string Location { get; init; } = "Error! Set me.";
        public int ActionsRemaining { get; init; }
        public ImmutableList<SerializablePlayerCard> Hand { get; init; } = ImmutableList<SerializablePlayerCard>.Empty;

        public static SerializablePlayer From(Player player)
        {
            return new SerializablePlayer
            {
                Role = player.Role,
                Location = player.Location,
                ActionsRemaining = player.ActionsRemaining,
                Hand = player.Hand.Select(SerializablePlayerCard.From).ToImmutableList()
            };
        }

        public Player ToPlayer(StandardGameBoard board)
        {
            return new Player
            {
                Role = Role,
                Location = Location,
                ActionsRemaining = ActionsRemaining,
                Hand = new PlayerHand(Hand.Select(c => c.ToPlayerCard(board)))
            };
        }
    }
}
