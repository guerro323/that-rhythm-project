using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Quadrum.Game.Utilities;

public static class MathUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float LerpNormalized(float a, float b, float t)
	{
		return a + Math.Clamp(t, 0, 1) * (b - a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double LerpNormalized(double a, double b, float t)
	{
		return a + Math.Clamp(t, 0, 1) * (b - a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Rcp(float a)
	{
		return MathF.ReciprocalEstimate(a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DivSafe(float x, float y)
	{
		if (x == 0 || y == 0)
			return 0;
		return x / y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int DivSafe(int x, int y)
	{
		if (x == 0 || y == 0)
			return 0;
		return x / y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref float Ref(this ref Vector3 vec3, int i)
	{
		switch (i)
		{
			case 0:
				return ref vec3.X;
			case 1:
				return ref vec3.Y;
			case 2:
				return ref vec3.Z;
			default:
				throw new IndexOutOfRangeException();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref float Ref(this ref Vector2 vec2, int i)
	{
		switch (i)
		{
			case 0:
				return ref vec2.X;
			case 1:
				return ref vec2.Y;
			default:
				throw new IndexOutOfRangeException();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 XY(this Vector3 vec3) => new Vector2(vec3.X, vec3.Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Sign(float value)
	{
		if (value.Equals(float.NaN))
			return 0;

		return Math.Sign(value);
	}

	public static Vector3 NormalizeSafe(Vector3 vector)
	{
		var normalized = Vector3.Normalize(vector);
		if (normalized.Length().Equals(float.NaN))
			return Vector3.Zero;
		return normalized;
	}

	public static Vector2 NormalizeSafe(Vector2 vector)
	{
		var normalized = Vector2.Normalize(vector);
		if (normalized.Length().Equals(float.NaN))
			return Vector2.Zero;
		return normalized;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float MoveTowards(float current, float target, float maxDelta)
	{
		if (Math.Abs(target - current) <= maxDelta)
			return target;
		return current + Sign(target - current) * maxDelta;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDelta)
	{
		return new(
			MoveTowards(current.X, target.X, maxDelta),
			MoveTowards(current.Y, target.Y, maxDelta),
			MoveTowards(current.Z, target.Z, maxDelta)
		);
	}

	public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDelta)
	{
		return new(
			MoveTowards(current.X, target.X, maxDelta),
			MoveTowards(current.Y, target.Y, maxDelta)
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(float a, float b) => Math.Abs(a - b);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float UnlerpNormalized(float a, float b, float t) =>
		a.Equals(b) ? 0.0f : Math.Clamp((t - a) / (b - a), 0, 1);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Unlerp(float a, float b, float t) => a.Equals(b) ? 0.0f : (t - a) / (b - a);

	public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
	{
		var sqrmag = vector.LengthSquared();
		if (sqrmag > maxLength * maxLength)
		{
			var mag = (float) Math.Sqrt(sqrmag);
			var normalized = vector / mag;
			return normalized * maxLength;
		}

		return vector;
	}

	public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
	{
		var sqrmag = vector.LengthSquared();
		if (sqrmag > maxLength * maxLength)
		{
			var mag = (float) Math.Sqrt(sqrmag);
			var normalized = vector / mag;
			return normalized * maxLength;
		}

		return vector;
	}

	public static Span<float> AsSpan(this ref Vector2 vec)
	{
		return MemoryMarshal.CreateSpan(ref vec.X, 2);
	}
}