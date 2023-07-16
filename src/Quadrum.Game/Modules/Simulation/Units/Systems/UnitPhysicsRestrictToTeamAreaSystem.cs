using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.Health.Components;
using Quadrum.Game.Modules.Simulation.Teams;
using Quadrum.Game.Utilities;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Units.Systems;

public partial class UnitPhysicsRestrictToTeamAreaSystem : SimulationSystem
{
    private UnitQuery _query;

    public UnitPhysicsRestrictToTeamAreaSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate,
            order => order.After(typeof(UnitPhysicsSystem))
        );
    }

    protected override void OnInit()
    {
        _query = new UnitQuery(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        foreach (var entity in _query)
        {
            var previousTranslation = entity.pos.Value;
            if (!entity.controller.PassThroughEnemies && Simulation.HasTeamHostileDescription(entity.team))
            {
                var enemies = Simulation.GetTeamHostileDescription(entity.team);
                for (var i = 0; i != enemies.Length; i++)
                {
                    if (!Simulation.HasTeamMovableArea(enemies[i]))
                        continue;

                    ref readonly var enemyArea = ref Simulation.GetTeamMovableArea(enemies[i]);

                    // If the new position is superior the area and the previous one inferior, teleport back to the area.
                    var size = entity.contribute.Size * 0.5f + entity.contribute.Center;
                    if (entity.pos.Value.X + size > enemyArea.Left && entity.direction.Value > 0)
                        entity.pos.Value.X = enemyArea.Left - size;

                    if (entity.pos.Value.X - size < enemyArea.Right && entity.direction.Value < 0)
                        entity.pos.Value.X = enemyArea.Right + size;

                    // if it's inside...
                    if (entity.pos.Value.X + size > enemyArea.Left && entity.pos.Value.X - size < enemyArea.Right)
                    {
                        if (entity.direction.Value < 0)
                            entity.pos.Value.X = enemyArea.Right + size;
                        else if (entity.direction.Value > 0)
                            entity.pos.Value.X = enemyArea.Left - size;
                    }
                }
            }

            entity.pos.Value.Y = previousTranslation.Y;

            if (Simulation.HasTeamMovableArea(entity.team)
                && Simulation.HasComponent(entity.team, SimulationAuthority.ToComponentType(Simulation)))
            {
                ref var teamArea = ref Simulation.GetTeamMovableArea(entity.team);

                teamArea.Left = Math.Min(entity.pos.Value.X - entity.contribute.Size - entity.contribute.Center, teamArea.Left);
                teamArea.Right = Math.Max(entity.pos.Value.X + entity.contribute.Size + entity.contribute.Center, teamArea.Right);
            }

            for (var v = 0; v != 2; v++)
                entity.pos.Value.Ref(v) = float.IsNaN(entity.pos.Value.Ref(v)) ? 0.0f : entity.pos.Value.Ref(v);
        }
    }

    private partial struct UnitQuery : IQuery<(
        Read<UnitControllerState> controller,
        Read<UnitDirection> direction,
        Write<PositionComponent> pos,
        Read<ContributeToTeamMovableArea> contribute,
        Read<TeamDescription.Relative> team,
        None<LivableIsDead>,
        All<SimulationAuthority>)> {}
}