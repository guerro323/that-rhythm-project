using System;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineSettings : ISparseComponent
{
    public TimeSpan BeatInterval;
    public int MaxBeats;
}