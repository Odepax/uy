using LinqToYourDoom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Uy;

/**
<summary>
	<para>
		Represents a 2D radian angle.
	</para>
</summary>
<remarks>
	<para>
		<see cref="AngleF"/>s are automatically <i>modulo-clamped</i> in <c>]-PI, +PI]</c>.
	</para>
</remarks>
**/
[DebuggerDisplay("{Value}rad")]
public readonly record struct AngleF : IEquatable<AngleF> {
	readonly float P;
	AngleF(float p) => P = Clamp(p);

	const byte UNCHECKED = default;
	AngleF(float p, byte _) => P = p;

	/**
	<summary>
		<para>
			<i>Modulo-clamps</i> the <paramref name="value"/> in <c>]-PI, +PI]</c>.
		</para>
	</summary>
	**/
	public static float Clamp(float value) =>
		// Adapted from:
		// https://stackoverflow.com/a/22949941
		// https://stackoverflow.com/questions/2320986/easy-way-to-keeping-angles-between-179-and-180-degrees#answer-22949941
		value - MathF.Ceiling((value - MathF.PI) / MathF.Tau) * MathF.Tau;

	bool PrintMembers(StringBuilder builder) {
		builder.Append($"{ nameof(PiValue) } = { PiValue }, ");
		builder.Append($"{ nameof(TauValue) } = { TauValue }");

		return true;
	}

	#region Factories

	public static readonly AngleF Zero = default;

	public static AngleF FromDegrees(float value) => new(value * MathF.Tau / 360f);
	public static AngleF FromRadians(float value) => new(value);
	public static AngleF FromTurns(float value) => new(value * MathF.Tau);
	public static AngleF FromAsin(float value) => new(MathF.Asin(value), UNCHECKED);
	public static AngleF FromAcos(float value) => new(MathF.Acos(value), UNCHECKED);
	public static AngleF FromAtan(float value) => new(MathF.Atan(value), UNCHECKED);
	public static AngleF FromAsinh(float value) => new(MathF.Asinh(value), UNCHECKED);
	public static AngleF FromAcosh(float value) => new(MathF.Acosh(value), UNCHECKED);
	public static AngleF FromAtanh(float value) => new(MathF.Atanh(value), UNCHECKED);
	public static AngleF FromDirection(Vector2 vector) => new(MathF.Atan2(vector.Y, vector.X), UNCHECKED);

	public static implicit operator AngleF(float radians) => new(radians);
	public static implicit operator float(AngleF angle) => angle.PiValue;
	
	public static implicit operator AngleF(Vector2 vector) => new(vector.Direction(), UNCHECKED);
	public static implicit operator Vector2(AngleF angle) => angle.Vector;

	#endregion
	#region Math

	public readonly float PiValue => P;
	public readonly float TauValue => P < 0 ? P + MathF.Tau : P;
	public readonly float Sin => MathF.Sin(P);
	public readonly float Sinh => MathF.Sinh(P);
	public readonly float Cos => MathF.Cos(P);
	public readonly float Cosh => MathF.Cosh(P);
	public readonly float Tan => MathF.Tan(P);
	public readonly float Tanh => MathF.Tanh(P);
	public readonly Vector2 Vector => new(MathF.Cos(P), MathF.Sin(P));
	public readonly AngleF Opposite => new(P + MathF.PI);

	public static AngleF operator +(AngleF a, float rotation) => new(a.P + rotation);
	public static AngleF operator -(AngleF a, float rotation) => new(a.P - rotation);
	public static AngleF operator *(AngleF a, float scale) => new(a.P * scale);
	public static AngleF operator *(float scale, AngleF a) => new(scale * a.P);
	public static AngleF operator /(AngleF a, float scale) => new(a.P / scale);

	public readonly float ShortArcTo(AngleF target) => Clamp(target.P - P);

	public readonly float LongArcTo(AngleF target) {
		var r = Clamp(target.P - P);

		return r < 0 ? r + MathF.Tau: r - MathF.Tau;
	}

	public readonly float ClockwiseArcTo(AngleF target) {
		var a = target.P - P;

		return a < 0 ? a + MathF.Tau : a;
	}

	public readonly float CounterClockwiseArcTo(AngleF target) {
		var a = target.P - P;

		return a < 0 ? a : a - MathF.Tau;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly float ShortArcFrom(AngleF @base) => @base.ShortArcTo(this);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly float LongArcFrom(AngleF @base) => @base.LongArcTo(this);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly float ClockwiseArcFrom(AngleF @base) => @base.ClockwiseArcTo(this);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly float CounterClockwiseArcFrom(AngleF @base) => @base.CounterClockwiseArcTo(this);

	public readonly AngleF ShortArcLerp(AngleF other, float ratio) => new(P + ShortArcTo(other) * ratio);
	public readonly AngleF LongArcLerp(AngleF other, float ratio) => new(P + LongArcTo(other) * ratio);
	public readonly AngleF ClockwiseArcLerp(AngleF other, float ratio) => new(P + ClockwiseArcTo(other) * ratio);
	public readonly AngleF CounterClockwiseArcLerp(AngleF other, float ratio) => new(P + CounterClockwiseArcTo(other) * ratio);

	public readonly AngleF CoerceIn(AngleF counterClockwiseBound, AngleF clockwiseBound) =>
		(counterClockwiseBound.CounterClockwiseArcTo(this) / counterClockwiseBound.CounterClockwiseArcTo(clockwiseBound)) switch {
			< 0.5f => counterClockwiseBound,
			< 1f => clockwiseBound,
			_ => this
		};

	#endregion
	#region Equality

	/**
	<param name="delta">
		<para>
			The maximum acceptable difference, <b>in radians</b>, between <see langword="this"/> and <paramref name="other"/>.
			If greater than <c>PI</c>, <see cref="Equals(AngleF, float)"/> will always return <see langword="true"/>.
			If negative <see cref="Equals(AngleF, float)"/> will always return <see langword="false"/>.
		</para>
	</param>
	**/
	public readonly bool Equals(AngleF other, float delta) => P == other.P || ShortArcTo(other).Abs() <= delta;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool DoesNotEqual(AngleF other, float delta) => !Equals(other, delta);

	/**
	<summary>
		<para>
			Compares <see cref="AngleF"/>s based on their raw value in the <c>]-PI, PI]</c> domain.
		</para>
	</summary>
	**/
	public static readonly IComparer<AngleF> AbsoluteComparer = new AbsoluteAngleFComparer();

	class AbsoluteAngleFComparer : IComparer<AngleF> {
		public int Compare(AngleF x, AngleF y) => x.P.CompareTo(y.P);
	}

	/**
	<summary>
		<para>
			Compares <see cref="AngleF"/>s based on their relative position.
			It can be understood as a relative <i>left of-, right of-</i> comparison.
		</para>
	</summary>
	**/
	public static readonly IComparer<AngleF> RelativeComparer = new RelativeAngleFComparer();

	class RelativeAngleFComparer : IComparer<AngleF> {
		public int Compare(AngleF x, AngleF y) {
			var shortArc = x.ShortArcTo(y);

			return shortArc == MathF.PI ? 3 : (int) MathF.Ceiling(-shortArc);
		}
	}

	#endregion
}
