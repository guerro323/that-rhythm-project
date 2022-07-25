using System;
using DefaultEcs;
using revghost;
using revghost.Module;
using revghost.Utility;
using Xunit;
using Xunit.Abstractions;

namespace Quadrum.Export.Godot.Tests;

public class HelloWorldTest : HostTest
{
    public override GhostRunner CreateRunner()
    {
        return GhostInit.Launch(
            scope => {},
            scope => new Module(scope)
        );
    }

    public class Module : HostModule
    {
        private World _world;
        
        public Module(HostRunnerScope scope) : base(scope)
        {
            _world = scope.World;
        }

        protected override void OnInit()
        {
            HostLogger.Output.Info("Hello World!");
            _world.Set<ExitApplication>();
        }
    }

    public HelloWorldTest(ITestOutputHelper testOutput) : base(testOutput)
    {
    }
}