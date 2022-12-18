using System.Collections.Generic;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.server.Dto;
using pandemic.Values;

namespace pandemic.server.test
{
    class SerializablePandemicGameTests
    {
        [Test]
        public void Deserialised_equals_initial_object()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new List<Role> {Role.Medic, Role.QuarantineSpecialist}
            });

            var serialisable = SerializablePandemicGame.From(game);
            var ser = serialisable.Serialise();
            var deser = SerializablePandemicGame.Deserialise(ser);
            var deserGame = deser.ToPandemicGame(StandardGameBoard.Instance());

            Assert.IsTrue(deserGame.IsSameStateAs(game));
        }
    }
}
