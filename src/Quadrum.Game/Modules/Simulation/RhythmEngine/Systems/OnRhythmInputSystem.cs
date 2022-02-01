using PataNext.Game.Client.Core.Inputs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct OnRhythmInputSystem : IRevolutionSystem,
    GameRhythmInput.Cmd.IRead
{
    public void Constraints(in SystemObject sys)
    {
        sys.DependOn<RhythmEngineExecutionGroup.Begin>();
        sys.AddForeignDependency<RhythmEngineExecutionGroup.End>();
    }

    public void Body()
    {
        var time = RequiredResource<GameTime>();

        foreach (var engine in RequiredQuery(
                     Read<RhythmEngineController>("Controller"),
                     Write<RhythmEngineState>("State"),
                     Read<RhythmEngineSettings>("Settings"),
                     Read<RhythmEngineCommandProgress>("Progress"),
                     Read<RhythmEnginePredictedCommands>("Predicted"),
                     Write<RhythmEngineRecoveryState>("Recovery"),
                     Read<PlayerDescription.Relative>("Relative"),
                     Write<GameCommandState>("CommandState"),
                     All<RhythmEngineIsPlaying>()))
        {
            if (!Cmd.HasGameRhythmInput(engine.Relative))
                continue;

            ref readonly var input = ref Cmd.ReadGameRhythmInput(engine.Relative);

            var flowBeat = RhythmEngineUtility.GetFlowBeat(engine.State, engine.Settings);
            // Don't accept inputs when the rhythm engine hasn't yet started
            if (flowBeat < 0)
                return;

            for (var i = 0; i < input.Actions.Length; i++)
            {
                ref readonly var action = ref input.Actions[i];
                if (!action.InterFrame.AnyUpdate(time.FrameRange))
                    continue;

                // If this is not the end of a slider or if it is but our command buffer is empty, skip it!
                if (action.InterFrame.IsReleased(time.FrameRange) && (!action.IsSliding || engine.Progress.Count == 0))
                    continue;

                var cmdChainEndFlow = RhythmEngineUtility.GetFlowBeat(
                    engine.CommandState.ChainEndTime,
                    engine.Settings.BeatInterval
                );
                var cmdEndFlow = RhythmEngineUtility.GetFlowBeat(
                    engine.CommandState.EndTime,
                    engine.Settings.BeatInterval
                );

                // check for one beat space between inputs (should we just check for predicted commands? 'maybe' we would have a command with one beat space)
                var failFlag1 = engine.Progress.Count > 0
                                && engine.Predicted.Count == 0
                                && flowBeat > engine.Progress[^1].Value.FlowBeat + 1
                                && cmdChainEndFlow > 0;
                // check if this is the first input and was started after the command input time
                var failFlag3 = flowBeat > cmdEndFlow
                                && engine.Progress.Count == 0
                                && cmdEndFlow > 0;
                // check for inputs that were done after the current command chain
                var failFlag2 = flowBeat >= cmdChainEndFlow
                                && cmdChainEndFlow > 0;
                failFlag2 = false; // this flag is deactivated for delayed reborn ability
                var failFlag0 = cmdEndFlow > flowBeat;

                if (failFlag0 || failFlag1 || failFlag2 || failFlag3)
                {
                    engine.Recovery.RecoveryActivationBeat = flowBeat + 1;
                    engine.CommandState = default;
                    continue;
                }

                var pressure = new FlowPressure(i + 1, engine.State.Elapsed, engine.Settings.BeatInterval)
                {
                    IsSliderEnd = action.IsSliding
                };

                // TODO: add an event (eg: for increasing summon energy)

                engine.Progress.Add(new RhythmEngineCommandProgress {Value = pressure});
                engine.State.LastPressure = pressure;
            }
        }
    }
}