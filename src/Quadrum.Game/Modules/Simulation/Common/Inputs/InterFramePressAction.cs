using System;

namespace PataNext.Game.Client.Core.Inputs;

using Frame = Int32;

public struct InterFramePressAction
{
    public Frame Pressed;
    public Frame Released;

    public readonly bool IsPressed(Range range)
    {
        return Contains(range, Pressed);
    }

    public readonly bool IsReleased(Range range)
    {
        return Contains(range, Released);
    }

    public readonly bool AnyUpdate(Range range)
    {
        return IsPressed(range) || IsReleased(range);
    }

    private static bool Contains(Range range, Frame frame)
    {
        return frame >= range.Start.Value && frame <= range.End.Value;
    }
}