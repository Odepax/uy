using System;
using System.Collections.Generic;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.WIC;

namespace Uy;

class DeviceIndependentResourceDictionary : ResourceDictionary, IDeviceIndependentResourceDictionary, IDisposable {
	public ID2D1Factory7 D2Factory { get; }
	public IWICImagingFactory2 WicFactory { get; }
	public IDWriteFactory7 WriteFactory { get; }

	readonly IEnumerable<IDeviceIndependentResourceInitializer> ResourceInitializers;

	public DeviceIndependentResourceDictionary(IEnumerable<IDeviceIndependentResourceInitializer> resourceInitializers) {
		D2Factory = D2D1.D2D1CreateFactory<ID2D1Factory7>();
		WicFactory = new IWICImagingFactory2();
		WriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory7>();

		ResourceInitializers = resourceInitializers;

		foreach (var initializer in ResourceInitializers)
			initializer.OnInit(this);

	}

	public override void Dispose() {
		foreach (var initializer in ResourceInitializers)
			initializer.OnDispose(this);

		base.Dispose();

		D2Factory.Dispose();
		WicFactory.Dispose();
		WriteFactory.Dispose();
	}
}
