using System.Reactive.Disposables;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Measure;

#pragma warning disable CS8618
#pragma warning disable IDE0044
#pragma warning disable IDE0060
class Game {
	public Window Window = new();
	public Clock Clock = new();
	public Stick Stick1 = new();
	public Stick Stick2 = new();

	#region Physics resources

	public void OnLoad() {
		// ...
	}

	#endregion
	#region Graphics resources - Device-independent

	IDWriteTextFormat3 Font;

	public void OnApplicationInit(IDeviceIndependentResourceDictionary applicationResources, CompositeDisposable disposables) {
		using (var font = applicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", 16))
			Font = font.QueryInterface<IDWriteTextFormat3>().DisposeWith(disposables);
	}

	#endregion
	#region Graphics resources - Device-dependent

	ID2D1SolidColorBrush TextColor;

	public void OnDeviceInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		var context = deviceResources.D2DeviceContext;

		TextColor = context.CreateSolidColorBrush(Colors.White).DisposeWith(disposables);
	}

	#endregion
	#region Graphics resources - Window size-dependent

	ID2D1BitmapRenderTarget Offscreen;

	public void OnResizeInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		var context = deviceResources.D2DeviceContext;

		Offscreen = context.CreateCompatibleRenderTarget();
	}

	#endregion
	#region Physics time

	double UpdateDuration;

	public void OnUpdate() {
		var _updateStartTime = Clock.ElapsedMilliseconds;
		{
			// ...
		}
		var _updateEndTime = Clock.ElapsedMilliseconds;

		UpdateDuration = _updateEndTime - _updateStartTime;
	}

	#endregion
	#region Drawing time

	public void OnRender(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		var context = deviceResources.D2DeviceContext;

		context.Clear(Colors.Black);
		{
			// ...
		}
		context.DrawText($"{ Clock.FPS :0} FPS - Update: { UpdateDuration :00.00}ms", Font, Window.Box.Deflated(16).ToRect(), TextColor);
	}

	#endregion
}
