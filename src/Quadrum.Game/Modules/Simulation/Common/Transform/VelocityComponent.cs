using System.Numerics;
using System.Runtime.CompilerServices;
using Quadrum.Game.Utilities;
using revecs.Extensions.Generator.Components;
using revecs.Utility;

namespace Quadrum.Game.Modules.Simulation.Common.Transform;

public partial struct VelocityComponent : ISparseComponent
{
    public Vector2 Value;

    // TODO: Provide a distinct method to have Velocity be directly Vector2
    public unsafe ref float X => ref Value.Ptr()->Ref().X;
    public unsafe ref float Y => ref Value.Ptr()->Ref().Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VelocityComponent(Vector2 value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VelocityComponent(float x, float y)
    {
        Value.X = x;
        Value.Y = x;
    }
}