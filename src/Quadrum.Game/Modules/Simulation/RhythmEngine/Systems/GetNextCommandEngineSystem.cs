using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Core;
using revecs.Extensions.Generator.Commands;
using revghost;
using revghost.Shared.Collections;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class GetNextCommandEngineSystem : SimulationSystem
{
    public GetNextCommandEngineSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            p => p
                .SetGroup<RhythmEngineExecutionGroup>()
                .After(typeof(OnRhythmInputSystem))
        );
    }

    private CommandQuery _commandQuery;
    private EngineQuery _engineQuery;
    private Commands _cmd;

    protected override void OnInit()
    {
        _commandQuery = new CommandQuery(Simulation);
        _engineQuery = new EngineQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        using var commands = new ValueList<UEntityHandle>(0);
        foreach (var iter in _commandQuery)
        {
            commands.Add(iter.Handle);
        }
        
        if (commands.Count == 0)
            return;

        using var output = new ValueList<UEntitySafe>(0);
        foreach (var engine in _engineQuery)
        {
            if (!engine.State.CanRunCommands)
                continue;

            RhythmCommandUtility.GetCommand(
                _cmd,
                commands.Span, engine.Progress.Reinterpret<FlowPressure>(), in output,
                false, engine.Settings.BeatInterval);

            engine.Predicted.Clear();
            engine.Predicted.Reinterpret<UEntitySafe>().AddRange(output.Span);
            if (engine.Predicted.Count == 0)
            {
                RhythmCommandUtility.GetCommand(
                    _cmd,
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

            var beatDuration = _cmd.ReadCommandDuration(engine.Executing.CommandTarget.Handle).Value;

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

    private partial record struct CommandQuery : IQuery<All<CommandActions>>;

    private partial record struct EngineQuery : IQuery<(
        Read<RhythmEngineState> State,
        Read<RhythmEngineSettings> Settings,
        Read<RhythmEngineCommandProgress> Progress,
        Write<RhythmEnginePredictedCommands> Predicted,
        Write<RhythmEngineExecutingCommand> Executing,
        All<RhythmEngineIsPlaying>)>;

    private partial record struct Commands :
        CommandActions.Cmd.IRead,
        CommandDuration.Cmd.IRead,
        ICmdEntitySafe;
}