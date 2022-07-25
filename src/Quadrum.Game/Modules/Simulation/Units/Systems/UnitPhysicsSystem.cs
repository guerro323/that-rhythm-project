using System;
using System.Numerics;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Utilities;
using revecs.Extensions.Generator.Commands;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.Units.Systems;

public partial struct UnitPhysicsSystem : IRevolutionSystem,
    CursorDescription.Relative.Cmd.IRead,
    PositionComponent.Cmd.IRead,
    CursorOffset.Cmd.IRead
{
    public void Constraints(in SystemObject sys)
    { 
    }

    public void Body()
    {
        var dt = (float) RequiredResource<GameTime>().Delta.TotalSeconds;
        var gravity = new Vector2(0, -26f);

        foreach (var unit in RequiredQuery(
                     Write<UnitControllerState>("controller"),
                     Write<PositionComponent>("pos"),
                     Write<VelocityComponent>("vel"),
                     Write<GroundState>("groundState"),
                     Read<UnitPlayState>("playState")))
        {
            if (unit.vel.Y > 0)
                unit.groundState.Value = false;

            var previousPosition = unit.pos.Value;
            var target = unit.controller.OverrideTargetPosition || Cmd.HasCursorDescriptionRelative(unit.Handle)
                ? unit.controller.TargetPosition
                : Cmd.ReadPositionComponent(Cmd.ReadCursorDescriptionRelative(unit.Handle)).Value.X
                  + (Cmd.HasCursorOffset(unit.Handle)
                      ? Cmd.ReadCursorOffset(unit.Handle).Value
                      : 0f);

            if (!unit.controller.ControlOverVelocityX)
            {
                if (unit.groundState.Value)
                {
                    var ps = unit.playState with
                    {
                        MovementSpeed = unit.playState.MovementReturnSpeed
                    };

                    unit.vel.X = AbilityUtility.GetTargetVelocityX(new AbilityUtility.GetTargetVelocityParameters
                    {
                        TargetPosition = new Vector2(target, 0),
                        PreviousPosition = unit.pos.Value,
                        PreviousVelocity = unit.vel.Value,
                        PlayState = ps,
                        Acceleration = 10,
                        Delta = dt
                    }, deaccelDistance: 0, deaccelDistanceMax: 0.25f);
                }
                else
                {
                    var accel = Math.Clamp(MathUtils.Rcp(unit.playState.Weight), 0, 1) * 5;
                    accel = Math.Min(accel * dt, 1) * 0.75f;

                    unit.vel.X = MathUtils.LerpNormalized(unit.vel.X, 0, accel);
                }
            }

            if (!unit.controller.ControlOverVelocityY)
                if (!unit.groundState.Value)
                    unit.vel.Value += gravity * dt;

            foreach (ref var axe in unit.vel.Value.AsSpan())
            {
                axe = float.IsNaN(axe) ? 0 : axe;
            }

            unit.pos.Value += unit.vel.Value * dt;
            // todo: proper floor support? (use collision objects?)
            unit.pos.Value.Y = Math.Max(0, unit.pos.Value.Y);

            if (!unit.controller.ControlOverVelocityY && unit.groundState.Value)
                unit.vel.Y = Math.Max(0, unit.vel.Y);

            foreach (ref var axe in unit.pos.Value.AsSpan())
            {
                axe = float.IsNaN(axe) ? 0 : axe;
            }

            unit.controller.ControlOverVelocity = default;
            unit.controller.OverrideTargetPosition = false;
            unit.controller.PassThroughEnemies = false; // TODO: set it to true once we add LivableIsDead
            unit.controller.PreviousPosition = previousPosition;
        }
    }
}