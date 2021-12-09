using System.Threading;
using GameHost.Audio;
using Quadrum.Game.Modules.Client.Audio.Server;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Domains;
using revghost.Module;

namespace Quadrum.Game.Modules.Client.Audio.Implementation.SoLoud;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    private Soloud soloud;

    protected override void OnInit()
    {
        soloud = new Soloud();
        soloud.init();
        soloud.setGlobalVolume(1f);
        
        TrackDomain((SimulationDomain domain) =>
        {
            var sysScope = new FreeScope
            (
                new MultipleScopeContext
                {
                    ModuleScope.Context,
                    domain.Scope.Context
                }
            );

            _ = new SoloudServerSystem(soloud, sysScope);
        });
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        soloud.deinit();
        soloud = null;
    }
}