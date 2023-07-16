using System;
using Quadrum.Game.Utilities;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Teams;

public partial struct TeamMovableArea : ISparseComponent
{
    public float Left;
    public float Right;
    
    public bool IsValid;

    public float Center => MathUtils.LerpNormalized(Left, Right, 0.5f);
    public float Size   => MathF.Abs(Left - Right) * 0.5f;
}

public partial struct ContributeToTeamMovableArea : ISparseComponent
{
    public readonly float Center;
    public readonly float Size;

    public ContributeToTeamMovableArea(float center, float size)
    {
        Center = center;
        Size   = size;
    }
}