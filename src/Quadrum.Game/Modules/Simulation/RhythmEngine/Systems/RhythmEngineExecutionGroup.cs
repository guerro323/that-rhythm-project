using revecs;
using revecs.Systems;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public static partial class RhythmEngineExecutionGroup
{
    public partial struct Begin : ISystem
    {
        [RevolutionSystem]
        private static void Method() { }
    }
    
    public partial struct End : ISystem
    {
        [RevolutionSystem, DependOn(typeof(Begin), true)]
        private static void Method() { }
    }
}