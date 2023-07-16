using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation;
using Quadrum.Game.Modules.Simulation.Abilities;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Physics;
using Quadrum.Game.Modules.Simulation.Common.SystemBase;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;
using Quadrum.Game.Modules.Simulation.Teams;
using Quadrum.Game.Utilities;
using revecs;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revecs.Systems;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Shared.Threading.Schedulers;
using revghost.Threading;

namespace Quadrum.Modules.Abilities;

public partial struct ProjectileSettings : ISparseComponent
{
    public float ColliderRadius;
    public Vector2 Gravity;
}

public class BasicProjectileProvider : BaseProvider<(UEntitySafe owner, Vector2 pos, Vector2 vel, ProjectileSettings settings)>
{
    private World _world;
    
    public BasicProjectileProvider(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);
    }
    
    private Entity _defaultCollider;

    protected override void OnInit()
    {
        base.OnInit();
        
        _defaultCollider = _world.CreateEntity();
        _defaultCollider.Set<Shape>(new CircleShape {Radius = 0.1f});
    }

    public override void SetEntityData(ref UEntityHandle handle, (UEntitySafe owner, Vector2 pos, Vector2 vel, ProjectileSettings settings) data)
    {
        var settings = data.settings;
        var collider = _defaultCollider;
        //settings.Collider ??= _defaultCollider;
        
        Simulation.AddProjectileSettings(handle, settings);
        Simulation.AddPositionComponent(handle, new PositionComponent(data.pos));
        Simulation.AddVelocityComponent(handle, new VelocityComponent(data.vel));
        Simulation.AddHitBox(handle, new HitBox(data.owner, 0));
        Simulation.AddHitBoxAgainstTeam(handle, new HitBoxAgainstTeam(Simulation.Safe(Simulation.GetTeamDescriptionRelative(data.owner.Handle))));
        Simulation.AddHitBoxHistory(handle, ReadOnlySpan<HitBoxHistory>.Empty);
        
        Simulation.GetPhysicsEngine()!.AssignCollider(handle, collider);
    }
}

public partial class BasicProjectileSystem : SimulationSystem
{
    public BasicProjectileSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            order => order.AfterGroup<AbilitySystemGroup>().Before(typeof(HitBoxAgainstTeamSystem))
        );
        
        _scheduler = new ConcurrentScheduler();
    }
    
    private ProjectileQuery _projectileQuery;

    private ConcurrentScheduler _scheduler;
    private GameTimeQuery _gameTimeQuery;


    protected override void OnInit()
    {
        _projectileQuery = new ProjectileQuery(Simulation);
        _gameTimeQuery = new GameTimeQuery(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        var dt = (float) _gameTimeQuery.First().GameTime.Delta.TotalSeconds;

        _projectileQuery.QueueAndComplete(
            Runner,
            static (SystemState<(float dt, IScheduler scheduler)> args, ProjectileQuery.EnumeratorSlice entities) =>
            {
                foreach (var ent in entities)
                {
                    ent.vel.Value += ent.projectile.Gravity * args.Data.dt;
                    ent.pos.Value += ent.vel.Value * args.Data.dt;

                    Console.WriteLine("zbouuuué");

                    if (ent.history.Count > 0 || ent.pos.Y <= 0)
                    {
                        Console.WriteLine("hit!");
                        args.Data.scheduler.Add(args =>
                        {
                            Console.WriteLine("pof");
                            args.state.Data.scheduler.Add(args.state.World.DestroyEntity, args.Handle);
                        }, (ent.Handle, state: args));
                    }
                }
            },
            (dt, _scheduler), true
        );
        
        _scheduler.Run();
    }
    
    private partial struct ProjectileQuery : IQuery<(Write<PositionComponent> pos, Write<VelocityComponent> vel, Read<ProjectileSettings> projectile, Read<HitBoxHistory> history)> {}
}