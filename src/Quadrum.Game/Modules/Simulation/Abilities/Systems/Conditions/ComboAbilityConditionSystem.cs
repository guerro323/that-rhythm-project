using revecs.Systems.Generator;

namespace Quadrum.Game.Modules.Simulation.Abilities.Systems.Conditions;

public partial struct ComboAbilityConditionSystem : IRevolutionSystem
{
    public void Constraints(in SystemObject sys)
    {
        sys.AbilityConditionConstraint();
    }

    public void Body()
    {
        
    }
}