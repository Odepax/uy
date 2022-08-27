using LinqToYourDoom;
using System;
using System.Numerics;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Measure;

#pragma warning disable IDE0079
#pragma warning disable CS8618
#pragma warning disable IDE0044
#pragma warning disable IDE0060
class _KeyGame : Game {
	#region Physics resources

	public override void OnLoad(CompositeDisposable disposables) {
	}

	#endregion
	#region Graphics resources - Device-independent

	IDWriteTextFormat3 Font;

	public override void OnApplicationInit(IDeviceIndependentResourceDictionary applicationResources, CompositeDisposable disposables) {
		using (var font = applicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", 16))
			Font = font.QueryInterface<IDWriteTextFormat3>().DisposeWith(disposables);
	}

	#endregion
	#region Graphics resources - Device-dependent

	ID2D1SolidColorBrush TextColor;

	public override void OnDeviceInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		var context = deviceResources.D2DeviceContext;

		TextColor = context.CreateSolidColorBrush(Colors.White).DisposeWith(disposables);
	}

	#endregion
	#region Graphics resources - Window size-dependent

	//ID2D1BitmapRenderTarget Offscreen;

	public override void OnResizeInit(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources, CompositeDisposable disposables) {
		//var context = deviceResources.D2DeviceContext;

		//Offscreen = context.CreateCompatibleRenderTarget().DisposeWith(disposables);
	}

	#endregion
	#region Physics time

	double UpdateDuration;

	public override void OnUpdate() {
		var _updateStartTime = Clock.ElapsedMilliseconds;
		{
		}
		var _updateEndTime = Clock.ElapsedMilliseconds;

		UpdateDuration = _updateEndTime - _updateStartTime;
	}

	#endregion
	#region Drawing time

	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	static extern int GetKeyNameText(int lParam, [Out] StringBuilder lpString, int nSize);
	static string GetKeyName(Key key) {
		var scanCode = key switch {
			Key.KeyW => 17,
			Key.KeyA => 30,
			Key.KeyS => 31,
			Key.KeyD => 32,

			_ => throw new NotImplementedException()
		};

		var builder = new StringBuilder(4, 4);

		return GetKeyNameText(scanCode << 16, builder, builder.Capacity) == 0
				? "?"
				: builder.ToString();
	}

	public override void OnRender(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		var context = deviceResources.D2DeviceContext;

		context.Clear(Colors.Black);
		{
			var WASD = GetKeyName(Key.KeyW) + GetKeyName(Key.KeyA) + GetKeyName(Key.KeyS) + GetKeyName(Key.KeyD);

			context.DrawText($"WASD = { WASD }", Font, Window.Box.Deflated(96).ToRect(), TextColor);
		}
		context.DrawText($"{ Clock.FPS :0} FPS - Update: { UpdateDuration :00.00}ms", Font, Window.Box.Deflated(16).ToRect(), TextColor);
	}

	#endregion
}
