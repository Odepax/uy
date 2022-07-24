using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Uy;

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
		services.AddUy<GameWindowRootContent>();
		services.AddTransient<GameWindowRootContent>();
		services.AddTransient<Game>();
	})
	.Build()
	.Run();
