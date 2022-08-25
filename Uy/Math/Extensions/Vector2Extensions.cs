using LinqToYourDoom;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Uy;

public static class Vector2Extensions {
	#region Conversion extensions

	public static void Deconstruct(this Vector2 @this, out float x, out float y) {
		x = @this.X;
		y = @this.Y;
	}

	#endregion
	#region From Vector2's static methods

	/**
	<summary>
		A size has negative components?
		<see cref="NormalizeSize(ref Vector2)"/> turns your <see cref="Vector2"/> back up right,
		resetting <see cref="Vector2.X"/> and <see cref="Vector2.Y"/>
		to their respective <see cref="MathF.Abs(float)">absolutes</see> if needs be.
	</summary>
	<remarks>
		<para>
			In-place extension method equivalent of <see cref="Vector2.Abs(Vector2)"/>.
		</para>
	</remarks>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NormalizeSize(this ref Vector2 @this) => @this = Vector2.Abs(@this);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.Abs(Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Abs(this Vector2 @this) => Vector2.Abs(@this);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.Negate(Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Negate(this ref Vector2 @this) => @this = Vector2.Negate(@this);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.Negate(Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Negated(this Vector2 @this) => Vector2.Negate(@this);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.Normalize(Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Normalize(this ref Vector2 @this) => @this = Vector2.Normalize(@this);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.Normalize(Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Normalized(this Vector2 @this) => Vector2.Normalize(@this);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.Reflect(Vector2, Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Reflect(this ref Vector2 @this, Vector2 normal) => @this = Vector2.Reflect(@this, normal);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.Reflect(Vector2, Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Reflected(this Vector2 @this, Vector2 normal) => Vector2.Reflect(@this, normal);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.SquareRoot(Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SquareRoot(this ref Vector2 @this) => @this = Vector2.SquareRoot(@this);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.SquareRoot(Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 SquareRooted(this Vector2 @this) => Vector2.SquareRoot(@this);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.Transform(Vector2, Quaternion)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Transform(this ref Vector2 @this, Quaternion rotation) => @this = Vector2.Transform(@this, rotation);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.Transform(Vector2, Quaternion)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transformed(this Vector2 @this, Quaternion rotation) => Vector2.Transform(@this, rotation);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.Transform(Vector2, Matrix3x2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Transform(this ref Vector2 @this, Matrix3x2 matrix) => @this = Vector2.Transform(@this, matrix);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.Transform(Vector2, Matrix3x2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transformed(this Vector2 @this, Matrix3x2 matrix) => Vector2.Transform(@this, matrix);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.Transform(Vector2, Matrix4x4)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Transform(this ref Vector2 @this, Matrix4x4 matrix) => @this = Vector2.Transform(@this, matrix);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.Transform(Vector2, Matrix4x4)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transformed(this Vector2 @this, Matrix4x4 matrix) => Vector2.Transform(@this, matrix);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.TransformNormal(Vector2, Matrix3x2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void TransformNormal(this ref Vector2 @this, Matrix3x2 matrix) => @this = Vector2.TransformNormal(@this, matrix);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.TransformNormal(Vector2, Matrix3x2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 WithNormalTransformed(this ref Vector2 @this, Matrix3x2 matrix) => Vector2.TransformNormal(@this, matrix);

	/**
	<summary>
		<para>
			In-place extension method equivalent of <see cref="Vector2.TransformNormal(Vector2, Matrix4x4)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void TransformNormal(this ref Vector2 @this, Matrix4x4 matrix) => @this = Vector2.TransformNormal(@this, matrix);

	/**
	<summary>
		<para>
			Immutable extension method equivalent of <see cref="Vector2.TransformNormal(Vector2, Matrix4x4)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 WithNormalTransformed(this ref Vector2 @this, Matrix4x4 matrix) => Vector2.TransformNormal(@this, matrix);

	/**
	<summary>
		<para>
			Extension method equivalent of <see cref="Vector2.Distance(Vector2, Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceTo(this Vector2 @this, Vector2 target) => Vector2.Distance(@this, target);

	/**
	<summary>
		<para>
			Extension method equivalent of <see cref="Vector2.DistanceSquared(Vector2, Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquaredTo(this Vector2 @this, Vector2 target) => Vector2.DistanceSquared(@this, target);

	/**
	<summary>
		<para>
			Extension method equivalent of <see cref="Vector2.Dot(Vector2, Vector2)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dot(this Vector2 @this, Vector2 other) => Vector2.Dot(@this, other);

	/**
	<summary>
		<para>
			Extension method equivalent of <see cref="Vector2.Lerp(Vector2, Vector2, float)"/>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Lerp(this Vector2 @this, Vector2 other, float ratio) => Vector2.Lerp(@this, other, ratio);

	/**
	<remarks>
		<para>
			<i>"What's the difference between <c>.<see cref="Lerp(Vector2, Vector2, float)">Lerp</see>()</c>
			and <c>.<see cref="LerpTo(Vector2, Vector2, float)">LerpTo</see>()</c>?"</i>, you will ask...
		</para>
		<para>
			Well: <c>.<see cref="Lerp(Vector2, Vector2, float)">Lerp</see>()</c>
			returns an interpolated <see cref="Vector2"/> corresponding to a point in between
			<paramref name="this"/> and <paramref name="other"/>, with coordinates expressed
			relatively from the origin, i.e. <see cref="Vector2.Zero"/>,
			whereas <c>.<see cref="LerpTo(Vector2, Vector2, float)">LerpTo</see>()</c>
			returns a <see cref="Vector2"/> corresponding to an arrow pointing from <paramref name="this"/>
			in the direction of <paramref name="other"/>.
		</para>
		<code><![CDATA[
		,------------------------. ,------------------------.
		|  this   .Lerp   other  | |  this  .LerpTo  other  |
		|    (+)   (*)   (+)     | |    (+)---->    (+)     |
		|      \    |    /       | |      \         /       |
		|       \   |   /        | |       \       /        |
		|        \  |  /         | |        \     /         |
		|         \ | /          | |         \   /          |
		|          \|/           | |          \ /           |
		|          (0) Origin    | |          (0) Origin    |
		`------------------------' `------------------------'
		]]></code>
	</remarks>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 LerpTo(this Vector2 @this, Vector2 other, float ratio) => Vector2.Lerp(Vector2.Zero, other - @this, ratio);

	#endregion
	#region Extra properties, most of them from Spaceshits

	public static void SetLength(this ref Vector2 @this, float value) {
		var l = @this.Length();

		if (l == 0)
			@this.X = value; // Length = 0 => Y = 0

		else {
			@this.X *= value / l;
			@this.Y *= value / l;
		}
	}
	public static Vector2 WithLength(this Vector2 @this, float value) {
		@this.SetLength(value); // @this is already a copy, therefore safe to mutate.

		return @this;
	}

	/**
	<summary>
		<para>
			Normalized direction, i.e. directed angle from <see cref="Vector2.UnitX"/>.
		</para>
	</summary>
	**/
	public static AngleF Direction(this Vector2 @this) => AngleF.FromDirection(@this);
	public static void SetDirection(this ref Vector2 @this, float value) {
		var l = @this.Length();

		@this.X = value.Cos() * l;
		@this.Y = value.Sin() * l;
	}
	public static Vector2 WithDirection(this Vector2 @this, float value) {
		@this.SetDirection(value); // @this is already a copy, therefore safe to mutate.

		return @this;
	}

