using LinqToYourDoom;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.WIC;

namespace Uy;

/**
<summary>
	<para>
		Represents a collection of <see cref="Vortice.Direct2D1"/> <i>device-independent</i> resources.
		Device-independent resources are automatically initialized when the application starts,
		and disposed when the application shuts down.
	</para>
	<para>
		Refer to the overloads
		of the <see cref="Set(Symbol{ID2D1RectangleGeometry}, ID2D1RectangleGeometry)">Set</see> method
		to find out what <see cref="Vortice.Direct2D1"/> resources are considered <i>device-independent</i>.
	</para>
	<para>
		A singleton <see cref="IDeviceIndependentResourceDictionary"/>
		is available via dependency injection,
		and is automatically populated by the <see cref="IDeviceIndependentResourceInitializer"/>s
		registered in the dependency injection container.
	</para>
</summary>
<remarks>
	<para>
		This interface <b>IS</b> available via dependency injection.
	</para>
	<para>
		This interface is <b>NOT</b> intended to be user-replaceable.
	</para>
</remarks>
**/
public interface IDeviceIndependentResourceDictionary : IResourceDictionary {
	ID2D1Factory7 D2Factory { get; }
	IWICImagingFactory2 WicFactory { get; }
	IDWriteFactory7 WriteFactory { get; }

	#region Device-dependency-safe methods

	ID2D1EllipseGeometry Set(Symbol<ID2D1EllipseGeometry> key, ID2D1EllipseGeometry value) => UncheckedSet(key, value);
	ID2D1GeometryGroup Set(Symbol<ID2D1GeometryGroup> key, ID2D1GeometryGroup value) => UncheckedSet(key, value);
	ID2D1PathGeometry Set(Symbol<ID2D1PathGeometry> key, ID2D1PathGeometry value) => UncheckedSet(key, value);
	ID2D1PathGeometry1 Set(Symbol<ID2D1PathGeometry1> key, ID2D1PathGeometry1 value) => UncheckedSet(key, value);
	ID2D1RectangleGeometry Set(Symbol<ID2D1RectangleGeometry> key, ID2D1RectangleGeometry value) => UncheckedSet(key, value);
	ID2D1RoundedRectangleGeometry Set(Symbol<ID2D1RoundedRectangleGeometry> key, ID2D1RoundedRectangleGeometry value) => UncheckedSet(key, value);
	ID2D1StrokeStyle Set(Symbol<ID2D1StrokeStyle> key, ID2D1StrokeStyle value) => UncheckedSet(key, value);
	ID2D1StrokeStyle1 Set(Symbol<ID2D1StrokeStyle1> key, ID2D1StrokeStyle1 value) => UncheckedSet(key, value);
	ID2D1TransformedGeometry Set(Symbol<ID2D1TransformedGeometry> key, ID2D1TransformedGeometry value) => UncheckedSet(key, value);

	IWICBitmap Set(Symbol<IWICBitmap> key, IWICBitmap value) => UncheckedSet(key, value);
	IWICBitmapClipper Set(Symbol<IWICBitmapClipper> key, IWICBitmapClipper value) => UncheckedSet(key, value);
	IWICBitmapFlipRotator Set(Symbol<IWICBitmapFlipRotator> key, IWICBitmapFlipRotator value) => UncheckedSet(key, value);
	IWICBitmapScaler Set(Symbol<IWICBitmapScaler> key, IWICBitmapScaler value) => UncheckedSet(key, value);
	IWICColorContext Set(Symbol<IWICColorContext> key, IWICColorContext value) => UncheckedSet(key, value);
	IWICColorTransform Set(Symbol<IWICColorTransform> key, IWICColorTransform value) => UncheckedSet(key, value);
	IWICComponentInfo Set(Symbol<IWICComponentInfo> key, IWICComponentInfo value) => UncheckedSet(key, value);
	IWICBitmapDecoder Set(Symbol<IWICBitmapDecoder> key, IWICBitmapDecoder value) => UncheckedSet(key, value);
	IWICBitmapEncoder Set(Symbol<IWICBitmapEncoder> key, IWICBitmapEncoder value) => UncheckedSet(key, value);
	IWICFastMetadataEncoder Set(Symbol<IWICFastMetadataEncoder> key, IWICFastMetadataEncoder value) => UncheckedSet(key, value);
	IWICFormatConverter Set(Symbol<IWICFormatConverter> key, IWICFormatConverter value) => UncheckedSet(key, value);
	IWICPalette Set(Symbol<IWICPalette> key, IWICPalette value) => UncheckedSet(key, value);
	IWICMetadataQueryWriter Set(Symbol<IWICMetadataQueryWriter> key, IWICMetadataQueryWriter value) => UncheckedSet(key, value);

