using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

public partial record struct AbilityType(ComponentType Target) : ISparseComponent;