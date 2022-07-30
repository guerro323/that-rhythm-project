using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

public partial struct AbilityState : ISparseComponent
{
    public int Combo;
    public int UpdateVersion;
    public int ActivationVersion;
}

// Similar to Enum.WillBeActive
public partial struct AbilityStateBeforeActivationTag : ITagComponent
{
    
}

// Similar to Enum.Active
public partial struct AbilityStateActiveTag : ITagComponent
{
    
}

// Similar to Enum.Chaining
public partial struct AbilityStateChainingTag : ITagComponent
{
    
}