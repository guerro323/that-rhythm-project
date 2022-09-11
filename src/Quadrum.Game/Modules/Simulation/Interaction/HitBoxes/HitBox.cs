using System.Numerics;
using revecs.Core;
using revecs.Extensions.Buffers;
using revecs.Extensions.Generator.Components;
using revecs.Extensions.RelativeEntity.Generator;

namespace Quadrum.Game.Modules.Simulation.Interaction.HitBoxes;

public partial record struct HitBox(
    // The source of this hit box
    UEntitySafe Instigator,
    
    // How much hits that this hit box can register (see HitBoxHistory buffer component)
    int MaxHits,
    // Whether or not velocity will be multiplied by the delta time
    bool VelocityUseDelta = true
) : ISparseComponent;

public partial record struct HitBoxOwnerDescription : IDescriptionComponent;

public partial struct HitBoxHistory : IBufferComponent
{
    public HitBoxHistory(UEntitySafe Victim,
        Vector2 Position,
        Vector2 Normal,
        float Distance)
    {
        this.Victim = Victim;
        this.Position = Position;
        this.Normal = Normal;
        this.Distance = Distance;
    }

    public UEntitySafe Victim;
    public Vector2 Position;
    public Vector2 Normal;
    public float Distance;
}

public partial record struct HitBoxAgainstTeam(UEntitySafe TeamTarget) : ISparseComponent;

