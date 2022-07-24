using LinqToYourDoom;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Uy;

readonly struct ResourceScope : IDisposable {
	readonly CompositeDisposable Disposables = new();
	readonly Action<IDeviceIndependentResourceDictionary, IDeviceDependentResourceDictionary, CompositeDisposable> Initialize_delegate;

	public ResourceScope(Action<IDeviceIndependentResourceDictionary, IDeviceDependentResourceDictionary, CompositeDisposable> initialize) : this() => Initialize_delegate = initialize;

	public void Initialize(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		if (Disposables.IsEmpty())
			Initialize_delegate.Invoke(applicationResources, deviceResources, Disposables);
	}

	public void Clear() => Disposables.Clear();
	public void Dispose() => Disposables.Dispose();
}
