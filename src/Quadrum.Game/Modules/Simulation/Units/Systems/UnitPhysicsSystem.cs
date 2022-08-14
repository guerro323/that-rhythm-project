using System;
using System.Numerics;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Cursors;
using Quadrum.Game.Utilities;
using revecs;
using revghost;
using revghost.Domains.Time;
using revghost.Loop.EventSubscriber;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Units.Systems;

public partial class UnitPhysicsSystem : SimulationSystem
{
    public UnitPhysicsSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(OnUpdate);
    }

    private GameTimeQuery _timeQuery;
    private UnitQuery _query;
    private Commands _cmd;

    protected override void OnInit()
    {
        Disposables.AddRange(new IDisposable[]
        {
            (_query = new UnitQuery(Simulation)).Query
        });

        _timeQuery = new GameTimeQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity entity)
    {
        var delta = (float) _timeQuery.First().GameTime.Delta.TotalSeconds;
        
        _query.QueueAndComplete(Runner, static (state, entities) =>
        {
            var gravity = new Vector2(0, -26f);
            
            var (dt, cmd) = state.Data;
            foreach (var unit in entities)
            {
                if (unit.vel.Y > 0)
                    unit.groundState.Value = false;

                var previousPosition = unit.pos.Value;
                // If the entity override its target position we take it.
                // Else if the entity has a cursor relative we take its position, alongside with its offset
                var target = unit.controller.OverrideTargetPosition || !cmd.HasCursorDescriptionRelative(unit.Handle)
                    ? unit.controller.TargetPosition
                    : cmd.ReadPositionComponent(cmd.ReadCursorDescriptionRelative(unit.Handle)).Value.X
                      + (cmd.HasCursorOffset(unit.Handle)
                          ? cmd.ReadCursorOffset(unit.Handle).Idle
                          : 0f);

                if (!unit.controller.ControlOverVelocityX)
                {
                    if (unit.groundState.Value)
                    {
                        var ps = unit.playState with
                        {
                            MovementAttackSpeed = unit.playState.MovementReturnSpeed
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
                        unit.vel.Y += gravity.Y * dt;

                foreach (ref var axe in unit.vel.Value.AsSpan())
                {
                    axe = float.IsNaN(axe) ? 0 : axe;
                }

                //Console.WriteLine($"{unit.vel.Value:F3} {unit.vel.Value * dt:F3}");
                unit.pos.Value += unit.vel.Value * dt;
                // todo: proper floor support? (use collision objects?)
                unit.pos.Value.Y = Math.Max(0, unit.pos.Value.Y);
                
                unit.groundState.Value = unit.pos.Value.Y <= 0;

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
        }, (delta, _cmd));
    }

    public partial record struct UnitQuery : IQuery<(
        Write<UnitControllerState> controller,
        Write<PositionComponent> pos,
        Write<VelocityComponent> vel,
        Write<GroundState> groundState,
        Read<UnitPlayState> playState)>;

    public partial record struct Commands :
        CursorDescription.Relative.Cmd.IRead,
        PositionComponent.Cmd.IRead,
        CursorOffset.Cmd.IRead;
}