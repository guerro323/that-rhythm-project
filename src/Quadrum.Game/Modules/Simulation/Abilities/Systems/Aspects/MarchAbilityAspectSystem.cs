using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Aspects;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Modules.Simulation.Units;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

using static Quadrum.Game.Utilities.MathUtils;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems.Aspects;

public partial class MarchAbilityAspectSystem : SimulationSystem
{
    public MarchAbilityAspectSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            b => b.SetGroup<AbilityExecutionSystemGroup>()
                .After(typeof(ExecuteAbilitySystem))
        );
    }

    private GameTimeQuery _timeQuery;
    private AbilityQuery _abilityQuery;
    private Commands _cmd;

    protected override void OnInit()
    {
        _timeQuery = new GameTimeQuery(Simulation);
        _abilityQuery = new AbilityQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        var time = _timeQuery.First().GameTime;
        
        _abilityQuery.QueueAndComplete(Runner, static (state, entities) =>
        {
            var delta = (float) state.Data.Delta.TotalSeconds;
            var cmd = state.Data._cmd;
            foreach (var entity in entities)
            {
                ref var aspect = ref entity.aspect;
                if (!aspect.IsActive)
                {
                    aspect.ActiveTime = default;
                    continue;
                }

                aspect.ActiveTime += delta;

                var owner = entity.owner;
                var cursorRelative = cmd.ReadCursorDescriptionRelative(owner);
                var unitPlayState = cmd.ReadUnitPlayState(owner);

                ref var cursorPosition = ref cmd.UpdatePositionComponent(cursorRelative).X;
                ref var ownerPosition = ref cmd.UpdatePositionComponent(owner).X;

                var cursorOffset = cmd.ReadCursorOffset(owner).Idle;
                
                float acceleration, walkSpeed;
                int   direction;

                // Cursor movement
                if ((aspect.Target & MarchAbilityAspect.ETarget.Cursor) != 0
                    && cmd.HasCursorControlTag(owner)
                    && aspect.ActiveTime <= 3.75f)
                {
                    direction = cmd.ReadUnitDirection(owner).Value;

                    // a different acceleration (not using the unit weight)
                    acceleration = aspect.AccelerationFactor;
                    acceleration = Math.Min(acceleration * delta, 1);

                    walkSpeed      =  unitPlayState.MovementSpeed;
                    cursorPosition += walkSpeed * direction * (aspect.ActiveTime > 0.25f ? 1 : LerpNormalized(2, 1, aspect.ActiveTime + 0.25f)) * acceleration;
                }

                // Character movement
                if ((aspect.Target & MarchAbilityAspect.ETarget.Movement) != 0)
                {
                    ref var velocity = ref cmd.UpdateVelocityComponent(owner);

                    // to not make tanks op, we need to get the weight from entity and use it as an acceleration factor
                    // We need to get the abs of the AccelerationFactor since the backward ability use -1
                    acceleration = Math.Clamp(Rcp(unitPlayState.Weight), 0, 1) * Math.Abs(aspect.AccelerationFactor) * 50;
                    acceleration = Math.Min(acceleration * delta, 1);

                    walkSpeed = unitPlayState.MovementSpeed;
                    direction = Math.Sign(cursorPosition + cursorOffset - ownerPosition);

                    velocity.X = LerpNormalized(velocity.X, walkSpeed * direction, acceleration);
                    
                    ref var controllerState = ref cmd.UpdateUnitControllerState(owner);
                    controllerState.ControlOverVelocityX = true;
                }
            }
        }, (time.Delta, _cmd));
    }

    private partial record struct AbilityQuery : IQuery<(
        Read<AbilityOwnerDescription.Relative> owner,
        Write<MarchAbilityAspect> aspect)>;

    private partial record struct Commands :
        PositionComponent.Cmd.IWrite,
        VelocityComponent.Cmd.IWrite,
        UnitControllerState.Cmd.IWrite,
        UnitPlayState.Cmd.IRead,
        UnitDirection.Cmd.IRead,

        CursorDescription.Relative.Cmd.IRead,
        CursorOffset.Cmd.IRead,
        CursorControlTag.Cmd.IRead;
}