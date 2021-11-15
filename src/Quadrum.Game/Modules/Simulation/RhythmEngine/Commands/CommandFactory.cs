using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using revecs.Core;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Commands;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;

public static class CommandFactory
{
    public static (UEntityHandle output, BufferData<RhythmCommandAction> buffer) New<TCmd>(TCmd cmd)
        where TCmd : ICmdEntityAdmin, CommandActions.Cmd.IAdmin, CommandDuration.Cmd.IAdmin
    {
        var output = cmd.CreateEntity();
        cmd.AddCommandActions(output);

        return (output, cmd.UpdateCommandActions(output).Reinterpret<RhythmCommandAction>());
    }
}