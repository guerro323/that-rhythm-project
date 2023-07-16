using System;
using DefaultEcs;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using Quadrum.Game.Modules.Simulation.Common.Transform;
using Quadrum.Game.Modules.Simulation.Interaction.Health.Components;
using revecs;
using revghost;

namespace Quadrum.Game.Modules.Simulation.Teams.Systems;

public partial class UpdateTeamMovableAreaSystem : SimulationSystem
{
    public UpdateTeamMovableAreaSystem(Scope scope) : base(scope)
    {
        SubscribeTo<ISimulationUpdateLoopSubscriber>(
            OnUpdate
        );
    }

    private TeamQuery _teamQuery;
    private ContributeQuery _contributeQuery;

    protected override void OnInit()
    {
        _teamQuery = new TeamQuery(Simulation);
        _contributeQuery = new ContributeQuery(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        foreach (var entity in _teamQuery)
        {
            entity.area = new TeamMovableArea
            {
                Left = float.PositiveInfinity,
                Right = float.NegativeInfinity
            };
        }

        foreach (var entity in _contributeQuery)
        {
            if (!Simulation.HasComponent(entity.team, SimulationAuthority.ToComponentType(Simulation)))
                continue;

            ref var teamArea = ref Simulation.GetTeamMovableArea(entity.team);
            if (!teamArea.IsValid)
            {
                teamArea.IsValid = true;
                teamArea.Left = entity.pos.Value.X - entity.area.Size - entity.area.Center;
                teamArea.Right = entity.pos.Value.X + entity.area.Size + entity.area.Center;
                
                continue;
            }
            
            teamArea.Left  = MathF.Min(entity.pos.Value.X - entity.area.Size - entity.area.Center, teamArea.Left);
            teamArea.Right = MathF.Max(entity.pos.Value.X + entity.area.Size + entity.area.Center, teamArea.Right);
        }
    }

    public partial struct TeamQuery : IQuery<(Write<TeamMovableArea> area, All<TeamDescription>, All<SimulationAuthority>)> {}
    public partial struct ContributeQuery : IQuery<(Read<ContributeToTeamMovableArea> area, Read<TeamDescription.Relative> team, Read<PositionComponent> pos, None<LivableIsDead>)> {}
}