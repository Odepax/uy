using System.Numerics;
using System.Reflection;
using Uy;
using Vortice.Direct2D1;
using Vortice.WIC;

static class ResourceDictionaryExtensions {
	public static IWICFormatConverter LoadBitmap(this IDeviceIndependentResourceDictionary @this, string resourcePath, Assembly? resourceAssembly = null) =>
		@this.WicFactory.LoadBitmap(resourcePath, resourceAssembly);

	public static ID2D1Bitmap1 LoadBitmap(this IDeviceDependentResourceDictionary @this, IWICFormatConverter imageConverter) =>
		@this.D2DeviceContext.LoadBitmap(imageConverter);

	public static ID2D1SvgDocument LoadSvg(this IDeviceDependentResourceDictionary @this, IDeviceIndependentResourceDictionary applicationResources, Vector2 viewportSize, string resourcePath, Assembly? resourceAssembly = null) =>
		@this.D2DeviceContext.LoadSvg(applicationResources.WicFactory, viewportSize, resourcePath, resourceAssembly);
}
