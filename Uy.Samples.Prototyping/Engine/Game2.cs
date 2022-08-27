using LinqToYourDoom;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.WIC;

abstract class ResourceDescriptor {
	public abstract void Populate(ResourceProvider1 R, IDeviceIndependentResourceDictionary applicationResources);
	public abstract void Populate(ResourceProvider2 R, IDeviceDependentResourceDictionary deviceResources);
	public abstract void Repopulate(ResourceProvider1 R1, ResourceProvider2 R2, IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources);
}

class ApplicationResourceDescriptor<TRes> : ResourceDescriptor where TRes : notnull {
	readonly Symbol<TRes> Key;
	readonly ResourceFactory1<TRes> Factory;

	public ApplicationResourceDescriptor(Symbol<TRes> key, ResourceFactory1<TRes> factory) {
		Key = key;
		Factory = factory;
	}

	public override void Populate(ResourceProvider1 R, IDeviceIndependentResourceDictionary applicationResources) =>
		applicationResources.UncheckedSet(Key, Factory.Invoke(R));

	public override void Populate(ResourceProvider2 R, IDeviceDependentResourceDictionary deviceResources) =>
		throw new NotImplementedException();

	public override void Repopulate(ResourceProvider1 R1, ResourceProvider2 R2, IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) =>
		throw new NotImplementedException();
}

class RepopulatingApplicationResourceDescriptor<TRes, TStale> : ResourceDescriptor where TRes : notnull {
	readonly Symbol<TRes> Key;
	readonly IObservable<TStale> StaleObservable;
	readonly ResourceFactory2<TRes, TStale> Factory;

	TStale? Stale;
	bool Re;

	public RepopulatingApplicationResourceDescriptor(Symbol<TRes> key, IObservable<TStale> observable, ResourceFactory2<TRes, TStale> factory) {
		Key = key;
		StaleObservable = observable;
		Factory = factory;
	}

	public override void Populate(ResourceProvider1 R, IDeviceIndependentResourceDictionary applicationResources) {
		var firstValue = Factory.Invoke(R, default, default);

		applicationResources.UncheckedSet(Key, firstValue);
		applicationResources.UncheckedSet(new(),
			StaleObservable
				.Subscribe(stale => {
					Stale = stale;
					Re = true;

					applicationResources.Remove(Key);
				})
		);
	}

	public override void Populate(ResourceProvider2 R, IDeviceDependentResourceDictionary deviceResources) =>
		throw new NotImplementedException();

	public override void Repopulate(ResourceProvider1 R1, ResourceProvider2 R2, IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		if (Re) {
			Re = false;

			applicationResources.TryGet(Key, out var existingValue);

			var newValue = Factory.Invoke(R1, Stale, existingValue);

			if (!ReferenceEquals(existingValue, newValue))
				applicationResources.UncheckedSet(Key, newValue);
		}
	}
}

class DeviceResourceDescriptor<TRes> : ResourceDescriptor where TRes : notnull {
	readonly Symbol<TRes> Key;
	readonly ResourceFactory3<TRes> Factory;

	public DeviceResourceDescriptor(Symbol<TRes> key, ResourceFactory3<TRes> factory) {
		Key = key;
		Factory = factory;
	}

	public override void Populate(ResourceProvider1 R, IDeviceIndependentResourceDictionary applicationResources) =>
		throw new NotImplementedException();

	public override void Populate(ResourceProvider2 R, IDeviceDependentResourceDictionary deviceResources) =>
		deviceResources.UncheckedSet(Key, Factory.Invoke(R));

	public override void Repopulate(ResourceProvider1 R1, ResourceProvider2 R2, IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) =>
		throw new NotImplementedException();
}

class RepopulatingDeviceResourceDescriptor<TRes, TStale> : ResourceDescriptor where TRes : notnull {
	readonly Symbol<TRes> Key;
	readonly IObservable<TStale> StaleObservable;
	readonly ResourceFactory4<TRes, TStale> Factory;

	TStale? Stale;
	bool Re;

	public RepopulatingDeviceResourceDescriptor(Symbol<TRes> key, IObservable<TStale> observable, ResourceFactory4<TRes, TStale> factory) {
		Key = key;
		StaleObservable = observable;
		Factory = factory;
	}

	public override void Populate(ResourceProvider1 R, IDeviceIndependentResourceDictionary applicationResources) =>
		throw new NotImplementedException();

	public override void Populate(ResourceProvider2 R, IDeviceDependentResourceDictionary deviceResources) {
		var firstValue = Factory.Invoke(R, default, default);

		deviceResources.UncheckedSet(Key, firstValue);
		deviceResources.UncheckedSet(new(),
			StaleObservable
				.Subscribe(stale => {
					Stale = stale;
					Re = true;

					deviceResources.Remove(Key);
				})
		);
	}

	public override void Repopulate(ResourceProvider1 R1, ResourceProvider2 R2, IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		if (Re) {
			Re = false;

			deviceResources.TryGet(Key, out var existingValue);

			var newValue = Factory.Invoke(R2, Stale, existingValue);

			if (!ReferenceEquals(existingValue, newValue))
				deviceResources.UncheckedSet(Key, newValue);
		}
	}
}

