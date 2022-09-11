using System.Numerics;
using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;

public partial record struct HitBoxEvent(
    UEntitySafe HitBox,
    UEntitySafe Instigator,
    UEntitySafe Victim,
    
    Vector2 ContactPosition,
    Vector2 ContactNormal,
    float ContactDistance
) : ISparseComponent;