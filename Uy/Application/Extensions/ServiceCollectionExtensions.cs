using Microsoft.Extensions.DependencyInjection;

namespace Uy;

public static class ServiceCollectionExtensions {
	/**
	<summary>
		<para>
			Adds services required to run a <see cref="Uy"/> desktop application.
		</para>
	</summary>
	**/
	public static IServiceCollection AddUy<TMainWindowContent>(this IServiceCollection @this) where TMainWindowContent : class, IWindowRootContent {
		@this.AddHostedService<UyApplication<TMainWindowContent>>();
		@this.AddScoped<IWindowBridge, Win32WindowBridge>();
		@this.AddSingleton<IDeviceIndependentResourceDictionary, DeviceIndependentResourceDictionary>();

		return @this;
	}
}
