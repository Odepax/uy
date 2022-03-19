using LinqToYourDoom;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Uy;

/**
<summary>
	<para>
		Represents a 2D rectangular bounding area, without knowledge of rotation.
	</para>
	<para>
		The <see cref="Box4"/> is mutable by default.
		Immutability is also supported via the <see langword="with"/> expression.
		Instance methods that mutate their receiver, i.e. <see langword="this"/>,
		return <see cref="void"/> and are named after active verbs,
		e.g. <see cref="Normalize"/>.
		Instance methods that return an updated copy,
		return <see cref="Box4"/> and are named after passive states,
		e.g. <see cref="Normalized"/>.
	</para>
</summary>
<remarks>
	<para>
		To instanciate a <see cref="Box4"/>,
		one should rely on the static factory methods,
		instead of using the object initializer syntax,
		as the order in which the size-related properties
		and position-related properties are set may matter.
	</para>
	<para>
		<u>About Anchors and Sides</u>
	</para>
	<para>
		<see cref="Box4"/>'s properties related to coordinates,
		e.g. <see cref="Left"/>, <see cref="RightBottom"/>, <see cref="TopAnchor"/>,
		are divided into two categories:
	</para>
	<list type="bullet">
		<item>
			<term>Anchor</term>
			<description>
				<para>
					Setting the coordinates of an <i>anchor</i>
					repositions the entire <see cref="Box4"/>,
					so that the anchor point coincides with the new coordinates,
					and leaving <see cref="Width"/> and <see cref="Height"/> untouched.
				</para>
			</description>
		</item>
		<item>
			<term>Side</term>
			<description>
				<para>
					Setting the coordinate of a <i>side</i>
					moves it independently of other sides,
					stretching the <see cref="Box4"/> in the process,
					i.e. updating <see cref="Width"/> and <see cref="Height"/>
					Think: <i>"set side = set size"</i>.
				</para>
			</description>
		</item>
	</list>
	<para>
		<i>Anchor</i>-related properties use the <c>Anchor</c> suffix,
		e.g. <see cref="LeftBottomAnchor"/> or <see cref="CenterYAnchor"/>,
		whereas <i>side</i>-related properties don't use any particular suffix,
		e.g. <see cref="RightBottom"/> or <see cref="Top"/>.
	</para>
	<para>
		<i>Lerp anchors</i> return values corresponding to a <i>ratio</i>
		proportional to the <see cref="Size"/> from the <see cref="LeftTopAnchor"/>.
		E.g. <c><see cref="Box4"/>.<see cref="FromLeftTop(float, float, float, float)">FromLeftTop</see>(1, 7, 4, 15).<see cref="LerpAnchor">LerpAnchor</see>(<see langword="new"/>(0.5f, 0.2f))</c>
		would return <c><see langword="new"/> <see cref="Vector2"/>(1 + 2, 7 + 3)</c>.
	</para>
	<para>
		Setting <i>lerp anchors</i> repositions <see langword="this"/> <see cref="Box4"/>
		so that <c><see cref="LerpAnchor(Vector2)">LerpAnchor</see>(ratio)</c>
		coincides with the new value,
		while leaving <see cref="Width"/> and <see cref="Height"/> untouched.
		E.g. <c><see cref="Box4"/>.<see cref="FromLeftTop(float, float, float, float)">FromLeftTop</see>(1, 7, 4, 15).<see cref="WithLerpAnchor">WithLerpAnchor</see>(<see langword="new"/>(0.5f, 0.2f), <see cref="Vector2.Zero"/>)</c>
		would return <c><see cref="Box4"/>.<see cref="FromLeftTop(float, float, float, float)">FromLeftTop</see>(0 - 2, 0 - 3, 4, 15)</c>.
	</para>
	<para>
		<u>About Sizes</u>
	</para>
	<para>
		<see cref="Box4"/>'s properties related to size,
		e.g. <see cref="Width"/>, <see cref="Size"/>, <see cref="TopAnchoredHeight"/>,
		are <i>anchored</i>: when setting the size, the anchor point doesn't move
		while the <see cref="Box4"/> scales around it.
	</para>
	<para>
		Size components are <b>positive</b>,
		unless <c><see cref="Right"/> &lt; <see cref="Left"/></c>
		or <c><see cref="Bottom"/> &lt; <see cref="Top"/></c>,
		in which case one may want to turn the <see cref="Box4"/> back upright
		by <see cref="Normalize">normalizing</see> it.
	</para>
</remarks>
**/
public record struct Box4 {
	float L;
	float T;
	float W;
	float H;

