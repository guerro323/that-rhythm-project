using System;
using System.Numerics;
using revecs.Extensions.Buffers;

namespace Quadrum.Game.Modules.Simulation.Units;

public partial record struct UnitWeakPoint(Vector2 Value) : IBufferComponent
{
}

public static class UnitWeakPointExtensions2
{
    public static (Vector2 pos, float dist) GetNearest(this Span<UnitWeakPoint> buffer, in Vector2 local)
    {
        var result = (Vector2.Zero, dist: -1f);
        foreach (var point in buffer)
        {
            var newDist = Vector2.Distance(point.Value, local);
            if (newDist < result.dist || result.dist < 0)
                result = (point.Value, newDist);
        }
        return result;
    } 

    public static bool TryGetNearest(this Span<UnitWeakPoint> buffer, in Vector2 local, out (Vector2 pos, float dist) result)
    {
        result = GetNearest(buffer, local);
        return result.dist >= 0;
    }
}