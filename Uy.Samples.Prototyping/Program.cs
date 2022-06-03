using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.Reactive.Linq;
using Uy;

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
	public MainWindowRootControl(IWindowBridge currentWindow, ILogger<MainWindowRootControl> logger) {
		currentWindow.Title = "Hello, there!";
		currentWindow.State = WindowState.Maximized;
		currentWindow.WindowClosing.Register(() => {
			logger.LogDebug("They came from.... Behind!");
		});
	}
}
