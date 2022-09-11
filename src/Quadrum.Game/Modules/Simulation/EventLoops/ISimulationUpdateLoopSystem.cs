using System;
using Collections.Pooled;
using DefaultEcs;
using revghost.Ecs;
using revghost.Loop.EventSubscriber;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Application;

public interface ISimulationUpdateLoopSubscriber : IEventSubscriber
{
    Entity Subscribe(Action callback, ProcessOrder process = null);
}

public class SimulationUpdateLoop : ISimulationUpdateLoopSubscriber,
    IDisposable
{
    private readonly OrderGroup _orderGroup;
    private Entity _callbackEntity;
    private readonly PooledList<Action> _callbacks = new(ClearMode.Always);

    public SimulationUpdateLoop(World world)
    {
        _orderGroup = new OrderGroup();
        _callbackEntity = world.CreateEntity();
    }

    public void Dispose()
    {
        _orderGroup.Dispose();
        _callbackEntity.Dispose();
    }

    public Entity Subscribe(Action<Entity> callback, ProcessOrder process)
    {
        return Subscribe(() => callback(_callbackEntity), process);
    }

    public Entity Subscribe(Action callback, ProcessOrder process)
    {
        var entity = _orderGroup.Add(process);
        entity.Set(in callback);
        return entity;
    }

    public void Invoke(GameTime gameTime)
    {
        _callbackEntity.Set(gameTime);
        
        if (_orderGroup.Build())
        {
            _callbacks.ClearReference();
            var entities = _orderGroup.Entities;
            for (var index = 0; index < entities.Length; ++index)
                _callbacks.Add(entities[index].Get<Action>());
        }
        
        foreach (var action in _callbacks.Span)
            action();
    }
}