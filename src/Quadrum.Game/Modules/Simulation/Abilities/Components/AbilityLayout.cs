using revecs.Core;
using revecs.Extensions.EntityLayout;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

public partial struct AbilityLayout : IEntityLayoutComponent
{
    public void GetComponentTypes(RevolutionWorld world, List<ComponentType> componentTypes)
    {
        componentTypes.AddRange(new[]
        {
            AbilityState.ToComponentType(world),
            AbilityPriority.ToComponentType(world),
            AbilityRhythmEngineSet.ToComponentType(world)
        });
    }
}