﻿using pandemic.Values;

namespace pandemic.Commands;

public record ShareKnowledgeGiveCommand(Role Role, string City, Role ReceivingRole) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role} give {City} to {ReceivingRole}";
    }
}
