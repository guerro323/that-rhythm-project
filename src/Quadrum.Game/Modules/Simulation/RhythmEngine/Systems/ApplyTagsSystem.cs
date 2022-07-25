using System;
using System.ComponentModel;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Core;
using revecs.Systems.Generator;
using revghost;
using revghost.Domains.Time;
using revghost.Ecs;
using revghost.Injection.Dependencies;
using revghost.Loop.EventSubscriber;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial struct ApplyTagsSystem : IRevolutionSystem,
    RhythmEngineIsPlaying.Cmd.IAdmin,
    RhythmEngineIsPaused.Cmd.IAdmin
{
    public void Constraints(in SystemObject sys)
    {
        sys.SetGroup<RhythmEngineExecutionGroup>();
    }

    public void Body()
    {
        foreach (var engine in RequiredQuery(Read<RhythmEngineController>("Controller")))
        {
            switch (engine.Controller.State)
            {
                case RhythmEngineController.EState.Playing:
                    Cmd.AddRhythmEngineIsPlaying(engine.Handle);
                    Cmd.RemoveRhythmEngineIsPaused(engine.Handle);
                    break;
                case RhythmEngineController.EState.Paused:
                    Cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
                    Cmd.AddRhythmEngineIsPaused(engine.Handle);
                    break;
                case RhythmEngineController.EState.Stopped:
                    Cmd.RemoveRhythmEngineIsPlaying(engine.Handle);
                    Cmd.RemoveRhythmEngineIsPaused(engine.Handle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public partial class V2ApplyTagsSystem : AppSystem
{
    private RevolutionWorld _world;
    private IDomainUpdateLoopSubscriber _updateLoop;

    public V2ApplyTagsSystem(Scope scope) : base(scope)
    {
        Dependencies.AddRef(() => ref _world);
        Dependencies.AddRef(() => ref _updateLoop);
    }
    
    private Commands _cmd;
    private EngineQuery _query;

    protected override void OnInit()
    {
        _cmd = new Commands(_world);
        
        Disposables.AddRange(new IDisposable[]
        {
            (_query = new EngineQuery(_world)).Query,
            _updateLoop.Subscribe(OnUpdate)
        });
    }

    private void OnUpdate(WorldTime time)
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