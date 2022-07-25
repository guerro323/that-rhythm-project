using System.Collections.Specialized;
using System.Numerics;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Units;

public partial struct UnitControllerState : ISparseComponent
{
    public BitVector32 ControlOverVelocity;
    public bool PassThroughEnemies;

    public bool OverrideTargetPosition;
    public float TargetPosition;

    public Vector2 PreviousPosition;

    public bool ControlOverVelocityX
    {
        get => ControlOverVelocity[1];
        set => ControlOverVelocity[1] = value;
    }
    
    public bool ControlOverVelocityY
    {
        get => ControlOverVelocity[2];
        set => ControlOverVelocity[2] = value;
    }
}