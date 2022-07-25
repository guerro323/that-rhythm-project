using Quadrum.Game.Utilities;
using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.Abilities;

public static class AbilitySystemObjectExtensions
{
    public static void AbilityConditionConstraint(this SystemObject systemObject)
    {
        systemObject.SetGroup<AbilityConditionSystemGroup>();
    }
    
    public static void AbilityConstraint(this SystemObject systemObject)
    {
        systemObject.SetGroup<AbilityExecutionSystemGroup>();
    }
}