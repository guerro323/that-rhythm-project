using System.Numerics;
using System.Runtime.CompilerServices;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Common.Transform;

public partial struct RotationComponent : ISparseComponent
{
    public float Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RotationComponent(float value)
    {
        Value = value;
    }
}