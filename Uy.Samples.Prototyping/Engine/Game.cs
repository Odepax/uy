using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Uy;

abstract class Game {
	public Window Window = new();
	public Clock Clock = new();
	public Stick Stick1 = new();
	public Stick Stick2 = new();

	public IScheduler Scheduler = null!;

	/**
	<summary>
		<para>
			For loading physics resources.
			No hot-reload here...
		</para>
	</summary>
	**/
	public abstract void OnLoad(CompositeDisposable disposables);

	/**
	<summary>
		<para>
			For loading device-independent graphics resources.
			No hot-reload here...
		</para>
	</summary>
	**/
	public abstract void OnApplicationInit(IDeviceIndependentResourceDictionary applicationResources, CompositeDisposable disposables);

	/**
	<summary>
		<para>
			For loading device-dependent graphics resources.
			Partial hot-reload on device loss.
		</para>
	</summary>
	**/
	public abstract void OnDeviceInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables);

	/**
	<summary>
		<para>
			For loading window size-dependent graphics resources.
			Partial hot-reload on size change.
		</para>
	</summary>
	**/
	public abstract void OnResizeInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables);

	/**
	<summary>
		<para>
			Physics time!
			Hot-reloadable!
		</para>
	</summary>
	**/
	public abstract void OnUpdate();

	/**
	<summary>
		<para>
			Graphics time!
			Hot-reloadable!
		</para>
	</summary>
	**/
	public abstract void OnRender(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources);
}
