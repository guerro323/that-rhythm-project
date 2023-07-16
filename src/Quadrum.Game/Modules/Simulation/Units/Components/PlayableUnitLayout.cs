using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Modules.Simulation.Teams;
using revecs.Core;
using revecs.Extensions.EntityLayout;

namespace Quadrum.Game.Modules.Simulation.Units;

public partial struct PlayableUnitLayout : IEntityLayoutComponent
{
    public void GetComponentTypes(RevolutionWorld world, List<ComponentType> componentTypes)
    {
        componentTypes.Add(UnitDescription.ToComponentType(world));
        componentTypes.Add(PositionComponent.ToComponentType(world));
        componentTypes.Add(VelocityComponent.ToComponentType(world));
        componentTypes.Add(UnitControllerState.ToComponentType(world));
        componentTypes.Add(UnitDirection.ToComponentType(world));
        componentTypes.Add(GroundState.ToComponentType(world));
        componentTypes.Add(UnitStatistics.ToComponentType(world));
        componentTypes.Add(UnitPlayState.ToComponentType(world));
        componentTypes.Add(CursorOffset.ToComponentType(world));
            
        componentTypes.Add(ContributeToTeamMovableArea.ToComponentType(world));
        
        // Enemy
        componentTypes.Add(UnitEnemySeekingState.ToComponentType(world));
    }
}