	Box4(float l, float t, float w, float h) {
		L = l;
		T = t;
		W = w;
		H = h;
	}

	#region Static factory methods

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 EmptyAtOrigin() => EmptyAt(0, 0);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 EmptyAt(Vector2 centerAnchor) => EmptyAt(centerAnchor.X, centerAnchor.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 EmptyAt(float centerAnchorX, float centerAnchorY) => new(centerAnchorX, centerAnchorY, 0, 0);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLeftTop(Vector2 leftTopAnchor, Vector2 size) => FromLeftTop(leftTopAnchor.X, leftTopAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLeftTop(float leftAnchor, float topAnchor, float width, float height) => new(leftAnchor, topAnchor, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromRightTop(Vector2 rightTopAnchor, Vector2 size) => FromRightTop(rightTopAnchor.X, rightTopAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromRightTop(float rightAnchor, float topAnchor, float width, float height) => new(rightAnchor - width, topAnchor, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromRightBottom(Vector2 rightBottomAnchor, Vector2 size) => FromRightBottom(rightBottomAnchor.X, rightBottomAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromRightBottom(float rightAnchor, float bottomAnchor, float width, float height) => new(rightAnchor - width, bottomAnchor - height, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLeftBottom(Vector2 leftBottomAnchor, Vector2 size) => FromLeftBottom(leftBottomAnchor.X, leftBottomAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLeftBottom(float leftAnchor, float bottomAnchor, float width, float height) => new(leftAnchor, bottomAnchor - height, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromCenter(Vector2 centerAnchor, Vector2 size) => FromCenter(centerAnchor.X, centerAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromCenter(float centerAnchorX, float centerAnchorY, float width, float height) => new(centerAnchorX - width / 2f, centerAnchorY - height / 2f, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLeftCenter(Vector2 leftCenterAnchor, Vector2 size) => FromLeftCenter(leftCenterAnchor.X, leftCenterAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLeftCenter(float leftAnchor, float centerAnchorY, float width, float height) => new(leftAnchor, centerAnchorY - height / 2f, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromRightCenter(Vector2 rightCenterAnchor, Vector2 size) => FromRightCenter(rightCenterAnchor.X, rightCenterAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromRightCenter(float rightAnchor, float centerYAnchor, float width, float height) => new(rightAnchor - width, centerYAnchor - height / 2f, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromCenterTop(Vector2 centerTopAnchor, Vector2 size) => FromCenterTop(centerTopAnchor.X, centerTopAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromCenterTop(float centerAnchorX, float topAnchor, float width, float height) => new(centerAnchorX - width / 2f, topAnchor, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromCenterBottom(Vector2 centerBottomAnchor, Vector2 size) => FromCenterBottom(centerBottomAnchor.X, centerBottomAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromCenterBottom(float centerAnchorX, float bottomAnchor, float width, float height) => new(centerAnchorX - width / 2f, bottomAnchor - height, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLerp(Vector2 ratio, Vector2 targetAnchor, Vector2 size) => FromLerp(ratio.X, ratio.Y, targetAnchor.X, targetAnchor.Y, size.X, size.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromLerp(float ratioX, float ratioY, float targetAnchorX, float targetAnchorY, float width, float height) => new(targetAnchorX - width * ratioX, targetAnchorY - height * ratioY, width, height);

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromTo(Vector2 leftTopAnchor, Vector2 rightBottomAnchor) => FromTo(leftTopAnchor.X, leftTopAnchor.Y, rightBottomAnchor.X, rightBottomAnchor.Y);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromTo(float leftAnchor, float topAnchor, float rightAnchor, float bottomAnchor) => new(leftAnchor, topAnchor, rightAnchor - leftAnchor, bottomAnchor - topAnchor);

	/**
	<summary>
		<para>
			CSS-style <i>top-right-bottom-left</i> order.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Box4 FromSides(float topAnchor, float rightAnchor, float bottomAnchor, float leftAnchor) => new(leftAnchor, topAnchor, rightAnchor - leftAnchor, bottomAnchor - topAnchor);

	#endregion
	#region Coordinates-related properties: anchors

	public Vector2 LeftTopAnchor {
		readonly get => new(LeftAnchor, TopAnchor);
		set => (LeftAnchor, TopAnchor) = (value.X, value.Y);
	}

	public Vector2 RightTopAnchor {
		readonly get => new(RightAnchor, TopAnchor);
		set => (RightAnchor, TopAnchor) = (value.X, value.Y);
	}

	public Vector2 RightBottomAnchor {
		readonly get => new(RightAnchor, BottomAnchor);
		set => (RightAnchor, BottomAnchor) = (value.X, value.Y);
	}

	public Vector2 LeftBottomAnchor {
		readonly get => new(LeftAnchor, BottomAnchor);
		set => (LeftAnchor, BottomAnchor) = (value.X, value.Y);
	}

	public Vector2 CenterAnchor {
		readonly get => new(CenterXAnchor, CenterYAnchor);
		set => (CenterXAnchor, CenterYAnchor) = (value.X, value.Y);
	}

	public Vector2 LeftCenterAnchor {
		readonly get => new(LeftAnchor, CenterYAnchor);
		set => (LeftAnchor, CenterYAnchor) = (value.X, value.Y);
	}

	public Vector2 RightCenterAnchor {
		readonly get => new(RightAnchor, CenterYAnchor);
		set => (RightAnchor, CenterYAnchor) = (value.X, value.Y);
	}

	public Vector2 CenterTopAnchor {
		readonly get => new(CenterXAnchor, TopAnchor);
		set => (CenterXAnchor, TopAnchor) = (value.X, value.Y);
	}

	public Vector2 CenterBottomAnchor {
		readonly get => new(CenterXAnchor, BottomAnchor);
		set => (CenterXAnchor, BottomAnchor) = (value.X, value.Y);
	}

	public readonly Vector2 LerpAnchor(Vector2 ratio) => new(LerpXAnchor(ratio.X), LerpYAnchor(ratio.Y));
	public void SetLerpAnchor(Vector2 ratio, Vector2 value) {
		SetLerpXAnchor(ratio.X, value.X);
		SetLerpYAnchor(ratio.Y, value.Y);
	}
	public readonly Box4 WithLerpAnchor(Vector2 ratio, Vector2 value) {
		var copy = this.WithLerpXAnchor(ratio.X, value.X);

		copy.SetLerpYAnchor(ratio.Y, value.Y);

		return copy;
	}

	public float LeftAnchor {
		readonly get => L;
		set => L = value;
	}

	public float TopAnchor {
		readonly get => T;
		set => T = value;
	}

	public float RightAnchor {
		readonly get => L + W;
		set => L = value - W;
	}

	public float BottomAnchor {
		readonly get => T + H;
		set => T = value - H;
	}

	public float CenterXAnchor {
		readonly get => L + W / 2f;
		set => L = value - W / 2f;
	}

	public float CenterYAnchor {
		readonly get => T + H / 2f;
		set => T = value - H / 2f;
	}

	public readonly float LerpXAnchor(float ratio) => L + W * ratio;
	public void SetLerpXAnchor(float ratio, float value) => L = value - W * ratio;
	public readonly Box4 WithLerpXAnchor(float ratio, float value) => this with { L = value - W * ratio };

	public readonly float LerpYAnchor(float ratio) => T + H * ratio;
	public void SetLerpYAnchor(float ratio, float value) => T = value - H * ratio;
	public readonly Box4 WithLerpYAnchor(float ratio, float value) => this with { T = value - H * ratio };

	#endregion
	#region Coordinates-related properties: sides

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

	public float Left {
		readonly get => L;
		set => W += L - (L = value);
	}

	public float Top {
		readonly get => T;
		set => H += T - (T = value);
	}

	public float Right {
		readonly get => L + W;
		set => W = value - L;
	}

	public float Bottom {
		readonly get => T + H;
		set => H = value - T;
	}

	#endregion
	#region Size-related properties

	public Vector2 LeftTopAnchoredSize {
		readonly get => Size;
		set => (LeftAnchoredWidth, TopAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 RightTopAnchoredSize {
		readonly get => Size;
		set => (RightAnchoredWidth, TopAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 RightBottomAnchoredSize {
		readonly get => Size;
		set => (RightAnchoredWidth, BottomAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 LeftBottomAnchoredSize {
		readonly get => Size;
		set => (LeftAnchoredWidth, BottomAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 CenterAnchoredSize {
		readonly get => Size;
		set => (CenterAnchoredWidth, CenterAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 LeftCenterAnchoredSize {
		readonly get => Size;
		set => (LeftAnchoredWidth, CenterAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 RightCenterAnchoredSize {
		readonly get => Size;
		set => (RightAnchoredWidth, CenterAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 CenterTopAnchoredSize {
		readonly get => Size;
		set => (CenterAnchoredWidth, TopAnchoredHeight) = (value.X, value.Y);
	}

	public Vector2 CenterBottomAnchoredSize {
		readonly get => Size;
		set => (CenterAnchoredWidth, BottomAnchoredHeight) = (value.X, value.Y);
	}

	public readonly Vector2 Size => new(W, H);
	public void SetLerpAnchoredSize(Vector2 ratio, Vector2 value) {
		SetLerpAnchoredWidth(ratio.X, value.X);
		SetLerpAnchoredHeight(ratio.Y, value.Y);
	}
	public readonly Box4 WithLerpAnchoredSize(Vector2 ratio, Vector2 value) {
		var copy = this.WithLerpAnchoredWidth(ratio.X, value.X);

		copy.SetLerpAnchoredHeight(ratio.Y, value.Y);

		return copy;
	}

	public float LeftAnchoredWidth {
		readonly get => W;
		set => W = value;
	}

	public float TopAnchoredHeight {
		readonly get => H;
		set => H = value;
	}

	public float RightAnchoredWidth {
		readonly get => W;
		set => L += W - (W = value);
	}

	public float BottomAnchoredHeight {
		readonly get => H;
		set => T += H - (H = value);
	}

	public float CenterAnchoredWidth {
		readonly get => W;
		set => L += (W - (W = value)) / 2f;
	}

	public float CenterAnchoredHeight {
		readonly get => H;
		set => T += (H - (H = value)) / 2f;
	}

	public readonly float Width => W;
	public void SetLerpAnchoredWidth(float ratio, float value) => L += (W - (W = value)) * ratio;
	public readonly Box4 WithLerpAnchoredWidth(float ratio, float value) => this with { L = L + (W - value) * ratio, W = value };

	public readonly float Height => H;
	public void SetLerpAnchoredHeight(float ratio, float value) => T += (H - (H = value)) * ratio;
	public readonly Box4 WithLerpAnchoredHeight(float ratio, float value) => this with { T = T + (H - value) * ratio, H = value };

	#endregion
	#region Methods

	public void OffsetBy(Vector2 offset) => OffsetBy(offset.X, offset.Y);
	public void OffsetBy(float dxy) => OffsetBy(dxy, dxy);
	public void OffsetBy(float dx, float dy) {
		L += dx;
		T += dy;
	}

	public readonly Box4 WithOffset(Vector2 offset) => WithOffset(offset.X, offset.Y);
	public readonly Box4 WithOffset(float dxy) => WithOffset(dxy, dxy);
	public readonly Box4 WithOffset(float dx, float dy) => this with {
		L = L + dx,
		T = T + dy,
	};

	/**
	<summary>
		<para>
			Offset by polar coordinates.
		</para>
	</summary>
	**/
	public void AngularOffsetBy(float angle, float length) {
		L += angle.Cos() * length;
		T += angle.Sin() * length;
	}

	/**
	<inheritdoc cref="AngularOffsetBy(float, float)"/>
	**/
	public readonly Box4 WithAngularOffset(float angle, float length) => this with {
		L = L + angle.Cos() * length,
		T = T + angle.Sin() * length,
	};

	public void InflateBy(Margin4 margin) => InflateBy(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public void InflateBy(float all) => InflateBy(all, all, all, all);
	public void InflateBy(float vertical, float horizontal) => InflateBy(vertical, horizontal, vertical, horizontal);
	public void InflateBy(float top, float horizontal, float bottom) => InflateBy(top, horizontal, bottom, horizontal);
	public void InflateBy(float top, float right, float bottom, float left) {
		L -= left;
		T -= top;
		W += left + right;
		H += top + bottom;
	}

	public readonly Box4 Inflated(Margin4 margin) => Inflated(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public readonly Box4 Inflated(float all) => Inflated(all, all, all, all);
	public readonly Box4 Inflated(float vertical, float horizontal) => Inflated(vertical, horizontal, vertical, horizontal);
	public readonly Box4 Inflated(float top, float horizontal, float bottom) => Inflated(top, horizontal, bottom, horizontal);
	public readonly Box4 Inflated(float top, float right, float bottom, float left) => this with {
		L = L - left,
		T = T - top,
		W = W + left + right,
		H = H + top + bottom,
	};

	public void DeflateBy(Margin4 margin) => DeflateBy(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public void DeflateBy(float all) => DeflateBy(all, all, all, all);
	public void DeflateBy(float vertical, float horizontal) => DeflateBy(vertical, horizontal, vertical, horizontal);
	public void DeflateBy(float top, float horizontal, float bottom) => DeflateBy(top, horizontal, bottom, horizontal);
	public void DeflateBy(float top, float right, float bottom, float left) {
		L += left;
		T += top;
		W -= left + right;
		H -= top + bottom;
	}

	public readonly Box4 Deflated(Margin4 margin) => Deflated(margin.Top, margin.Right, margin.Bottom, margin.Left);
	public readonly Box4 Deflated(float all) => Deflated(all, all, all, all);
	public readonly Box4 Deflated(float vertical, float horizontal) => Deflated(vertical, horizontal, vertical, horizontal);
	public readonly Box4 Deflated(float top, float horizontal, float bottom) => Deflated(top, horizontal, bottom, horizontal);
	public readonly Box4 Deflated(float top, float right, float bottom, float left) => this with {
		L = L + left,
		T = T + top,
		W = W - left - right,
		H = H - top - bottom,
	};

	/**
	<summary>
		<para>
			Updates <see langword="this"/> <see cref="Box4"/>,
			enlarging it so that its bounds completely include <paramref name="min"/>'s.
		</para>
	</summary>
	**/
	public void CoerceAtLeast(Box4 min) {
		Left = Left.CoerceAtMost(min.Left);
		Top = Top.CoerceAtMost(min.Top);
		Right = Right.CoerceAtLeast(min.Right);
		Bottom = Bottom.CoerceAtLeast(min.Bottom);
	}

	/**
	<inheritdoc cref="CoerceAtLeast(Box4)"/>
	**/
	public readonly Box4 CoercedAtLeast(Box4 min) => this with {
		Left = Left.CoerceAtMost(min.Left),
		Top = Top.CoerceAtMost(min.Top),
		Right = Right.CoerceAtLeast(min.Right),
		Bottom = Bottom.CoerceAtLeast(min.Bottom),
	};

	/**
	<summary>
		<para>
			Updates <see langword="this"/> <see cref="Box4"/>,
			shrinking it so that its bounds are completely included in <paramref name="max"/>'es.
		</para>
	</summary>
	**/
	public void CoerceAtMost(Box4 max) {
		Left = Left.CoerceIn(max.Left, max.Right, ArgumentValidation.Lenient);
		Top = Top.CoerceIn(max.Top, max.Bottom, ArgumentValidation.Lenient);
		Right = Right.CoerceIn(max.Left, max.Right, ArgumentValidation.Lenient);
		Bottom = Bottom.CoerceIn(max.Top, max.Bottom, ArgumentValidation.Lenient);
	}

	/**
	<inheritdoc cref="CoerceAtMost(Box4)"/>
	**/
	public readonly Box4 CoercedAtMost(Box4 max) => this with {
		Left = Left.CoerceIn(max.Left, max.Right, ArgumentValidation.Lenient),
		Top = Top.CoerceIn(max.Top, max.Bottom, ArgumentValidation.Lenient),
		Right = Right.CoerceIn(max.Left, max.Right, ArgumentValidation.Lenient),
		Bottom = Bottom.CoerceIn(max.Top, max.Bottom, ArgumentValidation.Lenient),
	};

	// TODO: Box4.CoerceIn()
	/*
	public void CoerceIn(Box4 min, Box4 max, ArgumentValidation argumentValidation = default) {}
	public readonly Box4 CoercedIn(Box4 min, Box4 max, ArgumentValidation argumentValidation = default) => default;
	*/

	/**
	<summary>
		<para>
			<see cref="Right"/> comes before <see cref="Left"/>?
			<see cref="Size"/> has negative components?
			<see cref="Normalize"/> turns your <see cref="Box4"/> back upright,
			exchanging <see cref="LeftTop"/> and <see cref="RightBottom"/> if needs be.
		</para>
	</summary>
	<remarks>
		<para>
			It would be <b>counter-productive</b>
			to check for a negative <see cref="Width"/> or <see cref="Height"/>
			before calling <see cref="Normalize"/>,
			since the method already performs these tests.
		</para>
	</remarks>
	**/
	public void Normalize() {
		if (W < 0) L -= W = -W;
		if (H < 0) T -= H = -H;
	}

	/**
	<inheritdoc cref="Normalize"/>
	**/
	public readonly Box4 Normalized() {
		var copy = this with {};

		copy.Normalize();

		return copy;
	}

	public readonly bool Contains(Vector2 point) =>
		   Left < point.X && point.X < Right
		&& Top < point.Y && point.Y < Bottom;

	#endregion
}
