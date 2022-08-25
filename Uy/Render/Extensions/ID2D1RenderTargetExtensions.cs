using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Direct2D1;

namespace Uy;

public static class ID2D1RenderTargetExtensions {
	/**
	<summary>
		<para>
			Alias of <c><paramref name="this"/>.<see cref="TransformScope(ID2D1RenderTarget, Matrix3x2)">TransformScope</see>(Matrix3x2.CreateRotation(<paramref name="rotation"/>) * <paramref name="this"/>.Transform);</c>
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TScope PushRotation(this ID2D1RenderTarget @this, float rotation) =>
		TransformScope(@this, Matrix3x2.CreateRotation(rotation) * @this.Transform);

	/**
	<summary>
		<para>
			Alias of <c><paramref name="this"/>.<see cref="TransformScope(ID2D1RenderTarget, Matrix3x2)">TransformScope</see>(Matrix3x2.CreateScale(<paramref name="scale"/>) * <paramref name="this"/>.Transform);</c>
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TScope PushScale(this ID2D1RenderTarget @this, float scale) =>
		TransformScope(@this, Matrix3x2.CreateScale(scale) * @this.Transform);

	/**
	<summary>
		<para>
			Alias of <c><paramref name="this"/>.<see cref="TransformScope(ID2D1RenderTarget, Matrix3x2)">TransformScope</see>(Matrix3x2.CreateTranslation(<paramref name="offset"/>) * <paramref name="this"/>.Transform);</c>
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TScope PushTranslation(this ID2D1RenderTarget @this, Vector2 offset) =>
		TransformScope(@this, Matrix3x2.CreateTranslation(offset) * @this.Transform);

	/**
	<summary>
		<para>
			Alias of <c><paramref name="this"/>.<see cref="TransformScope(ID2D1RenderTarget, Matrix3x2)">TransformScope</see>(Matrix3x2.CreateTranslation(<paramref name="x"/>, <paramref name="y"/>) * <paramref name="this"/>.Transform);</c>
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TScope PushTranslation(this ID2D1RenderTarget @this, float x, float y) =>
		TransformScope(@this, Matrix3x2.CreateTranslation(x, y) * @this.Transform);

	/**
	<summary>
		<para>
			Alias of <c><paramref name="this"/>.<see cref="TransformScope(ID2D1RenderTarget, Matrix3x2)">TransformScope</see>(<paramref name="transform"/> * <paramref name="this"/>.Transform);</c>
		</para>
	</summary>
	**/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TScope PushTransform(this ID2D1RenderTarget @this, Matrix3x2 transform) =>
		TransformScope(@this, transform * @this.Transform);

	/**
	<summary>
		<para>
			<see cref="TransformScope(ID2D1RenderTarget, Matrix3x2)"/> sets
			<see cref="ID2D1RenderTarget.Transform"/> to <paramref name="scopedTransform"/>,
			and returns a disposable that will restore the <see cref="ID2D1RenderTarget.Transform"/>
			to its original value when disposed.
		</para>
		<para>
			For instance:
		</para>
		<code><![CDATA[
		using (context.TransformScope(Matrix3x2.CreateScale(2)))
			context.FillRectangle(bounds, pinkBrush);
		]]></code>
	</summary>
	**/
	public static TScope TransformScope(this ID2D1RenderTarget @this, Matrix3x2 scopedTransform) {
		var savedTransform = @this.Transform;

		@this.Transform = scopedTransform;

		return new TScope(@this, savedTransform);
	}

	/**
	<inheritdoc cref="TransformScope(ID2D1RenderTarget, Matrix3x2)"/>
	**/
	public readonly struct TScope : IDisposable {
		readonly ID2D1RenderTarget Context;
		readonly Matrix3x2 SavedTransform;

		public TScope(ID2D1RenderTarget context, Matrix3x2 savedTransform) {
			Context = context;
			SavedTransform = savedTransform;
		}

		public void Dispose() => Context.Transform = SavedTransform;
	}
}
