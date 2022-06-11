namespace Uy;

public readonly ref struct RenderInfo {
	public readonly IDeviceIndependentResourceDictionary ApplicationResources;
	public readonly IDeviceDependentResourceDictionary DeviceResources;

	public RenderInfo(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		ApplicationResources = applicationResources;
		DeviceResources = deviceResources;
	}
}
