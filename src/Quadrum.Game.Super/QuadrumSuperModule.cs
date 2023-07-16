using revghost;
using revghost.Module;

namespace Quadrum.Game.Super;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        LoadModule(scope => new Quadrum.Game.Module(scope));
        LoadModule(scope => new Quadrum.Game.Super.Modules.Abilities.Module(scope));
    }
}