using revghost;
using revghost.Module;

namespace Quadrum.Game.Modules.Simulation.Interaction;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
    }

    protected override void OnInit()
    {
        LoadModule(sc => new Health.Module(sc));
        LoadModule(sc => new HitBoxes.Module(sc));
        LoadModule(sc => new Damage.Module(sc));
    }
}