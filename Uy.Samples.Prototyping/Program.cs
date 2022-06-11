using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System.Numerics;
using Uy;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using WindowState = Uy.WindowState;

Host
	.CreateDefaultBuilder(args)
	.ConfigureLogging(logging => {
		logging.ClearProviders();
		logging.AddDebug();
		logging.AddFilter<DebugLoggerProvider>(logLevel => LogLevel.Debug <= logLevel);
		logging.SetMinimumLevel(LogLevel.None);
	})
	.ConfigureServices(services => {
		services.AddUy<MainWindowRootControl>();
		services.AddTransient<MainWindowRootControl>();
	})
	.Build()
	.Run();

class MainWindowRootControl : IWindowRootContent {
	readonly IDeviceIndependentResourceDictionary ApplicationResources;
	readonly IWindowBridge CurrentWindow;

	readonly IDWriteTextLayout4 TextLayout;

	ID2D1SolidColorBrush? Background;
	ID2D1SolidColorBrush? Foreground;

	public MainWindowRootControl(IDeviceIndependentResourceDictionary applicationResources, IWindowBridge currentWindow, ILogger<MainWindowRootControl> logger) {
		ApplicationResources = applicationResources;
		CurrentWindow = currentWindow;

		CurrentWindow.Title = "Hello, there!";
		CurrentWindow.State = WindowState.Restored;
		CurrentWindow.WindowClosing.Register(() => {
			logger.LogDebug("They came from.... Behind!");
		});

		using var textFormat = ApplicationResources.WriteFactory.CreateTextFormat("JetBrains Mono", FontWeight.Medium, FontStyle.Normal, 16);
		using var textLayout = ApplicationResources.WriteFactory.CreateTextLayout("Hello, there!", textFormat, float.MaxValue, float.MaxValue);

		TextLayout = textLayout.QueryInterface<IDWriteTextLayout4>();
	}

	public void OnDeviceInit(DeviceInitInfo info) {
		Background = info.DeviceResources.D2DeviceContext.CreateSolidColorBrush(Colors.Gold);
		Foreground = info.DeviceResources.D2DeviceContext.CreateSolidColorBrush(Colors.Black);
	}

	public void OnDeviceDispose(DeviceDisposeInfo info) {
		Background?.Dispose();
		Foreground?.Dispose();

		Background = null;
		Foreground = null;
	}

	public void OnRender(RenderInfo info) {
		var allocatedSize = CurrentWindow.ScaledSize;
		var allocatedBox = Box4.FromLeftTop(Vector2.Zero, allocatedSize);
		var context = info.DeviceResources.D2DeviceContext;

		var rectBox = allocatedBox.Deflated(32);
		var rect = new Rect(rectBox.LeftTop, new Size(rectBox.Size));
		var textBox = rectBox.Deflated(32).Normalized();

		TextLayout.MaxWidth = textBox.Width;
		TextLayout.MaxHeight = textBox.Height;

		var textSize = new Vector2(TextLayout.Metrics.Width, TextLayout.Metrics.Height);

		context.Clear(Colors.Fuchsia);
		context.FillRectangle(rect, Background!);
		context.DrawTextLayout(Box4.FromCenter(allocatedBox.CenterAnchor, textSize).LeftTop, TextLayout, Foreground!);
	}
}
