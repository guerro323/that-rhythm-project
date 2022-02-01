using Quadrum.Game.BGM;
using revghost;
using revghost.Module;

namespace Quadrum.Game;

public class Module : HostModule
{
    public Module(HostRunnerScope scope) : base(scope)
    {
        scope.Context.Register(new BgmContainerStorage());
    }

    protected override void OnInit()
    {
        LoadModule(scope => new Modules.Simulation.Module(scope));
        LoadModule(scope => new Modules.Simulation.RhythmEngine.Module(scope));
        LoadModule(scope => new Modules.Client.Audio.Module(scope));
        LoadModule(scope => new Modules.Client.Audio.Implementation.SoLoud.Module(scope));
    }
} 