namespace Uy;

public interface IDeviceIndependentResourceInitializer {
	/**
	<summary>
		<para>
			<see cref="IDeviceIndependentResourceInitializer"/> implementations
			registered in the dependency injection container
			have their <see cref="OnInit(IDeviceIndependentResourceDictionary)"/> method
			automatically called upon application startup.
		</para>
		<para>
			Implementations are expected to populate the <paramref name="applicationResources"/>
			with whatever device-independent resources are relevant for the application being developed.
		</para>
	</summary>
	**/
	void OnInit(IDeviceIndependentResourceDictionary applicationResources);

	/**
	<summary>
		<para>
			<see cref="IDeviceIndependentResourceInitializer"/> implementations
			registered in the dependency injection container
			have their <see cref="OnDispose(IDeviceIndependentResourceDictionary)"/> method
			automatically called upon application shutdown,
			just before the rest of the <paramref name="applicationResources"/> are disposed.
		</para>
	</summary>
	<remarks>
		<para>
			The <paramref name="applicationResources"/> <b>WILL BE</b> disposed automatically
			just after invoking <see cref="OnDispose(IDeviceIndependentResourceDictionary)"/>.
			This method feels rather useless indeed...
		</para>
	</remarks>
	**/
	void OnDispose(IDeviceIndependentResourceDictionary applicationResources) {}
}
