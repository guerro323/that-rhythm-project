using System;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Application;

public partial struct GameTime : ISparseComponent
{
    public int Frame;
    public TimeSpan Total;
    public TimeSpan Delta;

    /// <summary>
    /// Provide a one length based range with the current frame
    /// </summary>
    public Range FrameRange => new Range(Frame, Frame);
}