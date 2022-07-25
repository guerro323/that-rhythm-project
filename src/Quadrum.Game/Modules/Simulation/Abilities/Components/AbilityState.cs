using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

public partial struct AbilityState : ISparseComponent
{
    public int Combo;
    public int UpdateVersion;
    public int ActivationVersion;
}

public partial struct AbilityStateBeforeActivationTag : ITagComponent
{
    
}

public partial struct AbilityStateActiveTag : ITagComponent
{
    
}

public partial struct AbilityStateChainingTag : ITagComponent
{
    
}