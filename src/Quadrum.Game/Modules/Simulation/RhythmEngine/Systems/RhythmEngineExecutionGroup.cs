using Quadrum.Game.Utilities;
using revecs;
using revecs.Systems;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Systems;

public partial class RhythmEngineExecutionGroup : ISystemGroup
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

    public static void AddToGroup(SystemObject systemObject)
    {
        systemObject.DependOn<Begin>();
        systemObject.AddForeignDependency<End>();
    }
}