using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

// The value should be build from something like an OrderGroup
public partial record struct AbilityPriority(int Value) : ISparseComponent;