using revecs.Extensions.Generator.Components;
using revecs.Extensions.RelativeEntity.Generator;

namespace Quadrum.Game.Modules.Simulation.Interaction.Health.Components;

public partial record struct LivableHealth(int Value, int Max) : ISparseComponent;
public partial record struct LivableIsDead : ITagComponent;

public partial record struct LivableDescription : IDescriptionComponent;