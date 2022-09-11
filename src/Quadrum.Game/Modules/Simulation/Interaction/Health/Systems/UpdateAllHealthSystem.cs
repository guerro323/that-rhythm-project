using Collections.Pooled;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Interaction.Health.EventLoops;
using revecs;
using revecs.Extensions.Buffers;
using revghost;
using revghost.Ecs;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health.Systems;

public partial class UpdateAllHealthSystem : SimulationSystem
{
    private readonly HostLogger _logger = new(nameof(UpdateAllHealthSystem));

    private HealthEventLoop _loop;

    public UpdateAllHealthSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _loop);

        SubscribeTo<ISimulationUpdateLoopSubscriber>(OnUpdate);
    }

    private BufferData<Components.ModifyHealthEvent> _eventBuffer;
    private EventQuery _eventQuery;
    private LivableQuery _livableQuery;
    private HealthQuery _healthQuery;
    private Commands _cmd;

    protected override void OnInit()
    {
        _eventBuffer = new BufferData<Components.ModifyHealthEvent>(new PooledList<byte>());

        _eventQuery = new EventQuery(Simulation);
        _livableQuery = new LivableQuery(Simulation);
        _healthQuery = new HealthQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity e)
    {
        _eventBuffer.Clear();

        foreach (var entity in _eventQuery)
        {
            ref readonly var ev = ref entity.ModifyHealthEvent;
            if (!Simulation.Exists(ev.Target))
            {
                _logger.Warn($"Entity '{ev.Target}' does not exist", "gather-event");
                continue;
            }

            _eventBuffer.Add(ev);
        }

        // Destroy events (TODO: introduce Query.RemoveAllEntities())
        foreach (var archetype in _eventQuery.Query.GetMatchedArchetypes())
        {
            Simulation.DestroyEntities(Simulation.ArchetypeBoard.GetEntities(archetype));
        }

        _loop.Invoke(_eventBuffer, e.Get<GameTime>());

        foreach (var entity in _livableQuery)
        {
            entity.LivableHealth = default;
        }

        foreach (var entity in _healthQuery)
        {
            if (!_cmd.HasLivableDescription(entity.owner))
                continue;

            ref var livableHealth = ref _cmd.UpdateLivableHealth(entity.owner);
            livableHealth.Value += entity.concrete.Value;
            livableHealth.Max += entity.concrete.Max;
        }
    }

    private partial record struct EventQuery : IQuery<Read<Components.ModifyHealthEvent>>;

    private partial record struct LivableQuery : IQuery<Write<Components.LivableHealth>>;

    private partial record struct HealthQuery : IQuery<(
        Read<Components.ConcreteHealthValue> concrete,
        Read<Components.LivableDescription.Relative> owner,
        All<Components.HealthDescription>)>;

    private partial record struct Commands :
        Components.LivableDescription.Cmd.IRead,
        Components.LivableHealth.Cmd.IWrite;
}