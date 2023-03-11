using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;
using revecs;
using revecs.Core.Components.Boards;
using revecs.Extensions.Generator.Commands;
using revecs.Querying;
using revghost;
using revghost.Loop.EventSubscriber;

namespace Quadrum.Game.Modules.Simulation.Interaction.Damage;

public partial class GenerateDamageRequestSystem : SimulationSystem
{
    public GenerateDamageRequestSystem(Scope scope) : base(scope)
    {
        SubscribeTo<IDomainUpdateLoopSubscriber>(
            OnUpdate,
            b => b.After(typeof(HitBoxAgainstTeamSystem))
        );
    }

    private ArchetypeQuery _toDestroy;
    private EventQuery _eventQuery;
    private Commands _cmd;

    protected override void OnInit()
    {
        _toDestroy = new ArchetypeQuery(Simulation, new[]
        {
            Simulation.RegisterComponent(nameof(GenerateDamageRequestSystem), new TagComponentBoard(Simulation))
        });
        _eventQuery = new EventQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity e)
    {
        foreach (var entity in _eventQuery)
        {
            ref readonly var ev = ref entity.HitBoxEvent;

            var frameData = default(DamageFrameData);
            
            var damageEv = Simulation.CreateEntity();
            _cmd.AddTargetDamageEvent(damageEv, new TargetDamageEvent(ev.Instigator, ev.Victim, -frameData.Attack));
            _cmd.AddDamageFrameData(damageEv, frameData);
            _cmd.AddPositionComponent(damageEv, new PositionComponent(ev.ContactPosition));
            Simulation.AddComponent(damageEv, _toDestroy.All[0]);
            
            // TODO: status buffer
            // TODO: client rpc replication and server networked entity
            // (https://github.com/patanext-project/patanext/blob/master/PataNext.Simulation.Mixed/Game/Systems/GamePlay/Damage/GenerateDamageRequestSystem.cs#L77)
        }
    }

    private partial record struct EventQuery : IQuery<Read<HitBoxEvent>>;

    private partial record struct Commands :
        TargetDamageEvent.Cmd.IAdmin,
        DamageFrameData.Cmd.IAdmin,
        PositionComponent.Cmd.IAdmin,
        ICmdEntityAdmin;
}