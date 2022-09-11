using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Interaction.Health.Components;
using Quadrum.Game.Modules.Simulation.Interaction.Health.EventLoops;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Components;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health.Implementations;

public partial record struct SimpleHealthImpl(int Value, int Max) : ISparseComponent;

public partial class SimpleHealthImplSystem : SimulationSystem
{
    public SimpleHealthImplSystem(Scope scope) : base(scope)
    {
        SubscribeTo<IHealthEventLoopSubscriber>(OnHealthEvents);
    }

    private Commands _cmd;

    protected override void OnInit()
    {
        _cmd = new Commands(Simulation);
    }

    private void OnHealthEvents(Entity e)
    {
        var buffer = e.Get<BufferData<ModifyHealthEvent>>();
        foreach (ref var ev in buffer)
        {
            foreach (var healthEntity in _cmd.ReadLivableDescription(ev.Target.Handle))
            {
                if (!_cmd.HasSimpleHealthImpl(healthEntity))
                    continue;

                ref var data = ref _cmd.UpdateSimpleHealthImpl(healthEntity);
                var difference = data.Value;
                data.Value = ev.Modifier switch
                {
                    HealthModifier.Add => Math.Clamp(data.Value + ev.Consumed, 0, data.Max),
                    HealthModifier.Fixed => Math.Clamp(ev.Consumed, 0, data.Max),
                    HealthModifier.Max => data.Max,
                    HealthModifier.None => 0,
                    _ => data.Value
                };

                ev.Consumed -= Math.Abs(data.Value - difference);
            }
        }
    }

    private partial record struct Commands : 
        LivableDescription.Cmd.IRead, 
        SimpleHealthImpl.Cmd.IWrite;
} 