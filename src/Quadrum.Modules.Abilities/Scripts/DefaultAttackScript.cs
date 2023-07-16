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
using revghost.Shared.Threading.Schedulers;

namespace Quadrum.Modules.Abilities.Scripts;

public class DefaultAttackScript : AttackAbilityScriptBase<DefaultAttackAbility>
{
    public DefaultAttackScript(Scope scope) : base(scope)
    {
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

        var meleeRange = 0.6f;

        if (IsAttackingAndUpdate(ref attackState, attackSettings, GameTime.Total))
        {
            controlVelocity.StayAtCurrentPositionX(10);
            
            if (CanAttackThisFrame(ref attackState, attackSettings, GameTime.Total, TimeSpan.FromSeconds(0.25f)))
            {
                Console.WriteLine("slash!");
                PostScheduler.Add(_ =>
                {
                    using var collider = World.CreateEntity();
                    collider.Set<Shape>(new PolygonShape(meleeRange + 0.2f, meleeRange * 0.5f));
                    
                    Simulation.GetPhysicsEngine()!.AssignCollider(self, collider);
                    
                    Simulation.AddHitBox(self, new HitBox(Simulation.Safe(owner), 0));
                    Console.WriteLine("SLAAAASH");
                }, 0);

                Simulation.GetPositionComponent(self).Value = position + new Vector2(0, 0.5f);
                Simulation.GetDamageFrameData(self) = new DamageFrameData(playState);
                Simulation.GetHitBoxAgainstTeam(self) = new HitBoxAgainstTeam(Simulation.Safe(Simulation.GetTeamDescriptionRelative(owner)));
            }
        }
        else if (HasChainingState(self))
        {
            controlVelocity.StayAtCurrentPositionX(50);
        }

        var (enemyPrioritySelf, dist) = GetNearestEnemy(owner, 2, null);
        if (HasActiveState(self) && enemyPrioritySelf.Handle.Id != default)
        {
            var targetPosition = Simulation.GetPositionComponent(enemyPrioritySelf.Handle).Value;
            if (attackState.AttackStart == default)
            {
                controlVelocity.SetAbsolutePositionX(targetPosition.X, 50);
            }

            var distanceMercy = meleeRange + 0.5f;
            if (MathF.Abs(targetPosition.X - position.X) < distanceMercy)
            {
                TriggerAttack(ref attackState, GameTime);
            }

            controlVelocity.OffsetFactor = 0f;
        }
    }
}