	/**
	<returns>
		<para>
			<c>(-y, x)</c>.
		</para>
	</returns>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Tangent(this Vector2 @this) => new(-@this.Y, @this.X);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 VectorTo(this Vector2 @this, Vector2 target) => target - @this;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 VectorFrom(this Vector2 @this, Vector2 origin) => @this - origin;

	/**
	<returns>
		<para>
			The <c>[-<see cref="MathF.PI">PI</see>, +<see cref="MathF.PI">PI</see>]</c> angle,
			i.e. the direction, of the arrow pointing toward <paramref name="target"/> from <paramref name="this"/>.
		</para>
	</returns>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static AngleF DirectionTo(this Vector2 @this, Vector2 target) => AngleF.FromDirection(target - @this);

	/**
	<returns>
		<para>
			The <c>[-<see cref="MathF.PI">PI</see>, +<see cref="MathF.PI">PI</see>]</c> angle,
			i.e. the direction, of the arrow pointing toward <paramref name="this"/> from <paramref name="origin"/>.
		</para>
	</returns>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static AngleF DirectionFrom(this Vector2 @this, Vector2 origin) => AngleF.FromDirection(@this - origin);

	#endregion
	#region Methods, some from Spaceshits, others from Box4

	public static bool Equals(this Vector2 @this, Vector2 other, float delta) =>
		   @this.X.Equals(other.X, delta)
		&& @this.Y.Equals(other.Y, delta);

	public static void OffsetBy(this ref Vector2 @this, float dxy) => @this.OffsetBy(new Vector2(dxy));
	public static void OffsetBy(this ref Vector2 @this, float dx, float dy) => @this.OffsetBy(new Vector2(dx, dy));
	public static void OffsetBy(this ref Vector2 @this, Vector2 offset) => @this += offset;

	public static Vector2 WithOffset(this Vector2 @this, float dxy) => @this.WithOffset(new Vector2(dxy));
	public static Vector2 WithOffset(this Vector2 @this, float dx, float dy) => @this.WithOffset(new Vector2(dx, dy));
	public static Vector2 WithOffset(this Vector2 @this, Vector2 offset) => @this + offset;

	public static void AngularOffsetBy(this ref Vector2 @this, float direction, float length) {
		@this.X += direction.Cos() * length;
		@this.Y += direction.Sin() * length;
	}

	public static Vector2 WithAngularOffset(this Vector2 @this, float direction, float length) => new(
		@this.X + direction.Cos() * length,
		@this.Y + direction.Sin() * length
	);

	public static void RelativeOffsetBy(this ref Vector2 @this, float dxy) => @this.RelativeOffsetBy(new Vector2(dxy));
	public static void RelativeOffsetBy(this ref Vector2 @this, float dx, float dy) => @this.RelativeOffsetBy(new Vector2(dx, dy));
	public static void RelativeOffsetBy(this ref Vector2 @this, Vector2 offset) {
		var d = @this.Direction();

		@this.X += d.Cos * offset.X - d.Sin * offset.Y;
		@this.Y += d.Sin * offset.X + d.Cos * offset.Y;
	}

	public static Vector2 WithRelativeOffset(this Vector2 @this, float dxy) => @this.WithRelativeOffset(new Vector2(dxy));
	public static Vector2 WithRelativeOffset(this Vector2 @this, float dx, float dy) => @this.WithRelativeOffset(new Vector2(dx, dy));
	public static Vector2 WithRelativeOffset(this Vector2 @this, Vector2 offset) {
		var d = @this.Direction();

		return new(
			@this.X + d.Cos * offset.X - d.Sin * offset.Y,
			@this.Y + d.Sin * offset.X + d.Cos * offset.Y
		);
	}

	public static void RelativeAngularOffsetBy(this ref Vector2 @this, float direction, float length) {
		var d = @this.Direction();

		@this.X += (d.Cos + direction) * length;
		@this.Y += (d.Sin + direction) * length;
	}

	public static Vector2 WithRelativeAngularOffset(this Vector2 @this, float direction, float length) {
		var d = @this.Direction();

		return new(
			@this.X + (d.Cos + direction) * length,
			@this.Y + (d.Sin + direction) * length
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void RotateBy(this ref Vector2 @this, float angle) => @this.SetDirection(@this.Direction() + angle);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 Rotated(this Vector2 @this, float angle) => @this.WithDirection(@this.Direction() + angle);

	public static void CoerceAtLeast(this ref Vector2 @this, Vector2 min) => @this.CoerceAtLeast(min.X, min.Y);
	public static void CoerceAtLeast(this ref Vector2 @this, float minX, float minY) {
		@this.X = @this.X.CoerceAtLeast(minX);
		@this.Y = @this.Y.CoerceAtLeast(minY);
	}

	public static Vector2 CoercedAtLeast(this Vector2 @this, Vector2 min) => @this.CoercedAtLeast(min.X, min.Y);
	public static Vector2 CoercedAtLeast(this Vector2 @this, float minX, float minY) => new(
		@this.X.CoerceAtLeast(minX),
		@this.Y.CoerceAtLeast(minY)
	);

	public static void CoerceAtMost(this ref Vector2 @this, Vector2 max) => @this.CoerceAtMost(max.X, max.Y);
	public static void CoerceAtMost(this ref Vector2 @this, float maxX, float maxY) {
		@this.X = @this.X.CoerceAtMost(maxX);
		@this.Y = @this.Y.CoerceAtMost(maxY);
	}

	public static Vector2 CoercedAtMost(this Vector2 @this, Vector2 max) => @this.CoercedAtMost(max.X, max.Y);
	public static Vector2 CoercedAtMost(this Vector2 @this, float maxX, float maxY) => new(
		@this.X.CoerceAtMost(maxX),
		@this.Y.CoerceAtMost(maxY)
	);

	public static void InflateBy(this ref Vector2 @this, Margin4 margin) => @this.InflateBy(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public static void InflateBy(this ref Vector2 @this, float all) => @this.InflateBy(all, all, all, all);
	public static void InflateBy(this ref Vector2 @this, float vertical, float horizontal) => @this.InflateBy(vertical, horizontal, vertical, horizontal);
	public static void InflateBy(this ref Vector2 @this, float top, float horizontal, float bottom) => @this.InflateBy(top, horizontal, bottom, horizontal);
	public static void InflateBy(this ref Vector2 @this, float top, float right, float bottom, float left) {
		@this.X += left + right;
		@this.Y += top + bottom;
	}

	public static Vector2 Inflated(this Vector2 @this, Margin4 margin) => @this.Inflated(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public static Vector2 Inflated(this Vector2 @this, float all) => @this.Inflated(all, all, all, all);
	public static Vector2 Inflated(this Vector2 @this, float vertical, float horizontal) => @this.Inflated(vertical, horizontal, vertical, horizontal);
	public static Vector2 Inflated(this Vector2 @this, float top, float horizontal, float bottom) => @this.Inflated(top, horizontal, bottom, horizontal);
	public static Vector2 Inflated(this Vector2 @this, float top, float right, float bottom, float left) => new(
		@this.X + left + right,
		@this.Y + top + bottom
	);

	public static void DeflateBy(this ref Vector2 @this, Margin4 margin) => @this.DeflateBy(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public static void DeflateBy(this ref Vector2 @this, float all) => @this.DeflateBy(all, all, all, all);
	public static void DeflateBy(this ref Vector2 @this, float vertical, float horizontal) => @this.DeflateBy(vertical, horizontal, vertical, horizontal);
	public static void DeflateBy(this ref Vector2 @this, float top, float horizontal, float bottom) => @this.DeflateBy(top, horizontal, bottom, horizontal);
	public static void DeflateBy(this ref Vector2 @this, float top, float right, float bottom, float left) {
		@this.X -= left + right;
		@this.Y -= top + bottom;
	}

	public static Vector2 Deflated(this Vector2 @this, Margin4 margin) => @this.Deflated(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public static Vector2 Deflated(this Vector2 @this, float all) => @this.Deflated(all, all, all, all);
	public static Vector2 Deflated(this Vector2 @this, float vertical, float horizontal) => @this.Deflated(vertical, horizontal, vertical, horizontal);
	public static Vector2 Deflated(this Vector2 @this, float top, float horizontal, float bottom) => @this.Deflated(top, horizontal, bottom, horizontal);
	public static Vector2 Deflated(this Vector2 @this, float top, float right, float bottom, float left) => new(
		@this.X + left + right,
		@this.Y + top + bottom
	);

	public static void AxisAlign(this ref Vector2 @this) {
		if (@this.X.Abs() < @this.Y.Abs())
			@this.X = 0;

		else @this.Y = 0;
	}
	
	public static Vector2 AxisAligned(this Vector2 @this) {
		@this.AxisAlign(); // @this is a copy, it's safe to modify it in place here.

		return @this;
	}

	#endregion
}
