using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs;
using revecs.Systems;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ApplyCommandEngineSystem : ISystem
{
    private partial struct EngineQuery : IQuery<
            Read<RhythmEngineSettings>,
            Read<RhythmEngineState>,
            Read<RhythmEngineRecoveryState>,
            Write<RhythmEngineExecutingCommand>,
            Read<RhythmEngineCommandProgress>,
            Write<RhythmEnginePredictedCommands>,
            Read<GameComboSettings>,
            Write<GameComboState>,
            Write<GameCommandState>
        >,
        With<RhythmEngineIsPlaying>
    {
    }

    private partial struct Commands : CommandDuration.Cmd.IRead
    {
    }

    [RevolutionSystem]
    [DependOn(typeof(RhythmEngineExecutionGroup.Begin)), AddForeignDependency(typeof(RhythmEngineExecutionGroup.End))]
    [DependOn(typeof(GetNextCommandEngineSystem))]
    private static void Method([Query] EngineQuery engines, [Cmd] Commands cmd)
    {
        foreach (var (settings,
                     state,
                     recovery,
                     executing,
                     buffer,
                     predictedBuffer,
                     comboSettings,
                     comboState,
                     commandState) in engines)
        {
            if (!state.CanRunCommands)
            {
                commandState.__ref.Reset();
                return;
            }

            // TODO: Apply Ability Selection

            const int mercy = 1; // increase it by one on a server
            const int cmdMercy = 0; // increase it by three on a server

            var rhythmActiveAtFlowBeat = executing.ActivationBeatStart;

            var checkStopBeat = Math.Max(state.LastPressure.FlowBeat + mercy,
                RhythmEngineUtility.GetFlowBeat(new TimeSpan(commandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                    settings.BeatInterval) + cmdMercy);
            if (true) // todo: !isServer && simulateTagFromEntity.Exists(entity)
                checkStopBeat = Math.Max(checkStopBeat,
                    RhythmEngineUtility.GetFlowBeat(new TimeSpan(commandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                        settings.BeatInterval));

            var flowBeat = RhythmEngineUtility.GetFlowBeat(state.__ref, settings.__ref);
            var activationBeat = RhythmEngineUtility.GetActivationBeat(state.__ref, settings.__ref);
            if (recovery.__ref.IsRecovery(flowBeat)
                || rhythmActiveAtFlowBeat < flowBeat && checkStopBeat < activationBeat
                || executing.CommandTarget.Equals(default) && predictedBuffer.Count != 0 &&
                rhythmActiveAtFlowBeat < state.LastPressure.FlowBeat
                || predictedBuffer.Count == 0)
            {
                comboState.__ref = default;
                commandState.__ref.Reset();
                executing.__ref = default;
            }

            if (executing.CommandTarget.Equals(default) || recovery.__ref.IsRecovery(flowBeat))
            {
                commandState.__ref.Reset();
                comboState.__ref = default;
                executing.__ref = default;
                continue;
            }

            if (!executing.WaitingForApply)
                continue;
            executing.WaitingForApply = false;

            var beatDuration = cmd.ReadCommandDuration(executing.CommandTarget.Handle).Value;
            /*foreach (var element in targetResourceBuffer.Span)
                beatDuration = Math.Max(beatDuration, (int) Math.Ceiling(element.Value.Beat.Target + 1 + element.Value.Beat.Offset + element.Value.Beat.SliderLength));*/

            // if (!isServer && settings.UseClientSimulation && simulateTagFromEntity.Exists(entity))
            if (true)
            {
                commandState.ChainEndTimeMs = (int) ((rhythmActiveAtFlowBeat + beatDuration + 4) *
                                                     (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
                commandState.StartTimeMs = (int) (executing.ActivationBeatStart *
                                                  (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
                commandState.EndTimeMs = (int) (executing.ActivationBeatEnd *
                                                (settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));

                var wasFever = comboSettings.__ref.CanEnterFever(comboState.__ref);

                comboState.__ref.Count++;
                comboState.__ref.Score += (float) (executing.Power - 0.5) * 2;
                if (comboState.Score < 0)
                    comboState.Score = 0;

                // We have a little bonus when doing a perfect command
                /*if (executing.IsPerfect
                    && wasFever
                    && HasComponent(entity, AsComponentType<RhythmSummonEnergy>()))
                    GetComponentData<RhythmSummonEnergy>(entity).Value += 20;*/
            }
        }
    }
}