using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

using static Uy.Key;

Host
	.CreateDefaultBuilder(args)
	#if DEBUG
	.ConfigureLogging(logging => {
		logging.ClearProviders();
		logging.AddDebug();
		logging.AddFilter<DebugLoggerProvider>(logLevel => LogLevel.Debug <= logLevel);
		logging.SetMinimumLevel(LogLevel.None);
	})
	#endif
	.ConfigureServices(services => {
		services.AddUy<MainWindowRootContent>();
		services.AddTransient<MainWindowRootContent>();
	})
	.Build()
	.Run();

class MainWindowRootContent : IWindowRootContent, IDisposable {
	#region Disposable

	readonly CompositeDisposable Control = new();
	readonly CompositeDisposable Device = new();

	public void Dispose() {
		Device.Dispose();
		Control.Dispose();
	}

	public void OnDeviceDispose(DeviceDisposeInfo info) {
		Device.Clear();
	}

	#endregion

	readonly IWindowBridge CurrentWindow;
	Vector2 AllocatedSize;
	Box4 AllocatedBox;

	readonly IDWriteTextFormat3 Font;

	ID2D1SolidColorBrush? Color;

	int CounterValue;

	public MainWindowRootContent(IWindowBridge currentWindow, IDeviceIndependentResourceDictionary applicationResources) {
		CurrentWindow = currentWindow;

		CurrentWindow.ScaledSizeObservable
			.Subscribe(size => {
				AllocatedSize = size;
				AllocatedBox = Box4.FromLeftTop(Vector2.Zero, size);
			})
			.DisposeWith(Control);

		using (var font_tmp = applicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", 16))
			Font = font_tmp.QueryInterface<IDWriteTextFormat3>().DisposeWith(Control);
	}

	public void OnDeviceInit(DeviceInitInfo info) {
		var context = info.DeviceResources.D2DeviceContext;

		Color = context
			.CreateSolidColorBrush(Colors.Lime)
			.DisposeWith(Device);
	}

	public void OnKeyDown(KeyDownEvent @event) {
		if (@event.IsNotRepeated && @event.LayoutKey == KeyC) {
			@event.StopProcessing();
			++CounterValue;
			CurrentWindow.RequestRender();
		}
	}

	public void OnKeyUp(KeyUpEvent @event) {
	}

	public void OnRender(RenderInfo info) {
		var context = info.DeviceResources.D2DeviceContext;

		context.Clear(Colors.Black);
		context.DrawText(CounterValue.ToString("0\nPress C to count\\."), Font, AllocatedBox.Deflated(16).ToRect(), Color!);
	}
}

static class Box4Extensions {
	public static Rect ToRect(this Box4 @this) =>
		new(@this.Left, @this.Top, @this.Width, @this.Height);
}
