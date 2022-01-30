using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Core;
using revecs.Extensions.Generator.Commands;
using revecs.Systems.Generator;
using revghost.Shared.Collections;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct GetNextCommandEngineSystem : IRevolutionSystem,
    CommandActions.Cmd.IRead, 
    CommandDuration.Cmd.IRead, 
    ICmdEntitySafe
{
    public void Constraints(in SystemObject sys)
    {
        sys.DependOn<RhythmEngineExecutionGroup.Begin>();
        sys.AddForeignDependency<RhythmEngineExecutionGroup.End>();
    }
    
    public void Body()
    {
        using var commands = new ValueList<UEntityHandle>(0);
        foreach (var iter in RequiredQuery(All<CommandActions>()))
        {
            commands.Add(iter.Handle);
        }
        
        if (commands.Count == 0)
            return;

        using var output = new ValueList<UEntitySafe>(0);
        foreach (var engine in RequiredQuery(
                     Read<RhythmEngineSettings>("Settings"),
                     Read<RhythmEngineState>("State"),
                     Write<RhythmEngineExecutingCommand>("Executing"),
                     Read<RhythmEngineCommandProgress>("Progress"),
                     Write<RhythmEnginePredictedCommands>("Predicted"),
                     All<RhythmEngineIsPlaying>()))
        {
            if (!engine.State.CanRunCommands)
                continue;

            RhythmCommandUtility.GetCommand(
                Cmd,
                commands.Span, engine.Progress.Reinterpret<FlowPressure>(), in output,
                false, engine.Settings.BeatInterval);

            engine.Predicted.Clear();
            engine.Predicted.Reinterpret<UEntitySafe>().AddRange(output.Span);
            if (engine.Predicted.Count == 0)
            {
                RhythmCommandUtility.GetCommand(
                    Cmd,
                    commands.Span, engine.Progress.Reinterpret<FlowPressure>(), in output,
                    true, engine.Settings.BeatInterval);
                if (output.Count > 0)
                {
                    engine.Predicted.Reinterpret<UEntitySafe>().AddRange(output.Span);
                }

                continue;
            }


            // this is so laggy clients don't have a weird things when their command has been on another beat on the server
            var targetBeat = engine.Progress[^1].Value.FlowBeat + 1;

            engine.Executing.Previous = engine.Executing.CommandTarget;
            engine.Executing.CommandTarget = output[0];
            engine.Executing.ActivationBeatStart = targetBeat;

            var beatDuration = Cmd.ReadCommandDuration(engine.Executing.CommandTarget.Handle).Value;

            engine.Executing.ActivationBeatEnd = targetBeat + beatDuration;
            engine.Executing.WaitingForApply = true;

            var power = 0.0f;
            for (var i = 0; i != engine.Progress.Count; i++)
            {
                // perfect
                if (engine.Progress[i].Value.GetAbsoluteScore() <= 0.16f)
                    power += 1.0f;
            }

            engine.Executing.Power = power / engine.Progress.Count;
            engine.Progress.Clear();
        }
    }
}