using System;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components.Aspects;

public partial struct MarchAbilityAspect : ISparseComponent
{
    /// <summary>
    /// Which entity should be moved?
    /// </summary>
    [Flags]
    public enum ETarget
    {
        None = 0,
        Cursor = 1 << 1,
        Movement = 1 << 2,
        All = Cursor | Movement
    }

    /// <summary>
    /// Is the aspect active?
    /// </summary>
    public bool IsActive;

    /// <summary>
    /// The target to use when <see cref="IsActive"/> is true
    /// </summary>
    public ETarget Target;

    /// <summary>
    /// The acceleration factor to use when <see cref="IsActive"/> is true
    /// </summary>
    /// <remarks>
    /// A negative factor can be given (the entity will go in reverse)
    /// </remarks>
    public float AccelerationFactor;

    /// <summary>
    /// How much time was this aspect active?
    /// </summary>
    public float ActiveTime;
}