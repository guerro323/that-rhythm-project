using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;

public partial struct CommandDuration : ISparseComponent
{
    public int Value;

    public CommandDuration(int value) => Value = value;
}