namespace Uy;

public readonly ref struct DeviceInitInfo {
	public readonly IDeviceIndependentResourceDictionary ApplicationResources;
	public readonly IDeviceDependentResourceDictionary DeviceResources;

	public DeviceInitInfo(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		ApplicationResources = applicationResources;
		DeviceResources = deviceResources;
	}
}
