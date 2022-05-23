using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
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
		currentWindow.Title = "Hello, title!";
		currentWindow.State = WindowState.Maximized;
		currentWindow.WindowClosing.Register(() => {
			logger.LogDebug("No! No! I don't want to die!..");
		});
	}
}


//using Windows.ApplicationModel.Core;
//using Windows.UI.Core;

//class Program : IFrameworkViewSource, IFrameworkView {
//	[System.MTAThread]
//    static void Main() {
//        CoreApplication.Run(new Program());
//    }

//    public IFrameworkView CreateView() => this;

//    public void Initialize(CoreApplicationView applicationView) { }
//    public void Load(string entryPoint) { }

//    public void Run() {
//        CoreWindow window = CoreWindow.GetForCurrentThread();
//        window.Activate();

//        CoreDispatcher dispatcher = window.Dispatcher;
//        dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
//    }

//    public void SetWindow(CoreWindow window) { }
//    public void Uninitialize() { }
//}