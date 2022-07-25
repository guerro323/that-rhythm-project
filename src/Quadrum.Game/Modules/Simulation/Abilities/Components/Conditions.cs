using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using revecs.Core;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

/// <summary>
/// A combo-based validation for abilities
/// </summary>
public partial struct ComboAbilityCondition : IBufferComponent
{
    public UEntitySafe ValidCommand;
}

/// <summary>
/// A validation that require the owner of the ability to be alive
/// </summary>
public partial struct LivingOwnerAbilityCondition : ITagComponent
{
    
}