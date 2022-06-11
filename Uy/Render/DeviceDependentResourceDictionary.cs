using System;
using Vortice.Direct2D1;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Uy;

class DeviceDependentResourceDictionary : ResourceDictionary, IDeviceDependentResourceDictionary, IDisposable {
	#pragma warning disable CS8766
	// > Nullability of reference types in return type
	// > doesn't match implicitly implemented member (possibly because of nullability attributes).
	//
	// The Win32 window will initialize the device context when it needs to be initialized.

	public ID3D11Device5? D3Device;
	public ID3D11DeviceContext4? D3DeviceContext;
	public IDXGISwapChain4? SwapChain;
	public IDXGISurface2? BackBuffer;
	public ID2D1Bitmap1? D2RenderTarget;

	public ID2D1Device6? D2Device { get; set; }
	public ID2D1DeviceContext6? D2DeviceContext { get; set; }

	#pragma warning restore CS8766

	/**
	<remarks>
		<para>
			<see cref="Dispose"/> is idempotent.
		</para>
	</remarks>
	**/
	public override void Dispose() {
		base.Dispose();

		D2DeviceContext?.Dispose();
		D2Device?.Dispose();

		D2RenderTarget?.Dispose();
		BackBuffer?.Dispose();
		SwapChain?.Dispose();
		D3DeviceContext?.Dispose();
		D3Device?.Dispose();

		D2DeviceContext = null;
		D2Device= null;

		D2RenderTarget = null;
		BackBuffer = null;
		SwapChain = null;
		D3DeviceContext = null;
		D3Device = null;
	}
}
