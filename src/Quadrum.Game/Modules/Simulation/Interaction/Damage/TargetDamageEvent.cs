using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Interaction.Damage;

public partial record struct TargetDamageEvent(
    UEntitySafe Instigator,
    UEntitySafe Victim, 
    double Damage
) : ISparseComponent;