using System;
using System.Threading;
using System.Threading.Tasks;
using Collections.Pooled;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revecs.Core;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revecs.Querying;
using revghost;
using revghost.Shared.Threading.Tasks;
using revghost.Threading;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Common.GameMode;

public abstract partial class GameModeSystemBase : SimulationSystem
{
    private readonly HostLogger _logger = new("gamemode");
    
    // don't use the one from the dependency
    private ConstrainedTaskScheduler _taskScheduler = new();
    
    protected GameModeSystemBase(Scope scope) : base(scope)
    {
        SubscribeToUpdateLoop();
    }

    private ArchetypeQuery _query;
    private Commands _cmd;

    protected override void OnInit()
    {
        using var all = new PooledList<ComponentType>();
        using var none = new PooledList<ComponentType>();
        using var or = new PooledList<ComponentType>();
        GetComponentTypes(all, none, or);

        _query = new ArchetypeQuery(Simulation, all.Span, none.Span, or.Span);
        _cmd = new Commands(Simulation);
    }

    protected virtual void SubscribeToUpdateLoop()
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(OnUpdate);
    }

    protected void OnUpdate(Entity _)
    {
        // Don't run gamemodes in parallel (no big gain with threading issues)
        foreach (var entity in _query)
        {
            switch (_cmd.HasGameModeAttachedTask(entity))
            {
                case true:
                {
                    break;
                }
                case false:
                {
                    var task = _taskScheduler.StartUnwrap(async () =>
                    {
                        var cts = new CancellationTokenSource();
                        var task = _taskScheduler.StartUnwrap(() => GetStateMachine(_cmd.Safe(entity), cts.Token));
                        while (_cmd.Exists(entity))
                        {
                            if (task.IsFaulted)
                            {
                                _logger.Error($"entity={_cmd.Safe(entity)} exception={task.Exception}", "crash");
                                
                                OnCrash(_cmd.Safe(entity), task.Exception);
                                _cmd.DestroyEntity(entity);
                                return;
                            }

                            if (task.IsCompleted)
                            {
                                var result = task.Result;
                                _logger.Info($"entity={_cmd.Safe(entity)} remove={result}", "completion");
                                
                                if (result)
                                    _cmd.DestroyEntity(entity);
                            }

                            await Task.Yield();
                        }
                        
                        cts.Cancel();
                    });
                    _cmd.AddGameModeAttachedTask(entity, new GameModeAttachedTask(task));
                    break;
                }
            }
        }
        
        _taskScheduler.Execute();
    }

    protected abstract void GetComponentTypes<TList>(TList all, TList none, TList or)
        where TList : IList<ComponentType>;

    protected abstract Task<bool> GetStateMachine(UEntitySafe gameModeEntity, CancellationToken token);
    protected abstract void OnCrash(UEntitySafe gameModeEntity, Exception exception);

    public partial record struct GameModeAttachedTask(Task State) : ISparseComponent;

    private partial record struct Commands : ICmdEntitySafe, ICmdEntityAdmin, GameModeAttachedTask.Cmd.IAdmin;
}