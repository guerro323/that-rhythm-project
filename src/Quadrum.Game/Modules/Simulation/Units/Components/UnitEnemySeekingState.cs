using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Units;

public partial struct UnitEnemySeekingState : ISparseComponent
{
    public UEntitySafe Enemy;
    public float RelativeDistance;
    public float SelfDistance;
}