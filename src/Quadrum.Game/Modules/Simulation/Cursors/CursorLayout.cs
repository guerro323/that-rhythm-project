using Quadrum.Game.Modules.Simulation.Common.Transform;
using revecs.Core;
using revecs.Extensions.EntityLayout;

namespace Quadrum.Game.Modules.Simulation.Cursors;

public partial struct CursorLayout : IEntityLayoutComponent
{
    public void GetComponentTypes(RevolutionWorld world, List<ComponentType> componentTypes)
    {
        componentTypes.Add(CursorDescription.ToComponentType(world));
        componentTypes.Add(PositionComponent.ToComponentType(world));
    }
}