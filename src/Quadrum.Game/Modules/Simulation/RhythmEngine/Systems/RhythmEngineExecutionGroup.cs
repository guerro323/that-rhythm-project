using revecs;
using revecs.Systems;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public static partial class RhythmEngineExecutionGroup
{
    public partial struct Begin : IRevolutionSystem
    {
        public void Constraints(in SystemObject sys)
        {
        }

        public void Body()
        {
        }
    }
    
    public partial struct End : IRevolutionSystem
    {
        public void Constraints(in SystemObject sys)
        {
        }

        public void Body()
        {
        }
    }
}