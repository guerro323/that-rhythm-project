using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health.Components;

public partial struct ConcreteHealthValue : ISparseComponent
{
    public int Value;
    public int Max;

    public ConcreteHealthValue(int value, int max)
    {
        Value = value;
        Max = max;
    }
}