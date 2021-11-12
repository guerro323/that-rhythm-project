using System.Numerics;
using System.Runtime.CompilerServices;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Common.Transform;

public partial struct PositionComponent : ISparseComponent
{
    public Vector2 Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PositionComponent(Vector2 value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PositionComponent(float x, float y)
    {
        Value.X = x;
        Value.Y = x;
    }
}