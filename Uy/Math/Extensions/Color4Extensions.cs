using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Mathematics;

namespace Uy;

public static class Color4Extensions {
	#region Conversion extensions

	/**
	<summary>
		<para>
			CSS-like <i>hash</i> syntax,
			e.g. <c>var transparentOrange = 0xFF450033.<see cref="ToRgba(uint)">ToRgba</see>()</c>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static Color4 ToRgba(this uint @this) =>
		new(
			((@this >> 24) & 0x000000ff) / 255f,
			((@this >> 16) & 0x000000ff) / 255f,
			((@this >> 08) & 0x000000ff) / 255f,
			((@this >> 00) & 0x000000ff) / 255f
		);

	/**
	<summary>
		<para>
			CSS-like <i>hash</i> syntax,
			e.g. <c>var orange = 0xFF4500.<see cref="ToRgba(uint)">ToRgb</see>()</c>.
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static Color4 ToRgb(this int @this) =>
		new(
			((@this >> 16) & 0x0000ff) / 255f,
			((@this >> 08) & 0x0000ff) / 255f,
			((@this >> 00) & 0x0000ff) / 255f,
			1f
		);

	public static void Deconstruct(this in Color4 @this, out float r, out float g, out float b, out float a) {
		r = @this.R;
		g = @this.G;
		b = @this.B;
		a = @this.A;
	}

	public static void Deconstruct(this in Color4 @this, out float r, out float g, out float b) {
		r = @this.R;
		g = @this.G;
		b = @this.B;
	}

	#endregion
	#region Color-space extensions

	/**
	<returns>
		<para>
			A <see cref="Vector4"/> of which the <see cref="Vector4.X"/>, <see cref="Vector4.Y"/>, <see cref="Vector4.Z"/> and <see cref="Vector4.W"/> components
			respectively correspond to <paramref name="this"/> <see cref="Color4"/>'s hue, saturation, lightness and <see cref="Color4.A">alpha</see>,
			all these ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static Vector4 Hsla(this in Color4 @this) => new(@this.Hsl(), @this.A);

	/**
	<returns>
		<para>
			A <see cref="Vector3"/> of which the <see cref="Vector3.X"/>, <see cref="Vector3.Y"/>, and <see cref="Vector3.Z"/> components
			respectively correspond to <paramref name="this"/> <see cref="Color4"/>'s hue, saturation and lightness,
			all these ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static Vector3 Hsl(this in Color4 @this) {
		var (l, s, h) = RGBMinMaxLDSH(@this);

		return new(h, s, l);
	}

	/**
	<returns>
		<para>
			The computed HSL hue of <paramref name="this"/> <see cref="Color4"/>,
			ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static float H(this in Color4 @this) => RGBMinMaxLDSH(@this).H;

	/**
	<returns>
		<para>
			A copy of <paramref name="this"/> <see cref="Color4"/>,
			with the HSL hue set to <paramref name="value"/>,
			ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static Color4 WithH(this in Color4 @this, float value) {
		var (_, s, l) = @this.Hsl();

		return HslaToRgba(value, s, l, @this.A);
	}

	/**
	<returns>
		<para>
			The computed HSL saturation of <paramref name="this"/> <see cref="Color4"/>,
			ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static float S(this in Color4 @this) => RGBMinMaxLDS(@this).S;

	/**
	<returns>
		<para>
			A copy of <paramref name="this"/> <see cref="Color4"/>,
			with the HSL saturation set to <paramref name="value"/>,
			ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static Color4 WithS(this in Color4 @this, float value) {
		var (h, _, l) = @this.Hsl();

		return HslaToRgba(h, value, l, @this.A);
	}

	/**
	<returns>
		<para>
			The computed HSL lightness of <paramref name="this"/> <see cref="Color4"/>,
			ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static float L(this in Color4 @this) => RGBMinMaxL(@this).L;

	/**
	<returns>
		<para>
			A copy of <paramref name="this"/> <see cref="Color4"/>,
			with the HSL lightness set to <paramref name="value"/>,
			ranging in <c>[0.0f, 1.0f]</c>.
		</para>
	</returns>
	**/
	public static Color4 WithL(this in Color4 @this, float value) {
		var (h, s, _) = @this.Hsl();

		return HslaToRgba(h, s, value, @this.A);
	}

	#endregion
	#region Color-space extensions: utilities

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static (float R, float G, float B, float Min, float Max, float L) RGBMinMaxL(in Color4 @this) {
		var (r, g, b) = @this;
		var max = MathF.Max(r, MathF.Max(g, b));
		var min = MathF.Min(r, MathF.Min(g, b));
		var l = (max + min) / 2f;

		return (r, g, b, min, max, l);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static (float R, float G, float B, float Max, float L, float D, float S) RGBMinMaxLDS(in Color4 @this) {
		var (r, g, b, min, max, l) = RGBMinMaxL(@this);
		var d = 0f;
		var s = 0f;

		if (max != min) {
			d = max - min;
			s = l > 0.5f
				? d / (2f - max - min)
				: d / (max + min);
		}

		return (r, g, b, max, l, d, s);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static (float L, float S, float H) RGBMinMaxLDSH(in Color4 @this) {
		var (r, g, b, max, l, d, s) = RGBMinMaxLDS(@this);
		var h = 0f;

		if (s != 0)
			h = (
				  max == r ? ((g - b) / d + (g < b ? 6f : 0f)) / 6f
				: max == g ? ((b - r) / d + 2f) / 6f
				: max == b ? ((r - g) / d + 4f) / 6f
				: 0
			);

		return (l, s, h);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	static Color4 HslaToRgba(float h, float s, float l, float a) {
		if (s == 0)
			return new Color4(l, l, l, a);

		else {
			var q = l < 0.5f ? l * (1f + s) : l + s - l * s;
			var p = 2f * l - q;
			var r = HueToRgb(p, q, h + 0.333_333f);
			var g = HueToRgb(p, q, h);
			var b = HueToRgb(p, q, h - 0.333_333f);

			return new(r, g, b, a);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	static float HueToRgb(float p, float q, float t) {
		if (t < 0) ++t;
		else if (1 < t) --t;

		if (t < 0.166_666f) return p + (q - p) * 6f * t;
		else if (t < 0.5f) return q;
		else if (t < 0.666_666f) return p + (q - p) * (0.666_666f - t) * 6f;
		else return p;
	}

	#endregion
}
