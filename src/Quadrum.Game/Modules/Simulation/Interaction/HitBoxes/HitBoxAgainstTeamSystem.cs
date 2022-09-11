using System;
using System.Numerics;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Physics;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.Health.Components;
using Quadrum.Game.Modules.Simulation.Teams;
using revecs;
using revecs.Core;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Commands;
using revecs.Extensions.Generator.Components;
using revghost;
using revghost.Domains.Time;
using revghost.Shared.Threading.Schedulers;
using revghost.Threading;

namespace Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;

public partial class HitBoxAgainstTeamSystem : SimulationSystem
{
    public partial record struct SystemEventTag : ITagComponent;

    private IManagedWorldTime _worldTime;
    private HitBoxQuery.OnEntities<ValueTuple> _onForeachHitBoxes;
    private ConcurrentScheduler _scheduler;

    public HitBoxAgainstTeamSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _worldTime);

        SubscribeTo<ISimulationUpdateLoopSubscriber>(OnUpdate);

        _scheduler = new ConcurrentScheduler();
        _onForeachHitBoxes = (_, entities) =>
        {
            var dt = (float) _worldTime.Delta.TotalSeconds;
            var cmd = _cmd;

            var mask = _mask.Query;
            var archetypes = Simulation.EntityBoard.Archetypes;
            
            foreach (var iter in entities)
            {
                if (!cmd.HasTeamDescription(iter.component.TeamTarget.Handle))
                    throw new InvalidOperationException($"{iter.component.TeamTarget} doesn't point to a team");
                
                BufferData<HitBoxHistory> history = default;
                if (cmd.HasHitBoxHistory(iter.Handle))
                {
                    history = cmd.ReadHitBoxHistory(iter.Handle);
                }

                ref readonly var hitbox = ref iter.hitbox;
                if (history.IsCreated && history.Count >= hitbox.MaxHits)
                    continue;

                var thisPosition = iter.pos.Value;
                var thisVelocity = Vector2.Zero;
                if (cmd.HasVelocityComponent(iter.Handle))
                {
                    thisVelocity = cmd.ReadVelocityComponent(iter.Handle).Value;
                    if (hitbox.VelocityUseDelta)
                        thisVelocity *= dt;
                }

                var hostiles = Span<UEntityHandle>.Empty;
                if (cmd.HasTeamHostileDescription(iter.Handle))
                    hostiles = cmd.ReadTeamHostileDescription(iter.Handle);

                foreach (var hostile in hostiles)
                {
                    var children = cmd.ReadTeamDescription(hostile);
                    foreach (var child in children)
                    {
                        if (false == mask.GetMatchedArchetypes()
                                .Contains(archetypes[child.Id]))
                            continue;

                        var safeChild = cmd.Safe(child);
                        if (history.Contains((ref HitBoxHistory h) => ref h.Victim, safeChild))
                            continue;

                        var o = cmd.Distance(new DistanceInput(thisVelocity, origin: new DistanceInput.EntityOverrides
                        {
                            Position = thisPosition
                        }), iter.Handle, child);

                        if (!o.Collided)
                            continue;

                        if (history.IsCreated)
                            history.Add(new HitBoxHistory(safeChild, o.Position, o.Normal, o.Distance));

                        _scheduler.Add(static args =>
                        {
                            var (cmd, hitBox, victim, o) = args;

                            var ev = cmd.CreateEntity();
                            var instigator = default(UEntityHandle);
                            if (cmd.HasHitBoxOwnerDescriptionRelative(hitBox))
                                instigator = cmd.ReadHitBoxOwnerDescriptionRelative(hitBox);

                            cmd.AddHitBoxEvent(ev, new HitBoxEvent(
                                cmd.Safe(hitBox),
                                cmd.Safe(instigator),
                                victim,

                                o.Position,
                                o.Normal,
                                o.Distance
                            ));
                        }, (cmd, iter.Handle, safeChild, o));
                    }
                }
            }
        };
    }

    private HitBoxQuery _hitboxQuery;
    private EventQuery _eventQuery;
    private ColliderMask _mask;

    private Commands _cmd;

    protected override void OnInit()
    {
        _hitboxQuery = new HitBoxQuery(Simulation);
        _eventQuery = new EventQuery(Simulation);
        _mask = new ColliderMask(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        // TODO: see if it's safe to do it while _hitboxQuery is being executed
        // (It was previously safe to do it in the previous PataNext version)
        foreach (var archetype in _eventQuery.Query.GetMatchedArchetypes())
        {
            Simulation.DestroyEntities(Simulation.ArchetypeBoard.GetEntities(archetype));
        }

        _hitboxQuery.QueueAndComplete(Runner, _onForeachHitBoxes);
        _scheduler.Run();
    }

    private partial record struct EventQuery : IQuery<All<SystemEventTag>>;

    private partial record struct ColliderMask : IQuery<(
        All<PhysicsCollider>,
        All<PositionComponent>,
        None<LivableIsDead>)>;

    private partial record struct HitBoxQuery : IQuery<(
        Read<HitBox> hitbox,
        Read<HitBoxAgainstTeam> component,
        Read<PositionComponent> pos,
        All<PhysicsCollider>)>;

    private partial record struct Commands :
        HitBoxHistory.Cmd.IWrite,
        VelocityComponent.Cmd.IRead,
        TeamHostileDescription.Cmd.IRead,
        TeamDescription.Cmd.IRead,
        HitBoxEvent.Cmd.IAdmin,
        HitBoxOwnerDescription.Relative.Cmd.IRead,
        IPhysicsCmdRead,
        ICmdEntitySafe,
        ICmdEntityAdmin;
}