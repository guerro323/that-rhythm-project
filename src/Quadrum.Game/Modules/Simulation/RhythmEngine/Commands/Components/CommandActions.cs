using revecs.Extensions.Buffers;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;

public partial struct CommandActions : IBufferComponent
{
    public RhythmCommandAction Value;
}