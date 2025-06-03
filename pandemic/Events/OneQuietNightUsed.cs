namespace pandemic.Events;

using Values;

internal sealed record OneQuietNightUsed(Role Role) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: used One Quiet Night";
    }
}
