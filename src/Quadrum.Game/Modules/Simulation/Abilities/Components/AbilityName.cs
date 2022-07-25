using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

public partial struct AbilityName : ISparseComponent
{
    public readonly string Value;

    public AbilityName(string value) => Value = value;

    public static implicit operator string(AbilityName v) => v.Value;
    public static implicit operator AbilityName(string v) => new(v);
}