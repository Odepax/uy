namespace Uy;

public interface IDeviceDependentResourceInitializer {
	/**
	<summary>
		<para>
			<see cref="IDeviceDependentResourceInitializer"/> implementations
			registered in the dependency injection container
			have their <see cref="OnDeviceInit(IDeviceIndependentResourceDictionary, IDeviceDependentResourceDictionary)"/> method
			automatically called when the device of window has been previously <i>lost</i>,
			and the resources are to be re-created.
		</para>
		<para>
			Implementations are expected to populate the <paramref name="deviceResources"/>
			with whatever device-dependent resources are relevant for the application being developed.
		</para>
	</summary>
	**/
	void OnDeviceInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources);

	/**
	<summary>
		<para>
			<see cref="IDeviceDependentResourceInitializer"/> implementations
			registered in the dependency injection container
			have their <see cref="OnDeviceDispose(IDeviceIndependentResourceDictionary, IDeviceDependentResourceDictionary)"/> method
			automatically called when the device of window is <i>lost</i>,
			just before the rest of the <paramref name="deviceResources"/> are disposed.
		</para>
	</summary>
	<remarks>
		<para>
			The <paramref name="deviceResources"/> <b>WILL BE</b> disposed automatically
			just after invoking <see cref="OnDeviceDispose(IDeviceIndependentResourceDictionary, IDeviceDependentResourceDictionary)"/>.
			This method feels rather useless indeed...
		</para>
	</remarks>
	**/
	void OnDeviceDispose(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {}
}
