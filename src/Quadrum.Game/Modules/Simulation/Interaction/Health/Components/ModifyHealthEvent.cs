using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health.Components;

public enum HealthModifier
{
    Fixed,
    Add,
    Max,
    None
}

public partial struct ModifyHealthEvent : ISparseComponent
{
    public HealthModifier Modifier;
    public int Origin;
    public int Consumed;

    public UEntitySafe Target;

    public ModifyHealthEvent(HealthModifier modifier, int origin, UEntitySafe target)
    {
        Modifier = modifier;

        Origin = origin;
        Consumed = origin;

        Target = target;
    }
}