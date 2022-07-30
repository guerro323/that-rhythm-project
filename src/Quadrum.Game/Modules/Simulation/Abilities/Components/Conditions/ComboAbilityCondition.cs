using revecs.Core;
using revecs.Extensions.Buffers;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components.Conditions;

/// <summary>
/// A combo-based validation for abilities
/// </summary>
public partial struct ComboAbilityCondition : IBufferComponent
{
    // Before it was an entity, but for server-client synchronization we use a component type
    public ComponentType ValidCommandType;
}