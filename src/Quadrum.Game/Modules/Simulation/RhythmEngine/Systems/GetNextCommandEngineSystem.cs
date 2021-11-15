using System.Collections;
using System.Runtime.InteropServices;
using Collections.Pooled;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Core;
using revecs.Extensions.Generator.Commands;
using revecs.Systems;
using revghost.Shared;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct GetNextCommandEngineSystem : ISystem
{
    private partial struct GetCommandQuery : IQuery<Read<CommandActions>>
    {}

    private partial struct EngineQuery : IQuery<
            Read<RhythmEngineSettings>,
            Read<RhythmEngineState>,
            Write<RhythmEngineExecutingCommand>,
            Read<RhythmEngineCommandProgress>,
            Write<RhythmEnginePredictedCommands>
        >,
        With<RhythmEngineIsPlaying>
    {
    }

    private partial struct Commands : CommandActions.Cmd.IRead, CommandDuration.Cmd.IRead, ICmdEntitySafe
    {
    }

    [RevolutionSystem]
    [DependOn(typeof(RhythmEngineExecutionGroup.Begin)), AddForeignDependency(typeof(RhythmEngineExecutionGroup.End))]
    private static void Method([Query] GetCommandQuery getCommand, [Query] EngineQuery engines, [Cmd] Commands cmd)
    {
        using var _0 = DisposableArray<UEntityHandle>.Rent(getCommand.GetEntityCount(), out var commands);

        var count = 0;
        foreach (var entity in getCommand.Query)
        {
            commands[count++] = entity;
        }

        if (count == 0)
            return;

        using var output = new PooledList<UEntitySafe>();

        foreach (var (settings, state, executing, buffer, predictedBuffer) in engines)
        {
            if (!state.CanRunCommands)
                continue;

            RhythmCommandUtility.GetCommand(
                cmd,
                commands, buffer.Reinterpret<FlowPressure>(), output,
                false, settings.BeatInterval);

            predictedBuffer.Clear();
            predictedBuffer.Reinterpret<UEntitySafe>().AddRange(output.Span);
            if (predictedBuffer.Count == 0)
            {
                RhythmCommandUtility.GetCommand(
                    cmd,
                    commands, buffer.Reinterpret<FlowPressure>(), output,
                    true, settings.BeatInterval
                );
                if (output.Count > 0)
                {
                    predictedBuffer.Reinterpret<UEntitySafe>().AddRange(output.Span);
                }

                continue;
            }


            // this is so laggy clients don't have a weird things when their command has been on another beat on the server
            var targetBeat = buffer[^1].Value.FlowBeat + 1;

            executing.Previous = executing.CommandTarget;
            executing.CommandTarget = output[0];
            executing.ActivationBeatStart = targetBeat;

            var beatDuration = cmd.ReadCommandDuration(executing.CommandTarget.Handle).Value;

            executing.ActivationBeatEnd = targetBeat + beatDuration;
            executing.WaitingForApply = true;

            var power = 0.0f;
            for (var i = 0; i != buffer.Count; i++)
            {
                // perfect
                if (buffer[i].Value.GetAbsoluteScore() <= 0.16f)
                    power += 1.0f;
            }

            executing.__ref.Power = power / buffer.Count;
            buffer.Clear();
        }
    }
}