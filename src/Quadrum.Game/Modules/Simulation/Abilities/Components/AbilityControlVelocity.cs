using System.Numerics;
using revecs.Extensions.Generator.Components;

namespace Quadrum.Game.Modules.Simulation.Abilities.Components;

public partial struct AbilityControlVelocity : ISparseComponent
{
    public bool IsActive;
    public bool ActiveInAir;

    public bool KeepX;
    public bool KeepY;
    public bool ControlX;
    public bool ControlY;

    public bool TargetFromCursor;
    public Vector2 TargetPosition;
    public float OffsetFactor;
    public float Acceleration;

    public bool HasCustomMovementSpeed;
    public float CustomMovementSpeed;

    public void ResetPositionX(float acceleration, float offsetFactor = 1)
    {
        SetCursorPositionX(0, acceleration, offsetFactor);
    }

    public void SetCursorPositionX(float position, float acceleration, float offsetFactor = 1)
    {
        KeepX = false;
        ControlX = false;

        IsActive = true;
        TargetFromCursor = true;
        TargetPosition.X = position;
        OffsetFactor = offsetFactor;
        Acceleration = acceleration;
    }

    public void SetAbsolutePositionX(float position, float acceleration, float offsetFactor = 1)
    {
        KeepX = false;
        ControlX = false;

        IsActive = true;
        TargetFromCursor = false;
        TargetPosition.X = position;
        OffsetFactor = offsetFactor;
        Acceleration = acceleration;
    }

    public void StayAtCurrentPositionX(float acceleration)
    {
        ControlX = default;

        IsActive = true;
        KeepX = true;
        Acceleration = acceleration;
    }
}