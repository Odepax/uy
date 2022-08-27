using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Uy;

delegate void ApplicationResourceFactory(
	IDeviceIndependentResourceDictionary applicationResources,
	CompositeDisposable disposables
);

delegate void DeviceResourceFactory(
	IDeviceIndependentResourceDictionary applicationResources,
	IDeviceDependentResourceDictionary deviceResources,
	CompositeDisposable disposables
);

delegate void VolatileResourceFactory<T>(
	IDeviceIndependentResourceDictionary applicationResources,
	IDeviceDependentResourceDictionary deviceResources,
	T value,
	CompositeDisposable applicationDisposables,
	CompositeDisposable deviceDisposables
);

abstract class VolatileResourceDescriptor {
	public abstract void Sync(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable applicationDisposables, CompositeDisposable deviceDisposables);
}

class VolatileResourceDescriptor<T> : VolatileResourceDescriptor {
	readonly IObservable<T> VolatileObservable;
	readonly VolatileResourceFactory<T> Factory;

	T? Value;
	bool Sub = true;
	bool Re;

	public VolatileResourceDescriptor(IObservable<T> volatileobservable, VolatileResourceFactory<T> factory) {
		VolatileObservable = volatileobservable;
		Factory = factory;
	}

	public override void Sync(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable applicationDisposables, CompositeDisposable deviceDisposables) {
		if (Sub) {
			Sub = false;

			VolatileObservable
				.Subscribe(value => {
					Value = value;
					Re = true;
				})
				.DisposeWith(applicationDisposables);
		}

		if (Re) {
			Re = false;

			Factory.Invoke(applicationResources, deviceResources, Value!, applicationDisposables, deviceDisposables);
		}
	}
}

readonly ref struct ResourceCollection2 {
	readonly List<ApplicationResourceFactory> ApplicationResources;
	readonly List<DeviceResourceFactory> DeviceResources;
	readonly List<VolatileResourceDescriptor> VolatileResources;
	readonly IScheduler Scheduler;

	public ResourceCollection2(List<ApplicationResourceFactory> applicationResources, List<DeviceResourceFactory> deviceResources, List<VolatileResourceDescriptor> volatileResources, IScheduler scheduler) {
		ApplicationResources = applicationResources;
		DeviceResources = deviceResources;
		VolatileResources = volatileResources;
		Scheduler = scheduler;
	}

	public void AddApplication(ApplicationResourceFactory factory) =>
		ApplicationResources.Add(factory);

	public void AddDevice(DeviceResourceFactory factory) =>
		DeviceResources.Add(factory);

	/**
	<param name="volatileObservable">
		<para>
			Will be <see cref="Observable.ObserveOn{TSource}(IObservable{TSource}, IScheduler)">observed</see>
			on the <see cref="Clock.GameLoopScheduler">game-loop scheduler</see>.
		</para>
	</param>
	**/
	public void AddVolatile<T>(IObservable<T> volatileObservable, VolatileResourceFactory<T> factory) =>
		VolatileResources.Add(new VolatileResourceDescriptor<T>(volatileObservable.ObserveOn(Scheduler), factory));
}

abstract class Game3 {
	public Window Window = new();
	public Clock Clock = new();
	public Stick Stick1 = new();
	public Stick Stick2 = new();

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
			For loading graphics resources.
			No hot-reload here...
		</para>
	</summary>
	**/
	public abstract void OnLoad(in ResourceCollection2 R, CompositeDisposable disposables);

	/**
	<summary>
		<para>
			Physics time!
			Hot-reloadable!
		</para>
	</summary>
	**/
	public abstract void OnUpdate(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources);

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
