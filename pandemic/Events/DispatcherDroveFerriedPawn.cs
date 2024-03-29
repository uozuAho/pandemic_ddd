﻿using pandemic.Values;

namespace pandemic.Events;

internal record DispatcherDroveFerriedPawn(Role Role, string City) : IEvent
{
    public override string ToString()
    {
        return $"Dispatcher: drove {Role} to {City}";
    }
}
