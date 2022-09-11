using System;
using Collections.Pooled;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using revecs.Extensions.Buffers;
using revghost.Ecs;
using revghost.Loop.EventSubscriber;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health.EventLoops;

public interface IHealthEventLoopSubscriber : IEventSubscriber
{
    Entity Subscribe(Action<BufferData<Components.ModifyHealthEvent>> action, ProcessOrder order = null);
}

public class HealthEventLoop : IHealthEventLoopSubscriber,
    IDisposable
{
    private readonly OrderGroup _orderGroup;
    private Entity _callbackEntity;
    private readonly PooledList<Action<BufferData<Components.ModifyHealthEvent>>> _callbacks = new(ClearMode.Always);

    public HealthEventLoop(World world)
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
        return Subscribe((BufferData<Components.ModifyHealthEvent> _) => callback(_callbackEntity), process);
    }

    public Entity Subscribe(Action<BufferData<Components.ModifyHealthEvent>> callback, ProcessOrder process)
    {
        var entity = _orderGroup.Add(process);
        entity.Set(in callback);
        return entity;
    }

    public void Invoke(BufferData<Components.ModifyHealthEvent> buffer, GameTime gameTime)
    {
        if (_orderGroup.Build())
        {
            _callbacks.ClearReference();
            var entities = _orderGroup.Entities;
            for (var index = 0; index < entities.Length; ++index)
                _callbacks.Add(entities[index].Get<Action<BufferData<Components.ModifyHealthEvent>>>());
        }
        
        _callbackEntity.Set(buffer);
        _callbackEntity.Set(gameTime);
        foreach (var action in _callbacks.Span)
            action(buffer);
    }
}