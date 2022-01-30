using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ApplyCommandEngineSystem : IRevolutionSystem,
    CommandDuration.Cmd.IRead
{
    public void Constraints(in SystemObject sys)
    {
        sys.DependOn<RhythmEngineExecutionGroup.Begin>();
        sys.AddForeignDependency<RhythmEngineExecutionGroup.End>();
        {
            sys.DependOn<GetNextCommandEngineSystem>();
        }
    }

    public void Body()
    {
        foreach (var engine in RequiredQuery(
                     Read<RhythmEngineSettings>("Settings"),
                     Read<RhythmEngineState>("State"),
                     Read<RhythmEngineRecoveryState>("Recovery"),
                     Write<RhythmEngineExecutingCommand>("Executing"),
                     Read<RhythmEngineCommandProgress>("Progress"),
                     Write<RhythmEnginePredictedCommands>("Predicted"),
                     Read<GameComboSettings>("ComboSettings"),
                     Write<GameComboState>("ComboState"),
                     Write<GameCommandState>("CommandState"),
                     All<RhythmEngineIsPlaying>()
                 ))
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
                RhythmEngineUtility.GetFlowBeat(new TimeSpan(engine.CommandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                    engine.Settings.BeatInterval) + cmdMercy);
            if (true) // todo: !isServer && simulateTagFromEntity.Exists(entity)
                checkStopBeat = Math.Max(checkStopBeat,
                    RhythmEngineUtility.GetFlowBeat(new TimeSpan(engine.CommandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                        engine.Settings.BeatInterval));

            var flowBeat = RhythmEngineUtility.GetFlowBeat(engine.State, engine.Settings);
            var activationBeat = RhythmEngineUtility.GetActivationBeat(engine.State, engine.Settings);
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

            var beatDuration = Cmd.ReadCommandDuration(engine.Executing.CommandTarget.Handle).Value;
            /*foreach (var element in targetResourceBuffer.Span)
                beatDuration = Math.Max(beatDuration, (int) Math.Ceiling(element.Value.Beat.Target + 1 + element.Value.Beat.Offset + element.Value.Beat.SliderLength));*/

            // if (!isServer && settings.UseClientSimulation && simulateTagFromEntity.Exists(entity))
            if (true)
            {
                engine.CommandState.ChainEndTimeMs = (int) ((rhythmActiveAtFlowBeat + beatDuration + 4) *
                                                     (engine.Settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
                engine.CommandState.StartTimeMs = (int) (engine.Executing.ActivationBeatStart *
                                                         (engine.Settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));
                engine.CommandState.EndTimeMs = (int) (engine.Executing.ActivationBeatEnd *
                                                       (engine.Settings.BeatInterval.Ticks / TimeSpan.TicksPerMillisecond));

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
}