using System;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;
using revghost.Utility;

namespace Quadrum.Game.Modules.Simulation.Interaction.Damage;

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
                new GenerateDamageRequestSystem(domain.Scope)
            });
        });
    }
}