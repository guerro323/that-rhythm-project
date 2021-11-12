using revghost;
using revghost.Module;

namespace Quadrum.Game;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        LoadModule(scope => new Modules.Simulation.Module(scope));
    }
}