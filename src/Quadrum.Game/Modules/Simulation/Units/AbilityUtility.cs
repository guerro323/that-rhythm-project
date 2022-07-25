using System;
using System.Numerics;
using Quadrum.Game.Utilities;

namespace Quadrum.Game.Modules.Simulation.Units;

public static class AbilityUtility
{
    public static float GetTargetVelocityX(GetTargetVelocityParameters param, 
        float deaccelDistance = -1,
        float deaccelDistanceMax = -1)
    {
        var speed = MathUtils.LerpNormalized(MathF.Abs(param.PreviousVelocity.X),
            param.PlayState.MovementAttackSpeed,
            param.PlayState.GetAcceleration() * param.Acceleration * param.Delta);

        if (deaccelDistance >= 0)
        {
            var dist = MathUtils.Distance(param.TargetPosition.X, param.PreviousPosition.X);
            if (dist > deaccelDistance && dist < deaccelDistanceMax)
            {
                speed *= MathUtils.UnlerpNormalized(deaccelDistance, deaccelDistanceMax, dist);
                speed = Math.Max(speed, param.Delta);
            }
        }

        var newPosX = MathUtils.MoveTowards(param.PreviousPosition.X, param.TargetPosition.X, speed * param.Delta);

        return (newPosX - param.PreviousPosition.X) / param.Delta;
    }

    public struct GetTargetVelocityParameters
    {
        public Vector2 TargetPosition;
        public Vector2 PreviousPosition;
        public Vector2 PreviousVelocity;
        public UnitPlayState PlayState;
        public float Acceleration;
        public float Delta;
    }
}