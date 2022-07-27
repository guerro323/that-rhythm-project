using Quadrum.Export.Godot;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Module;

namespace Quadrum.Export.Desktop;

public class EntryModule : HostModule
{
    public EntryModule(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        LoadModule(scope => new Quadrum.Game.Module(scope));
        
        TrackDomain((SimulationDomain domain) =>
        {
            //_ = new TestSoundSystem(ModuleScope.DataStorage, domain.Scope);
        });
    }
}