using revecs.Core;
using revecs.Extensions.Buffers;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineCommandProgress : IBufferComponent
{
    public FlowPressure Value;
}

public partial struct RhythmEnginePredictedCommands : IBufferComponent
{
    public UEntitySafe Value;
}