delegate TRes ResourceFactory1<TRes>(ResourceProvider1 R) where TRes : notnull;
delegate TRes ResourceFactory2<TRes, TStale>(ResourceProvider1 R, TStale? stale, TRes? existing) where TRes : notnull;
delegate TRes ResourceFactory3<TRes>(ResourceProvider2 R) where TRes : notnull;
delegate TRes ResourceFactory4<TRes, TStale>(ResourceProvider2 R, TStale? stale, TRes? existing) where TRes : notnull;

readonly ref struct ResourceCollection {
	readonly List<ResourceDescriptor> ApplicationResources;
	readonly List<ResourceDescriptor> DeviceResources;
	readonly List<ResourceDescriptor> RepopulatingResources;
	readonly IScheduler Scheduler;

	public ResourceCollection(List<ResourceDescriptor> applicationResources, List<ResourceDescriptor> deviceResources, List<ResourceDescriptor> repopulatingResources, IScheduler scheduler) {
		ApplicationResources = applicationResources;
		DeviceResources = deviceResources;
		RepopulatingResources = repopulatingResources;
		Scheduler = scheduler;
	}

	public void Add1<TRes>(Symbol<TRes> key, ResourceFactory1<TRes> factory) where TRes : notnull =>
		new ApplicationResourceDescriptor<TRes>(key, factory)
			.AddTo(ApplicationResources);

	public void Add1<TStale, TRes>(Symbol<TRes> key, IObservable<TStale> staleObservable, ResourceFactory2<TRes, TStale> factory) where TRes : notnull =>
		new RepopulatingApplicationResourceDescriptor<TRes, TStale>(key, staleObservable.ObserveOn(Scheduler), factory)
			.AddTo(ApplicationResources)
			.AddTo(RepopulatingResources);

	public void Add2<TRes>(Symbol<TRes> key, ResourceFactory3<TRes> factory) where TRes : notnull =>
		new DeviceResourceDescriptor<TRes>(key, factory)
			.AddTo(DeviceResources);

	public void Add2<TStale, TRes>(Symbol<TRes> key, IObservable<TStale> staleObservable, ResourceFactory4<TRes, TStale> factory) where TRes : notnull =>
		new RepopulatingDeviceResourceDescriptor<TRes, TStale>(key, staleObservable.ObserveOn(Scheduler), factory)
			.AddTo(DeviceResources)
			.AddTo(RepopulatingResources);
}

static class ResourceCollectionExtensions {
	//public static void Add<TRes>(this ResourceCollection @this, Symbol<TRes> key, Func<TRes> factory) where TRes : notnull {}
	//public static void Add<TRes, TStale>(this ResourceCollection @this, Symbol<TRes> key, IObservable<TStale> staleObservable, Func<TStale, TRes> factory) where TRes : notnull {}
}

readonly ref struct ResourceProvider1 {
	readonly IDeviceIndependentResourceDictionary ApplicationResources;

	public ResourceProvider1(IDeviceIndependentResourceDictionary applicationResources) {
		ApplicationResources = applicationResources;
	}

	public ID2D1Factory7 D2Factory => ApplicationResources.D2Factory;
	public IWICImagingFactory2 WicFactory => ApplicationResources.WicFactory;
	public IDWriteFactory7 WriteFactory => ApplicationResources.WriteFactory;

	public TRes Get<TRes>(Symbol<TRes> key) where TRes : notnull =>
		ApplicationResources.Get(key);
}

readonly ref struct ResourceProvider2 {
	readonly IDeviceIndependentResourceDictionary ApplicationResources;
	readonly IDeviceDependentResourceDictionary DeviceResources;

	public ResourceProvider2(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		ApplicationResources = applicationResources;
		DeviceResources = deviceResources;
	}

	public ID2D1Factory7 D2Factory => ApplicationResources.D2Factory;
	public IWICImagingFactory2 WicFactory => ApplicationResources.WicFactory;
	public IDWriteFactory7 WriteFactory => ApplicationResources.WriteFactory;
	public ID2D1Device6 Device => DeviceResources.D2Device;
	public ID2D1DeviceContext6 Context => DeviceResources.D2DeviceContext;

	public TRes Get<TRes>(Symbol<TRes> key) where TRes : notnull =>
		DeviceResources.TryGet(key, out var value)
			? value
			: ApplicationResources.Get(key);
}

abstract class Game2 {
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
	public abstract void OnLoad(ResourceCollection R, CompositeDisposable disposables);

	/**
	<summary>
		<para>
			Physics time!
			Hot-reloadable!
		</para>
	</summary>
	**/
	public abstract void OnUpdate(ResourceProvider2 R);

	/**
	<summary>
		<para>
			Graphics time!
			Hot-reloadable!
		</para>
	</summary>
	**/
	public abstract void OnRender(ResourceProvider2 R);
}
