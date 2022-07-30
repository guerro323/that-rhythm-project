using System.Collections.Specialized;
using System.Numerics;
using Quadrum.Game.Modules.Simulation.Units.Systems;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Units;

public partial struct UnitControllerState : ISparseComponent
{
    /// <summary>
    /// Enabling any of the value will disable <see cref="UnitPhysicsSystem"/> process on that axis
    /// </summary>
    public BitVector32 ControlOverVelocity;
    /// <summary>
    /// Whether or not the unit can pass through enemies
    /// </summary>
    public bool PassThroughEnemies;

    /// <summary>
    /// If enabled, use <see cref="TargetPosition"/> as the target position.
    /// </summary>
    public bool OverrideTargetPosition;
    public float TargetPosition;

    /// <summary>
    /// Cache variable. Set after <see cref="UnitPhysicsSystem"/>
    /// </summary>
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