	IDWriteColorGlyphRunEnumerator Set(Symbol<IDWriteColorGlyphRunEnumerator> key, IDWriteColorGlyphRunEnumerator value) => UncheckedSet(key, value);
	IDWriteColorGlyphRunEnumerator1 Set(Symbol<IDWriteColorGlyphRunEnumerator1> key, IDWriteColorGlyphRunEnumerator1 value) => UncheckedSet(key, value);
	IDWriteFontCollection Set(Symbol<IDWriteFontCollection> key, IDWriteFontCollection value) => UncheckedSet(key, value);
	IDWriteFontCollection1 Set(Symbol<IDWriteFontCollection1> key, IDWriteFontCollection1 value) => UncheckedSet(key, value);
	IDWriteFontCollection2 Set(Symbol<IDWriteFontCollection2> key, IDWriteFontCollection2 value) => UncheckedSet(key, value);
	IDWriteFontCollection3 Set(Symbol<IDWriteFontCollection3> key, IDWriteFontCollection3 value) => UncheckedSet(key, value);
	IDWriteFontDownloadQueue Set(Symbol<IDWriteFontDownloadQueue> key, IDWriteFontDownloadQueue value) => UncheckedSet(key, value);
	IDWriteFontFace Set(Symbol<IDWriteFontFace> key, IDWriteFontFace value) => UncheckedSet(key, value);
	IDWriteFontFace1 Set(Symbol<IDWriteFontFace1> key, IDWriteFontFace1 value) => UncheckedSet(key, value);
	IDWriteFontFace2 Set(Symbol<IDWriteFontFace2> key, IDWriteFontFace2 value) => UncheckedSet(key, value);
	IDWriteFontFace3 Set(Symbol<IDWriteFontFace3> key, IDWriteFontFace3 value) => UncheckedSet(key, value);
	IDWriteFontFace4 Set(Symbol<IDWriteFontFace4> key, IDWriteFontFace4 value) => UncheckedSet(key, value);
	IDWriteFontFace5 Set(Symbol<IDWriteFontFace5> key, IDWriteFontFace5 value) => UncheckedSet(key, value);
	IDWriteFontFace6 Set(Symbol<IDWriteFontFace6> key, IDWriteFontFace6 value) => UncheckedSet(key, value);
	IDWriteFontFaceReference Set(Symbol<IDWriteFontFaceReference> key, IDWriteFontFaceReference value) => UncheckedSet(key, value);
	IDWriteFontFaceReference1 Set(Symbol<IDWriteFontFaceReference1> key, IDWriteFontFaceReference1 value) => UncheckedSet(key, value);
	IDWriteFontFallback Set(Symbol<IDWriteFontFallback> key, IDWriteFontFallback value) => UncheckedSet(key, value);
	IDWriteFontFallbackBuilder Set(Symbol<IDWriteFontFallbackBuilder> key, IDWriteFontFallbackBuilder value) => UncheckedSet(key, value);
	IDWriteFontFile Set(Symbol<IDWriteFontFile> key, IDWriteFontFile value) => UncheckedSet(key, value);
	IDWriteFontFileStream Set(Symbol<IDWriteFontFileStream> key, IDWriteFontFileStream value) => UncheckedSet(key, value);
	IDWriteFontResource Set(Symbol<IDWriteFontResource> key, IDWriteFontResource value) => UncheckedSet(key, value);
	IDWriteFontSet Set(Symbol<IDWriteFontSet> key, IDWriteFontSet value) => UncheckedSet(key, value);
	IDWriteFontSet1 Set(Symbol<IDWriteFontSet1> key, IDWriteFontSet1 value) => UncheckedSet(key, value);
	IDWriteFontSet2 Set(Symbol<IDWriteFontSet2> key, IDWriteFontSet2 value) => UncheckedSet(key, value);
	IDWriteFontSet3 Set(Symbol<IDWriteFontSet3> key, IDWriteFontSet3 value) => UncheckedSet(key, value);
	IDWriteFontSetBuilder Set(Symbol<IDWriteFontSetBuilder> key, IDWriteFontSetBuilder value) => UncheckedSet(key, value);
	IDWriteFontSetBuilder1 Set(Symbol<IDWriteFontSetBuilder1> key, IDWriteFontSetBuilder1 value) => UncheckedSet(key, value);
	IDWriteFontSetBuilder2 Set(Symbol<IDWriteFontSetBuilder2> key, IDWriteFontSetBuilder2 value) => UncheckedSet(key, value);
	IDWriteGlyphRunAnalysis Set(Symbol<IDWriteGlyphRunAnalysis> key, IDWriteGlyphRunAnalysis value) => UncheckedSet(key, value);
	IDWriteInlineObject Set(Symbol<IDWriteInlineObject> key, IDWriteInlineObject value) => UncheckedSet(key, value);
	IDWriteInMemoryFontFileLoader Set(Symbol<IDWriteInMemoryFontFileLoader> key, IDWriteInMemoryFontFileLoader value) => UncheckedSet(key, value);
	IDWriteNumberSubstitution Set(Symbol<IDWriteNumberSubstitution> key, IDWriteNumberSubstitution value) => UncheckedSet(key, value);
	IDWriteRemoteFontFileLoader Set(Symbol<IDWriteRemoteFontFileLoader> key, IDWriteRemoteFontFileLoader value) => UncheckedSet(key, value);
	IDWriteRenderingParams Set(Symbol<IDWriteRenderingParams> key, IDWriteRenderingParams value) => UncheckedSet(key, value);
	IDWriteRenderingParams1 Set(Symbol<IDWriteRenderingParams1> key, IDWriteRenderingParams1 value) => UncheckedSet(key, value);
	IDWriteRenderingParams2 Set(Symbol<IDWriteRenderingParams2> key, IDWriteRenderingParams2 value) => UncheckedSet(key, value);
	IDWriteRenderingParams3 Set(Symbol<IDWriteRenderingParams3> key, IDWriteRenderingParams3 value) => UncheckedSet(key, value);
	IDWriteTextAnalyzer Set(Symbol<IDWriteTextAnalyzer> key, IDWriteTextAnalyzer value) => UncheckedSet(key, value);
	IDWriteTextAnalyzer1 Set(Symbol<IDWriteTextAnalyzer1> key, IDWriteTextAnalyzer1 value) => UncheckedSet(key, value);
	IDWriteTextAnalyzer2 Set(Symbol<IDWriteTextAnalyzer2> key, IDWriteTextAnalyzer2 value) => UncheckedSet(key, value);
	IDWriteTextFormat Set(Symbol<IDWriteTextFormat> key, IDWriteTextFormat value) => UncheckedSet(key, value);
	IDWriteTextFormat1 Set(Symbol<IDWriteTextFormat1> key, IDWriteTextFormat1 value) => UncheckedSet(key, value);
	IDWriteTextFormat2 Set(Symbol<IDWriteTextFormat2> key, IDWriteTextFormat2 value) => UncheckedSet(key, value);
	IDWriteTextFormat3 Set(Symbol<IDWriteTextFormat3> key, IDWriteTextFormat3 value) => UncheckedSet(key, value);
	IDWriteTextLayout Set(Symbol<IDWriteTextLayout> key, IDWriteTextLayout value) => UncheckedSet(key, value);
	IDWriteTextLayout1 Set(Symbol<IDWriteTextLayout1> key, IDWriteTextLayout1 value) => UncheckedSet(key, value);
	IDWriteTextLayout2 Set(Symbol<IDWriteTextLayout2> key, IDWriteTextLayout2 value) => UncheckedSet(key, value);
	IDWriteTextLayout3 Set(Symbol<IDWriteTextLayout3> key, IDWriteTextLayout3 value) => UncheckedSet(key, value);
	IDWriteTextLayout4 Set(Symbol<IDWriteTextLayout4> key, IDWriteTextLayout4 value) => UncheckedSet(key, value);
	IDWriteTypography Set(Symbol<IDWriteTypography> key, IDWriteTypography value) => UncheckedSet(key, value);

	#endregion
}
