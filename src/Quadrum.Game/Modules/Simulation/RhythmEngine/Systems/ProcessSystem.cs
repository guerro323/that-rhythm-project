using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class ProcessSystem : SimulationSystem
{
    public ProcessSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            p => p
                .SetGroup<RhythmEngineExecutionGroup>()
                .After(typeof(ApplyTagsSystem))
        );
    }

    private GameTimeQuery _gameTimeQuery;
    private EngineQuery _engineQuery;
    private Commands _cmd;
    
    protected override void OnInit()
    {
        _gameTimeQuery = new GameTimeQuery(Simulation);
        _engineQuery = new EngineQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        var time = _gameTimeQuery.First().GameTime;
        
        foreach (var engine in _engineQuery)
        {
            if (engine.Controller.StartTime != engine.State.PreviousStartTime)
            {
                engine.State.PreviousStartTime = engine.Controller.StartTime;
                engine.State.Elapsed = time.Total - engine.Controller.StartTime;
            }

            engine.State.Elapsed += time.Delta;

            if (engine.State.Elapsed < TimeSpan.Zero)
            {
                _cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
            }

            var nextCurrentBeats = RhythmUtility.GetActivationBeat(engine.State, engine.Settings);
            if (engine.State.CurrentBeat != nextCurrentBeats)
                engine.State.NewBeatTick = (uint) time.Frame;

            engine.State.CurrentBeat = nextCurrentBeats;
        }
    }

    private partial record struct EngineQuery : IQuery<(
        Write<RhythmEngineState> State,
        Read<RhythmEngineSettings> Settings,
        Read<RhythmEngineController> Controller,
        All<RhythmEngineIsPlaying>)>;


    private partial record struct Commands : RhythmEngineIsPlaying.Cmd.IAdmin;
}