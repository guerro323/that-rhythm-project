using System;
using Quadrum.Game.Utilities;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Units;

public partial struct UnitStatistics : ISparseComponent
{
    public int Health;

    public int   Attack;
    public float AttackSpeed;

    public int Defense;

    public float MovementAttackSpeed;
    public float BaseWalkSpeed;
    public float FeverWalkSpeed;

    /// <summary>
    ///     Weight can be used to calculate unit acceleration for moving or for knock-back power amplification.
    /// </summary>
    public float Weight;

    public float KnockbackPower;
    public float Precision;

    public float AttackMeleeRange;
    public float AttackSeekRange;
}