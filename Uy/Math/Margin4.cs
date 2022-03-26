using System.Numerics;

namespace Uy;

/**
<summary>
	<para>
		Represents a 2D 4-sided margin box.
	</para>
</summary>
<remarks>
	<para>
		<u>About Thickness and Offsets</u>
	</para>
	<para>
		<see cref="Margin4"/>'s properties are divided into two categories:
	</para>
	<list type="bullet">
		<item>
			<term>Thickness</term>
			<description>
				<para>
					Setting the value of a <i>thickness</i>
					updates the value of the margin on the corresponding side,
					independently of the other sides.
					Thickness components should be positive <b>positive</b>,
					unless one wants to define negative margins.
				</para>
			</description>
		</item>
		<item>
			<term>Offset</term>
			<description>
				<para>
					<i>Offsets</i> are <b>vectors</b>
					corresponding to the offset defined by the <see cref="Margin4"/>,
					<b>directed</b> from the outside toward the inside.
					E.g. <c><see langword="new"/> <see cref="Margin4"/>(1, 2, 3, 4).<see cref="RightBottomOffset">RightBottomOffset</see> == <see langword="new"/> <see cref="Vector2"/>(-2, -3)</c>.
				</para>
			</description>
		</item>
	</list>
	<para>
		<i>Offset</i>-related properties use the <c>Offset</c> suffix,
		e.g. <see cref="LeftOffset"/> or <see cref="RightBottomOffset"/>,
		whereas <i>thickness</i>-related properties don't use any particular suffix,
		e.g. <see cref="LeftBottom"/> or <see cref="Top"/>.
	</para>
</remarks>
**/
public record struct Margin4 {
	public Margin4(float all) {
		Top = Right = Bottom = Left = all;
	}

	/**
	<summary>
		<para>
			CSS-style <i>top/bottom-left/right</i> order.
		</para>
	</summary>
	**/
	public Margin4(float vertical, float horizontal) {
		Top = Bottom = vertical;
		Right = Left = horizontal;
	}

	/**
	<summary>
		<para>
			CSS-style <i>top-left/right-bottom</i> order.
		</para>
	</summary>
	**/
	public Margin4(float top, float horizontal, float bottom) {
		Top = top;
		Right = Left = horizontal;
		Bottom = bottom;
	}

	/**
	<summary>
		<para>
			CSS-style <i>top-right-bottom-left</i> order.
		</para>
	</summary>
	**/
	public Margin4(float top, float right, float bottom, float left) {
		Top = top;
		Right = right;
		Bottom = bottom;
		Left = left;
	}
	
	public static implicit operator Margin4(float value) => new(value);
	public static implicit operator Margin4((float Vertical, float Horizontal) tuple) => new(tuple.Vertical, tuple.Horizontal);
	public static implicit operator Margin4((float Top, float Horizontal, float Bottom) tuple) => new(tuple.Top, tuple.Horizontal, tuple.Bottom);
	public static implicit operator Margin4((float Top, float Right, float Bottom, float Left) tuple) => new(tuple.Top, tuple.Right, tuple.Bottom, tuple.Left);
	public static implicit operator Margin4(Vector2 horizontalVertical) => new(horizontalVertical.Y, horizontalVertical.X);

	#region Thickness-related properties

	public Vector2 LeftTop {
		readonly get => new(Left, Top);
		set => (Left, Top) = (value.X, value.Y);
	}

	public Vector2 RightTop {
		readonly get => new(Right, Top);
		set => (Right, Top) = (value.X, value.Y);
	}

	public Vector2 RightBottom {
		readonly get => new(Right, Bottom);
		set => (Right, Bottom) = (value.X, value.Y);
	}

	public Vector2 LeftBottom {
		readonly get => new(Left, Bottom);
		set => (Left, Bottom) = (value.X, value.Y);
	}

	public float Top { readonly get; set; }
	public float Right { readonly get; set; }
	public float Bottom { readonly get; set; }
	public float Left { readonly get; set; }

	/**
	<summary>
		<para>
			Returns the sum of the <see cref="Left"/> and <see cref="Right"/> margins.
		</para>
	</summary>
	**/
	public readonly float Horizontal => Left + Right;

	/**
	<summary>
		<para>
			Returns the sum of the <see cref="Top"/> and <see cref="Bottom"/> margins.
		</para>
	</summary>
	**/
	public readonly float Vertical => Top + Bottom;

	#endregion
	#region Offset-related properties

	public readonly Vector2 TopOffset => new(0, Top);
	public readonly Vector2 RightOffset => new(-Right, 0);
	public readonly Vector2 BottomOffset => new(0, -Bottom);
	public readonly Vector2 LeftOffset => new(Left, 0);

	public Vector2 LeftTopOffset {
		readonly get => new(Left, Top);
		set => (Left, Top) = (value.X, value.Y);
	}

	public Vector2 RightTopOffset {
		readonly get => new(-Right, Top);
		set => (Right, Top) = (-value.X, value.Y);
	}

	public Vector2 RightBottomOffset {
		readonly get => new(-Right, -Bottom);
		set => (Right, Bottom) = (-value.X, -value.Y);
	}

	public Vector2 LeftBottomOffset {
		readonly get => new(Left, -Bottom);
		set => (Left, Bottom) = (value.X, -value.Y);
	}

	#endregion
	#region Operators: scaling

	public static Margin4 operator -(Margin4 margin) =>
		new(-margin.Top, -margin.Right, -margin.Bottom, -margin.Left);
	
	public static Margin4 operator +(Margin4 margin, Margin4 other) =>
		new(margin.Top + other.Top, margin.Right + other.Right, margin.Bottom + other.Bottom, margin.Left + other.Left);

	public static Margin4 operator -(Margin4 margin, Margin4 other) =>
		new(margin.Top - other.Top, margin.Right - other.Right, margin.Bottom - other.Bottom, margin.Left - other.Left);

	public static Margin4 operator *(Margin4 margin, float scale) =>
		new(margin.Top * scale, margin.Right * scale, margin.Bottom * scale, margin.Left * scale);

	public static Margin4 operator /(Margin4 margin, float scale) =>
		new(margin.Top / scale, margin.Right / scale, margin.Bottom / scale, margin.Left * scale);

	#endregion
	#region Operators: use with other structures

	public static Box4 operator +(Box4 box, Margin4 margin) =>
		Box4.FromLeftTop(box.Left - margin.Left, box.Top - margin.Top, box.Width + margin.Horizontal, box.Height + margin.Vertical);

	public static Box4 operator -(Box4 box, Margin4 margin) =>
		Box4.FromLeftTop(box.Left + margin.Left, box.Top + margin.Top, box.Width - margin.Horizontal, box.Height - margin.Vertical);

	public static Vector2 operator +(Vector2 vector, Margin4 margin) =>
		new(vector.X + margin.Horizontal, vector.Y + margin.Vertical);

	public static Vector2 operator -(Vector2 vector, Margin4 margin) =>
		new(vector.X - margin.Horizontal, vector.Y - margin.Vertical);

	#endregion
}
