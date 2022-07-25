using Quadrum.Game.Utilities;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.Abilities;

public partial class AbilitySystemGroup : ISystemGroup
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
            sys.DependOn<Begin>();
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