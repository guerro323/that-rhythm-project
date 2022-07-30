using System.Numerics;
using System.Runtime.CompilerServices;
using Quadrum.Game.Utilities;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Common.Transform;

public partial struct PositionComponent : ISparseComponent
{
    public Vector2 Value;
    
    public unsafe ref float X => ref Value.Ptr()->Ref().X;
    public unsafe ref float Y => ref Value.Ptr()->Ref().Y;

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