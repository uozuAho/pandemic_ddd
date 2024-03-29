﻿using pandemic.Values;

namespace pandemic.Events;

internal record OneQuietNightUsed(Role Role) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: used One Quiet Night";
    }
}
