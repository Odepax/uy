namespace Uy;

public readonly ref struct DeviceDisposeInfo {
	public readonly IDeviceIndependentResourceDictionary ApplicationResources;
	public readonly IDeviceDependentResourceDictionary DeviceResources;

	public DeviceDisposeInfo(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		ApplicationResources = applicationResources;
		DeviceResources = deviceResources;
	}
}
