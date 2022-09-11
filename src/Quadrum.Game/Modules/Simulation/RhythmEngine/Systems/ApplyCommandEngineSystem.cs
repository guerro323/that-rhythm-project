using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class ApplyCommandEngineSystem : SimulationSystem
{
    public ApplyCommandEngineSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            p => p
                .SetGroup<RhythmEngineExecutionGroup>()
                .After(typeof(GetNextCommandEngineSystem))
        );
    }
    
    private EngineQuery _engineQuery;
    private Commands _cmd;

    protected override void OnInit()
    {
        _engineQuery = new EngineQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        foreach (var engine in _engineQuery)
        {
            if (!engine.State.CanRunCommands)
            {
                engine.CommandState.Reset();
                return;
            }

            // TODO: Apply Ability Selection

            const int mercy = 1; // increase it by one on a server
            const int cmdMercy = 0; // increase it by three on a server

            var rhythmActiveAtFlowBeat = engine.Executing.ActivationBeatStart;

            var checkStopBeat = Math.Max(engine.State.LastPressure.FlowBeat + mercy,
                RhythmUtility.GetFlowBeat(new TimeSpan(engine.CommandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                    engine.Settings.BeatInterval) + cmdMercy);
            if (true) // todo: !isServer && simulateTagFromEntity.Exists(entity)
                checkStopBeat = Math.Max(checkStopBeat,
                    RhythmUtility.GetFlowBeat(
                        new TimeSpan(engine.CommandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                        engine.Settings.BeatInterval));

            var flowBeat = RhythmUtility.GetFlowBeat(engine.State, engine.Settings);
            var activationBeat = RhythmUtility.GetActivationBeat(engine.State, engine.Settings);
            if (engine.Recovery.IsRecovery(flowBeat)
                || rhythmActiveAtFlowBeat < flowBeat && checkStopBeat < activationBeat
                || engine.Executing.CommandTarget.Equals(default) && engine.Predicted.Count != 0 &&
                rhythmActiveAtFlowBeat < engine.State.LastPressure.FlowBeat
                || engine.Predicted.Count == 0)
            {
                engine.CommandState.Reset();
                engine.ComboState = default;
                engine.Executing = default;
            }

            if (engine.Executing.CommandTarget.Equals(default) || engine.Recovery.IsRecovery(flowBeat))
            {
                engine.CommandState.Reset();
                engine.ComboState = default;
                engine.Executing = default;
                continue;
            }

            if (!engine.Executing.WaitingForApply)
                continue;
            engine.Executing.WaitingForApply = false;

            var beatDuration = _cmd.ReadCommandDuration(engine.Executing.CommandTarget.Handle).Value;
            /*foreach (var element in targetResourceBuffer.Span)
                beatDuration = Math.Max(beatDuration, (int) Math.Ceiling(element.Value.Beat.Target + 1 + element.Value.Beat.Offset + element.Value.Beat.SliderLength));*/

            // if (!isServer && settings.UseClientSimulation && simulateTagFromEntity.Exists(entity))
            if (true)
            {
                engine.CommandState.ChainEndTimeMs = (int) ((rhythmActiveAtFlowBeat + beatDuration + 4) *
                                                            (engine.Settings.BeatInterval.Ticks /
                                                             TimeSpan.TicksPerMillisecond));
                engine.CommandState.StartTimeMs = (int) (engine.Executing.ActivationBeatStart *
                                                         (engine.Settings.BeatInterval.Ticks /
                                                          TimeSpan.TicksPerMillisecond));
                engine.CommandState.EndTimeMs = (int) (engine.Executing.ActivationBeatEnd *
                                                       (engine.Settings.BeatInterval.Ticks /
                                                        TimeSpan.TicksPerMillisecond));

                var wasFever = engine.ComboSettings.CanEnterFever(engine.ComboState);

                engine.ComboState.Count++;
                engine.ComboState.Score += (float) (engine.Executing.Power - 0.5) * 2;
                if (engine.ComboState.Score < 0)
                    engine.ComboState.Score = 0;

                // We have a little bonus when doing a perfect command
                /*if (executing.IsPerfect
                    && wasFever
                    && HasComponent(entity, AsComponentType<RhythmSummonEnergy>()))
                    GetComponentData<RhythmSummonEnergy>(entity).Value += 20;*/
            }
        }
    }

    private partial record struct EngineQuery : IQuery<(
        Read<RhythmEngineState> State,
        Read<RhythmEngineSettings> Settings,
        Read<RhythmEngineCommandProgress> Progress,
        Read<RhythmEnginePredictedCommands> Predicted,
        Write<RhythmEngineExecutingCommand> Executing,
        Read<RhythmEngineRecoveryState> Recovery,
        Write<GameCommandState> CommandState,
        Read<GameComboSettings> ComboSettings,
        Write<GameComboState> ComboState,
        All<RhythmEngineIsPlaying>)>;

    private partial record struct Commands : CommandDuration.Cmd.IRead;
}