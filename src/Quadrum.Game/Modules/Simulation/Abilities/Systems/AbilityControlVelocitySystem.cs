using System;
using System.Numerics;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Modules.Simulation.Units;
using Quadrum.Game.Utilities;
using revecs.Querying;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems;

public partial class AbilityControlVelocitySystem : SimulationSystem
{
    public AbilityControlVelocitySystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            order => order
                .SetGroup<AbilityExecutionSystemGroup>()
                .After(typeof(ExecuteAbilitySystem))
        );
    }

    private GameTimeQuery _gameTimeQuery;
    private ArchetypeQuery _query;
    
    protected override void OnInit()
    {
        _gameTimeQuery = new GameTimeQuery(Simulation);
        _query = new ArchetypeQuery(Simulation, stackalloc[]
        {
            AbilityControlVelocity.ToComponentType(Simulation),
            AbilityOwnerDescription.Relative.ToComponentType(Simulation)
        });
    }

    private void OnUpdate(Entity _)
    {
        var gameTime = _gameTimeQuery.First().GameTime;
        
        var targetAccessor = Simulation.AccessSparseSet(AbilityControlVelocity.Type.GetOrCreate(Simulation));
        var ownerAccessor  = Simulation.AccessEntityComponent(AbilityOwnerDescription.Relative.Type.GetOrCreate(Simulation));

        var positionAccessor = Simulation.AccessSparseSet(PositionComponent.Type.GetOrCreate(Simulation));
        var playStateAccessor = Simulation.AccessSparseSet(UnitPlayState.Type.GetOrCreate(Simulation));
        var velocityAccessor = Simulation.AccessSparseSet(VelocityComponent.Type.GetOrCreate(Simulation));
        var controllerAccessor = Simulation.AccessSparseSet(UnitControllerState.Type.GetOrCreate(Simulation));
        
        var cursorAccessor      = Simulation.AccessEntityComponent(CursorDescription.Relative.Type.GetOrCreate(Simulation));

        var dt = (float) gameTime.Delta.TotalSeconds;
        
        foreach (var entity in _query)
        {
            ref var target = ref targetAccessor[entity];
            if (!target.IsActive)
            {
                continue;
            }

            target.IsActive    = false;

            var owner = ownerAccessor.FirstOrThrow(entity);
            if (Simulation.GetGroundState(owner).Value == false && !target.ActiveInAir)
            {
                continue;
            }

            target.ActiveInAir = false;

            ref var position   = ref positionAccessor[owner].Value;
            ref var playState  = ref playStateAccessor[owner];
            ref var velocity   = ref velocityAccessor[owner].Value;
            ref var controller = ref controllerAccessor[owner];

            if (target.HasCustomMovementSpeed)
                playState.MovementAttackSpeed = target.CustomMovementSpeed;

            if (target.KeepX)
            {
                if (target.Acceleration.Equals(float.PositiveInfinity))
                    velocity.X = 0;
                else if (target.Acceleration > 0)
                    velocity.X = MathUtils.LerpNormalized(velocity.X, 0, target.Acceleration * dt);
            }
            else
            {
                var targetPosition = target.TargetPosition;
                if (target.TargetFromCursor && Simulation.HasCursorDescriptionRelative(owner))
                {
                    var cursor = cursorAccessor.FirstOrThrow(owner);
                    if (Simulation.HasPositionComponent(cursor))
                        targetPosition += Simulation.GetPositionComponent(cursor).Value;
                }

                if (MathF.Abs(target.OffsetFactor) > 0.01f && Simulation.HasCursorOffset(owner))
                {
                    targetPosition.X += Simulation.GetCursorOffset(owner).Idle * target.OffsetFactor;
                }

                var prev = velocity.X;
                velocity.X = AbilityUtility.GetTargetVelocityX(new AbilityUtility.GetTargetVelocityParameters
                {
                    TargetPosition   = targetPosition,
                    PreviousPosition = new Vector2(position.X, 0),
                    PreviousVelocity = velocity,
                    PlayState        = playState,
                    Acceleration     = target.Acceleration,
                    Delta            = dt
                }, 0.1f, 0.25f);
            }

            controller.ControlOverVelocityX = true;
        }
    }
}