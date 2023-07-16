using Quadrum.Game.Modules.Simulation.Abilities.Components.Aspects;
using Quadrum.Game.Modules.Simulation.Abilities.SystemBase;
using QuadrumPrototype.Code.Abilities;
using revecs.Core;
using revghost;

namespace Quadrum.Modules.Abilities.Scripts;

public class DefaultMarchScript : AbilityScript<DefaultMarchAbility>
{
    public DefaultMarchScript(Scope scope) : base(scope)
    {
    }

    protected override void OnSetup(ReadOnlySpan<UEntityHandle> abilities)
    {
    }

    protected override void OnExecute(UEntityHandle owner, UEntityHandle self)
    {
        Simulation.GetMarchAbilityAspect(self).IsActive = HasActiveState(self);
    }
}