using LinqToYourDoom;
using Vortice.Direct2D1;

namespace Uy;

/**
<summary>
	<para>
		Represents a collection of <see cref="Vortice.Direct2D1"/> <i>device-dependent</i> resources.
		Device-dependent resources are automatically disposed whenever the device is <i>lost</i>,
		and (re)initialized whenever a window enters a render pass.
	</para>
	<para>
		Refer to the overloads
		of the <see cref="Set(Symbol{ID2D1SolidColorBrush}, ID2D1SolidColorBrush)">Set</see> method
		to find out what <see cref="Vortice.Direct2D1"/> resources are considered <i>device-dependent</i>.
	</para>
	<para>
		One <see cref="IDeviceDependentResourceDictionary"/> is created along with each window,
		and is automatically populated with a <see cref="ID2D1DeviceContext6"/>,
		as well as by the <see cref="IDeviceDependentResourceInitializer"/>s
		registered in the dependency injection container.
	</para>
</summary>
<remarks>
	<para>
		This interface is <b>NOT</b> available through the dependency injection container.
		The reason for this is to keep accesses to the device-dependent resources
		synchronized with the render pass of the game loop.
	</para>
	<para>
		There are moments when the device has been lost, and the resources have been disposed,
		but a new render pass isn't required because the content of the window hasn't changed.
		In this case, it's possible to spend several frames with an empty resource dictionary,
		as the (re)initialization of the device-dependent resources is <b>lazy</b>.
	</para>
	<para>
		One <b>MUST NOT keep a reference</b> to the <see cref="IDeviceDependentResourceDictionary"/>!
	</para>
</remarks>
**/
public interface IDeviceDependentResourceDictionary : IResourceDictionary {
	ID2D1Device6 D2Device { get; }
	ID2D1DeviceContext6 D2DeviceContext { get; }

	#region Device-dependency-safe methods

	ID2D1DeviceContext Set(Symbol<ID2D1DeviceContext> key, ID2D1DeviceContext value) => UncheckedSet(key, value);
	ID2D1DeviceContext1 Set(Symbol<ID2D1DeviceContext1> key, ID2D1DeviceContext1 value) => UncheckedSet(key, value);
	ID2D1DeviceContext2 Set(Symbol<ID2D1DeviceContext2> key, ID2D1DeviceContext2 value) => UncheckedSet(key, value);
	ID2D1DeviceContext3 Set(Symbol<ID2D1DeviceContext3> key, ID2D1DeviceContext3 value) => UncheckedSet(key, value);
	ID2D1DeviceContext4 Set(Symbol<ID2D1DeviceContext4> key, ID2D1DeviceContext4 value) => UncheckedSet(key, value);
	ID2D1DeviceContext5 Set(Symbol<ID2D1DeviceContext5> key, ID2D1DeviceContext5 value) => UncheckedSet(key, value);
	ID2D1DeviceContext6 Set(Symbol<ID2D1DeviceContext6> key, ID2D1DeviceContext6 value) => UncheckedSet(key, value);
	ID2D1PrintControl Set(Symbol<ID2D1PrintControl> key, ID2D1PrintControl value) => UncheckedSet(key, value);

	ID2D1Bitmap Set(Symbol<ID2D1Bitmap> key, ID2D1Bitmap value) => UncheckedSet(key, value);
	ID2D1Bitmap1 Set(Symbol<ID2D1Bitmap1> key, ID2D1Bitmap1 value) => UncheckedSet(key, value);
	ID2D1BitmapBrush Set(Symbol<ID2D1BitmapBrush> key, ID2D1BitmapBrush value) => UncheckedSet(key, value);
	ID2D1BitmapBrush1 Set(Symbol<ID2D1BitmapBrush1> key, ID2D1BitmapBrush1 value) => UncheckedSet(key, value);
	ID2D1BitmapRenderTarget Set(Symbol<ID2D1BitmapRenderTarget> key, ID2D1BitmapRenderTarget value) => UncheckedSet(key, value);
	ID2D1ColorContext Set(Symbol<ID2D1ColorContext> key, ID2D1ColorContext value) => UncheckedSet(key, value);
	ID2D1ColorContext1 Set(Symbol<ID2D1ColorContext1> key, ID2D1ColorContext1 value) => UncheckedSet(key, value);
	ID2D1CommandList Set(Symbol<ID2D1CommandList> key, ID2D1CommandList value) => UncheckedSet(key, value);
	ID2D1GeometryRealization Set(Symbol<ID2D1GeometryRealization> key, ID2D1GeometryRealization value) => UncheckedSet(key, value);
	ID2D1GradientMesh Set(Symbol<ID2D1GradientMesh> key, ID2D1GradientMesh value) => UncheckedSet(key, value);
	ID2D1GradientStopCollection Set(Symbol<ID2D1GradientStopCollection> key, ID2D1GradientStopCollection value) => UncheckedSet(key, value);
	ID2D1GradientStopCollection1 Set(Symbol<ID2D1GradientStopCollection1> key, ID2D1GradientStopCollection1 value) => UncheckedSet(key, value);
	ID2D1ImageBrush Set(Symbol<ID2D1ImageBrush> key, ID2D1ImageBrush value) => UncheckedSet(key, value);
	ID2D1ImageSource Set(Symbol<ID2D1ImageSource> key, ID2D1ImageSource value) => UncheckedSet(key, value);
	ID2D1ImageSourceFromWic Set(Symbol<ID2D1ImageSourceFromWic> key, ID2D1ImageSourceFromWic value) => UncheckedSet(key, value);
	ID2D1Ink Set(Symbol<ID2D1Ink> key, ID2D1Ink value) => UncheckedSet(key, value);
	ID2D1InkStyle Set(Symbol<ID2D1InkStyle> key, ID2D1InkStyle value) => UncheckedSet(key, value);
	ID2D1Layer Set(Symbol<ID2D1Layer> key, ID2D1Layer value) => UncheckedSet(key, value);
	ID2D1LinearGradientBrush Set(Symbol<ID2D1LinearGradientBrush> key, ID2D1LinearGradientBrush value) => UncheckedSet(key, value);
	ID2D1LookupTable3D Set(Symbol<ID2D1LookupTable3D> key, ID2D1LookupTable3D value) => UncheckedSet(key, value);
	ID2D1Mesh Set(Symbol<ID2D1Mesh> key, ID2D1Mesh value) => UncheckedSet(key, value);
	ID2D1RadialGradientBrush Set(Symbol<ID2D1RadialGradientBrush> key, ID2D1RadialGradientBrush value) => UncheckedSet(key, value);
	ID2D1SolidColorBrush Set(Symbol<ID2D1SolidColorBrush> key, ID2D1SolidColorBrush value) => UncheckedSet(key, value);
	ID2D1SpriteBatch Set(Symbol<ID2D1SpriteBatch> key, ID2D1SpriteBatch value) => UncheckedSet(key, value);
	ID2D1SvgDocument Set(Symbol<ID2D1SvgDocument> key, ID2D1SvgDocument value) => UncheckedSet(key, value);
	ID2D1SvgGlyphStyle Set(Symbol<ID2D1SvgGlyphStyle> key, ID2D1SvgGlyphStyle value) => UncheckedSet(key, value);
	ID2D1TransformedImageSource Set(Symbol<ID2D1TransformedImageSource> key, ID2D1TransformedImageSource value) => UncheckedSet(key, value);

	#endregion
}
