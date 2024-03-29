using System;
using DefaultEcs;
using PataNext.Game.Client.Core.Inputs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Players;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class OnRhythmInputSystem : SimulationSystem
{
    public OnRhythmInputSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            p => p
                .SetGroup<RhythmEngineExecutionGroup>()
        );
    }

    private GameTimeQuery _timeQuery;
    private EngineQuery _engineQuery;
    private Commands _cmd;

    protected override void OnInit()
    {
        _timeQuery = new GameTimeQuery(Simulation);
        _engineQuery = new EngineQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        var time = _timeQuery
            .First()
            .GameTime;
        
        foreach (var engine in _engineQuery)
        {
            if (!_cmd.HasGameRhythmInput(engine.Relative))
                continue;

            ref readonly var input = ref _cmd.ReadGameRhythmInput(engine.Relative);

            var flowBeat = RhythmUtility.GetFlowBeat(engine.State, engine.Settings);
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

                var cmdChainEndFlow = RhythmUtility.GetFlowBeat(
                    engine.CommandState.ChainEndTime,
                    engine.Settings.BeatInterval
                );
                var cmdEndFlow = RhythmUtility.GetFlowBeat(
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
                if (engine.ComboState.Count > 0) // No spamming to get score
                {
                    var multiplier = 1.0f;
                    multiplier = MathUtils.LerpNormalized(multiplier, 2f, ((int) (engine.ComboState.Score * 4)) * 0.25f);

                    if (engine.ComboState.Score >= 1.0f)
                        multiplier += 0.5f;

                    engine.PowerState.Increase((int) ((1f - Math.Abs(pressure.Score)) * multiplier * 5));
                }

                engine.Progress.Add(new RhythmEngineCommandProgress {Value = pressure});
                engine.State.LastPressure = pressure;
            }
        }
    }

    private partial record struct EngineQuery : IQuery<(
        Read<RhythmEngineController> Controller,
        Write<RhythmEngineState> State,
        Read<RhythmEngineSettings> Settings,
        Read<RhythmEngineCommandProgress> Progress,
        Read<RhythmEnginePredictedCommands> Predicted,
        Write<RhythmEngineRecoveryState> Recovery,
        Read<PlayerDescription.Relative> Relative,
        Write<GameCommandState> CommandState,
        Write<GameComboState> ComboState,
        Write<PowerGaugeState> PowerState,
        All<RhythmEngineIsPlaying>)>;

    private partial record struct Commands : GameRhythmInput.Cmd.IRead;
}