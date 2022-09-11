using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health;

public partial record struct LivableHealth(int Value, int Max) : ISparseComponent;
public partial record struct LivableIsDead : ITagComponent;