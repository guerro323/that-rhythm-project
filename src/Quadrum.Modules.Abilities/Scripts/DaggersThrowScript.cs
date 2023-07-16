using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Common.Physics;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.Damage;
using Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;
using Quadrum.Game.Modules.Simulation.Teams;
using Quadrum.Game.Modules.Simulation.Units;
using QuadrumPrototype.Code.Abilities;
using revecs.Core;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Shared.Threading.Schedulers;

namespace Quadrum.Modules.Abilities.Scripts;

public class DaggersThrowScript : AttackAbilityScriptBase<DaggersThrowAbility>
{
    private BasicProjectileProvider _projectileProvider;
    
    public DaggersThrowScript(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _projectileProvider);
    }

    protected override void OnSetup(ReadOnlySpan<UEntityHandle> abilities)
    {
        base.OnSetup(abilities);
        foreach (var handle in abilities)
        {
            Simulation.RemoveHitBox(handle);
        }
    }

    protected override void OnExecute(UEntityHandle owner, UEntityHandle self,
        ref AttackAbilityState attackState,
        in AttackAbilitySettings attackSettings)
    {
        attackState.Cooldown -= GameTime.Delta;
        
        Simulation.GetHitBoxHistory(self).Clear();
        
        if (!HasActiveOrChainingState(self))
        {
            StopAttack(ref attackState);
            return;
        }
        
        ref var controlVelocity = ref Simulation.GetAbilityControlVelocity(self);
        
        ref readonly var position = ref Simulation.GetPositionComponent(owner).Value;
        ref readonly var playState = ref Simulation.GetUnitPlayState(owner);

        ref var velocity = ref Simulation.GetVelocityComponent(owner).Value;

        var meleeRange = 0.6f;

        if (IsAttackingAndUpdate(ref attackState, attackSettings, GameTime.Total))
        {
            // velocity.X *= (float) GameTime.Delta.TotalSeconds;
            
            if (CanAttackThisFrame(ref attackState, attackSettings, GameTime.Total, TimeSpan.FromSeconds(0.25f)))
            {
                velocity.X = 0;
                
                Console.WriteLine("throw!");
                var pos = position + new Vector2(Simulation.GetUnitDirection(owner).Value * 0.6f, 1f);
                PostScheduler.Add(_ =>
                {
                    _projectileProvider.SpawnEntity((
                        Simulation.Safe(owner), pos,
                        new Vector2(Simulation.GetUnitDirection(owner).Value * 15, 0),
                        new ProjectileSettings { }
                    ));
                }, 0);
                /*PostScheduler.Add(_ =>
                {
                    using var collider = World.CreateEntity();
                    collider.Set<Shape>(new PolygonShape(meleeRange + 0.2f, meleeRange * 0.5f));
                    
                    Simulation.GetPhysicsEngine()!.AssignCollider(self, collider);
                    
                    Simulation.AddHitBox(self, new HitBox(Simulation.Safe(owner), 0));
                    Console.WriteLine("SLAAAASH");
                }, 0);

                Simulation.GetPositionComponent(self).Value = position + new Vector2(0, 0.5f);
                Simulation.GetDamageFrameData(self) = new DamageFrameData(playState);
                Simulation.GetHitBoxAgainstTeam(self) = new HitBoxAgainstTeam(Simulation.Safe(Simulation.GetTeamDescriptionRelative(owner)));*/
            }
        }

        var (enemyPrioritySelf, dist) = GetNearestEnemy(owner, 2, null);
        if (HasActiveState(self) && enemyPrioritySelf.Handle.Id != default)
        {
            var targetPosition = Simulation.GetPositionComponent(enemyPrioritySelf.Handle).Value;
            /*controlVelocity.IsActive = true;
            controlVelocity.CustomMovementSpeed = 0.5f;
            controlVelocity.HasCustomMovementSpeed = true;*/

            if (MathF.Abs(targetPosition.X - position.X) < 10)
            {
                TriggerAttack(ref attackState, GameTime);
            }

            controlVelocity.OffsetFactor = 0f;
        }
    }
}