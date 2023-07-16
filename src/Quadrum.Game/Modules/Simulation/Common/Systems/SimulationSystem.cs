using System;
using DefaultEcs;
using revecs.Core;
using revecs.Querying;
using revecs.Systems;
using revghost;
using revghost.Ecs;
using revghost.Loop.EventSubscriber;
using revtask.Core;

namespace Quadrum.Game.Modules.Simulation.Common.Systems;

public abstract class SimulationSystem : AppSystem
{
    private IJobRunner _runner;
    private RevolutionWorld _simulation;
    
    public SimulationSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _runner);
        Dependencies.Add(() => ref _simulation);
    }

    private List<Func<IEventSubscriber, bool>> _subscribedActionList;

    protected override void OnDependenciesResolved(IReadOnlyList<object> dependencies)
    {
        base.OnDependenciesResolved(dependencies);

        if (_subscribedActionList == null)
            return;
        
        foreach (var dep in dependencies)
        {
            if (dep is not IEventSubscriber eventSubscriber)
                continue;

            for (var i = 0; i < _subscribedActionList.Count; i++)
            {
                if (!_subscribedActionList[i](eventSubscriber))
                    continue;
                
                _subscribedActionList.RemoveAt(i--);
            }
        }
    }

    public ref readonly RevolutionWorld Simulation => ref _simulation;
    public ref readonly IJobRunner Runner => ref _runner;
    
    protected void SubscribeTo<T>(Action<Entity> onEvent, ProcessOrder process = null
        , Type typeToUse = null
        , bool disableArchetypeSynchronization = false)
        where T : IEventSubscriber
    {
        typeToUse ??= GetType();
        
        _subscribedActionList ??= new List<Func<IEventSubscriber, bool>>();
        _subscribedActionList.Add(eventSubscriber =>
        {
            if (eventSubscriber is not T)
                return false;

            if (!disableArchetypeSynchronization)
            {
                var original = onEvent;
                onEvent = e =>
                {
                    Simulation.ArchetypeUpdateBoard.Update();
                    original(e);
                };
            }

            var entity = eventSubscriber.Subscribe(onEvent, process);
            entity.Set(typeToUse);
            
            Disposables.Add(entity);
            return true;
        });
        
        Dependencies.Add(new Dependency(typeof(T)));
    }

    public void SynchronizeArchetypes()
    {
        Simulation.ArchetypeUpdateBoard.Update();
    }

    public void Parallel<T>(ArchetypeQuery query, ArchetypeJob<T>.OnArchetype onArchetype, T arg)
    {
        /*var job = new ArchetypeJob<T>(onArchetype, query, true);
        job.PrepareData(arg);
        Runner.QueueAndComplete(job);*/
        foreach (var archetype in query.GetMatchedArchetypes())
        {
            onArchetype(Simulation.ArchetypeBoard.GetEntities(archetype), new SystemState<T>
            {
                Data = arg,
                World = Simulation
            });
        }
    }
}