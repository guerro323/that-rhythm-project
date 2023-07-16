using System;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Teams.Systems;
using revghost;
using revghost.Module;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Teams;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        TrackDomain((SimulationDomain domain) =>
        {
            Disposables.AddRange(new IDisposable[]
            {
                new UpdateTeamMovableAreaSystem(domain.Scope)
            });
        });
    }
}