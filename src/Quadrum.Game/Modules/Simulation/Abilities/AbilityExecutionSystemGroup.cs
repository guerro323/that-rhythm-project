using Quadrum.Game.Utilities;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.Abilities;

// Update in AbilitySystemGroup
public partial class AbilityExecutionSystemGroup : ISystemGroup
{
    public partial struct Begin : IRevolutionSystem
    {
        public void Constraints(in SystemObject sys)
        {
            sys.DependOn<AbilitySystemGroup.Begin>();
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
            sys.AddForeignDependency<AbilitySystemGroup.End>();
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