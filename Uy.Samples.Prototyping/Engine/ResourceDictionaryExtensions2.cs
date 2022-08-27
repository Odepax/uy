using LinqToYourDoom;
using System.Numerics;
using System.Reflection;
using Vortice.Direct2D1;
using Vortice.WIC;

static class ResourceDictionaryExtensions2 {
	public static IWICFormatConverter LoadBitmap(this IWICImagingFactory2 @this, string resourcePath, Assembly? resourceAssembly = null) {
		resourceAssembly ??= Assembly.GetExecutingAssembly();

		var resourceBytes = resourceAssembly.GetEmbeddedResourceBytes(resourcePath);
		using var imageStream = @this.CreateStream(resourceBytes);
		using var imageDecoder = @this.CreateDecoderFromStream(imageStream, DecodeOptions.CacheOnLoad);
		using var imageFrame = imageDecoder.GetFrame(0);

		var imageConverter = @this.CreateFormatConverter();

		imageConverter.Initialize(
			imageFrame,
			PixelFormat.Format32bppPBGRA,
			BitmapDitherType.None,
			iPalette: null,
			alphaThresholdPercent: 0,
			BitmapPaletteType.MedianCut
		);

		return imageConverter;
	}

	public static ID2D1Bitmap1 LoadBitmap(this ID2D1DeviceContext6 @this, IWICFormatConverter imageConverter) =>
		@this.CreateBitmapFromWicBitmap(imageConverter);

	public static ID2D1SvgDocument LoadSvg(this ID2D1DeviceContext6 @this, IWICImagingFactory2 wicFactory, Vector2 viewportSize, string resourcePath, Assembly? resourceAssembly = null) {
		resourceAssembly ??= Assembly.GetExecutingAssembly();

		var resourceBytes = resourceAssembly.GetEmbeddedResourceBytes(resourcePath);
		using var svgStream = wicFactory.CreateStream(resourceBytes);

		return @this.CreateSvgDocument(svgStream, new(viewportSize));
	}
}
