using Quadrum.Game.Modules.Simulation.Application;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Modules.Abilities;

public partial struct AttackAbilityState : ISparseComponent
{
    // Can only attack if Cooldown is passed, and if there are no delay before next attack
    public TimeSpan AttackStart;

    // prev: HasThrown, HasSlashed, ...
    public bool DidAttack;

    /// <summary>
    /// Cooldown before waiting for the next attack
    /// </summary>
    public TimeSpan Cooldown;
}

public partial struct AttackAbilitySettings : ISparseComponent
{
    /// <summary>
    /// Delay before the attack (does not include <see cref="Cooldown"/>)
    /// </summary>
    public TimeSpan DelayBeforeAttack;

    /// <summary>
    /// Delay after the attack (does not include <see cref="Cooldown"/>)
    /// </summary>
    public TimeSpan PauseAfterAttack;
}