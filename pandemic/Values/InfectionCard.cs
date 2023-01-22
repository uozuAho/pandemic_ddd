using pandemic.GameData;

namespace pandemic.Values
{
    public record InfectionCard(string City, Colour Colour)
    {
        public static InfectionCard FromCity(CityData city)
        {
            return new InfectionCard(city.Name, city.Colour);
        }
    }
}
