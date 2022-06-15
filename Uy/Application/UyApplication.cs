using LinqToYourDoom;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Uy;

/**
<summary>
	<para>
		Encapsulates the the bootstrapping of a <see cref="Uy"/> application and its assets.
	</para>
</summary>
**/
class UyApplication<TMainWindowContent> : BackgroundService where TMainWindowContent : IWindowRootContent {
	readonly IHostApplicationLifetime ApplicationLifetime;
	readonly IServiceProvider ServiceProvider;

	public UyApplication(IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider) {
		ApplicationLifetime = applicationLifetime;
		ServiceProvider = serviceProvider;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken) {
		// Starting a long-running task in order to not block the current thread-pool thread.
		return Task.Factory.StartNew(() => {
			using var application = new Win32Application(ServiceProvider);

			application.OpenWindow(typeof(TMainWindowContent));
			application.RunGameLoop(stoppingToken);

			// If we got there, but not because the host application is shutting down,
			// it means the game loop stopped with WM_QUIT,
			// which is a signal to shutdown the host application.
			if (stoppingToken.IsCancellationRequested.Nt())
				ApplicationLifetime.StopApplication();
		}, TaskCreationOptions.LongRunning);
	}
}
