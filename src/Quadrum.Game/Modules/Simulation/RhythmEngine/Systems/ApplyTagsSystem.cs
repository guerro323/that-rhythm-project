using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Core;
using revghost;
using revghost.Loop.EventSubscriber;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class ApplyTagsSystem : SimulationSystem
{
    private RevolutionWorld _world;
    private IDomainUpdateLoopSubscriber _updateLoop;

    public ApplyTagsSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);
        Dependencies.Add(() => ref _updateLoop);

        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b.SetGroup<RhythmEngineExecutionGroup>()
        );
    }
    
    private Commands _cmd;
    private EngineQuery _query;

    protected override void OnInit()
    {
        _cmd = new Commands(_world);
        
        Disposables.AddRange(new IDisposable[]
        {
            (_query = new EngineQuery(_world)).Query
        });
    }

    private void OnUpdate(Entity _)
    {
        foreach (var engine in _query)
        {
            ref readonly var controller = ref engine.RhythmEngineController;
            
            switch (controller.State)
            {
                case RhythmEngineController.EState.Playing:
                    _cmd.AddRhythmEngineIsPlaying(engine.Handle);
                    _cmd.RemoveRhythmEngineIsPaused(engine.Handle);
                    break;
                case RhythmEngineController.EState.Paused:
                    _cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
                    _cmd.AddRhythmEngineIsPaused(engine.Handle);
                    break;
                case RhythmEngineController.EState.Stopped:
                    _cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
                    _cmd.RemoveRhythmEngineIsPaused(engine.Handle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public partial struct Commands :
        RhythmEngineIsPlaying.Cmd.IAdmin,
        RhythmEngineIsPaused.Cmd.IAdmin
    {}
    
    public partial struct EngineQuery : IQuery<Read<RhythmEngineController>> {}
}