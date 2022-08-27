using LinqToYourDoom;
using System;
using System.Numerics;
using System.Reactive.Disposables;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Measure;

#pragma warning disable IDE0079
#pragma warning disable CS8618
#pragma warning disable IDE0044
#pragma warning disable IDE0060
class _SquareGame : Game {
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

	Vector2 CircleCenter = new(320, 320);
	float CircleRadius = 100;
	AngleF SquareDirection = MathD.Random.PiAngleF();
	float SquareSide = 30;
	Vector2 SquareCenter;

	public override void OnUpdate() {
		var _updateStartTime = Clock.ElapsedMilliseconds;
		{
			SquareDirection += Stick1.Vector.X * 0.33f * T / S * Clock.SPF;
			SquareCenter = CircleCenter.WithAngularOffset(SquareDirection, CircleRadius + (SquareDirection.Cos.Abs() + SquareDirection.Sin.Abs()) * SquareSide / 2);
		}
		var _updateEndTime = Clock.ElapsedMilliseconds;

		UpdateDuration = _updateEndTime - _updateStartTime;
	}

	#endregion
	#region Drawing time

	public override void OnRender(IDeviceIndependentResourceDictionary applicationResources, IDeviceDependentResourceDictionary deviceResources) {
		var context = deviceResources.D2DeviceContext;

		context.Clear(Colors.Black);
		{
			context.DrawEllipse(new Ellipse(CircleCenter, CircleRadius, CircleRadius), TextColor, 2);
			context.DrawRectangle(Box4.FromCenter(SquareCenter, new(SquareSide, SquareSide)).ToRect(), TextColor, 2);
		}
		context.DrawText($"{ Clock.FPS :0} FPS - Update: { UpdateDuration :00.00}ms", Font, Window.Box.Deflated(16).ToRect(), TextColor);
	}

	#endregion
}
