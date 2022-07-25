using System;
using Quadrum.Game.Utilities;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Units;

public partial struct UnitPlayState : ISparseComponent
{
    public int Attack;
    public int Defense;

    public float ReceiveDamagePercentage;

    public float MovementSpeed;
    public float MovementAttackSpeed;
    public float MovementReturnSpeed;
    public float AttackSpeed;

    public float AttackSeekRange;

    public float Weight;
    public float KnockbackPower;
    public float Precision;

    public readonly float GetAcceleration()
    {
        return Math.Clamp(MathUtils.Rcp(Weight), 0, 1);
    }
}