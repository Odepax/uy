using LinqToYourDoom;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.Graphics.Gdi.MONITOR_FROM_FLAGS;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.HiDpi.PROCESS_DPI_AWARENESS;
using static Windows.Win32.UI.WindowsAndMessaging.PEEK_MESSAGE_REMOVE_TYPE;
using static Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_EX_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using D3FeatureLevel = Vortice.Direct3D.FeatureLevel;

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
			// which is a signal to close the host application.
			if (stoppingToken.IsCancellationRequested.Nt())
				ApplicationLifetime.StopApplication();
		}, TaskCreationOptions.LongRunning);
	}
}

/**
<summary>
	<para>
		Encapsulates the state and the handle of a <see cref="Vortice.Direct2D1"/>/<see cref="Windows.Win32"/> application,
		holds the list of opened windows,
		runs the game loop.
	</para>
</summary>
<remarks>
	<para>
		This class is <b>not to be registered with the dependecy injection container.</b>
		This is a DI wrapper!
	</para>
</remarks>
**/
class Win32Application : IDisposable {
	internal readonly IServiceProvider ServiceProvider;
	internal readonly FreeLibrarySafeHandle InstanceHandle;
	internal readonly DeviceIndependentResourceDictionary Resources;

	readonly ConcurrentDictionary<HWND, Win32Window> Windows = new();
	readonly ILogger<Win32Application> Logger;

	int MainWindowCount;

	/**
	<summary>
		<para>
			Initializes a Win32 application.
		</para>
	</summary>
	**/
	public Win32Application(IServiceProvider serviceProvider) {
		ServiceProvider = serviceProvider;
		InstanceHandle = GetModuleHandle(null as string) ?? throw new Bug("6ADC2291-23FF-4829-82BE-666FBAD0961F"); // NULL => Unable to get the Win32 instance handle.
		Logger = ServiceProvider.GetRequiredService<ILogger<Win32Application>>();

		InitializeDpiAwareness();
		RegisterWin32WindowClass();
		InitializeDirectXDeviceIndependentResources(out Resources);
	}

	/**
	<summary>
		<para>
			Closes and destroys all Win32 windows managed by this Win32 application.
		</para>
	</summary>
	**/
	public void Dispose() {
		Windows
			.DisposeAll(kvp => kvp.Value)
			.Clear();

		DisposeDirectXDeviceIndependentResources();
		UnregisterWin32WindowClass();

		InstanceHandle.Close();
		InstanceHandle.Dispose();

		_ = ServiceProvider; // Not the application's job to dispose of the service provider.
	}

	public void OpenWindow(Type windowRootContentType) {
		_ = new Win32Window(this, windowRootContentType);
	}

	internal void RegisterWindow(Win32Window window) {
		if (Windows.TryAdd(window.WindowHandle, window).Nt())
			throw new Bug("2FDB1820-D621-4DF8-995A-5368DFF02774");

		if (window.IsMainWindow)
			Interlocked.Increment(ref MainWindowCount);
	}

	public void CloseWindow(Win32Window window) {
		// From https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroywindow:
		//
		// > Destroys the specified window. The function sends WM_DESTROY and WM_NCDESTROY messages
		// > to the window to deactivate it and remove the keyboard focus from it.
		// > The function also destroys the window's menu, flushes the thread message queue,
		// > destroys timers, removes clipboard ownership, and breaks the clipboard viewer chain
		// > if the window is at the top of the viewer chain.
		//
		// > A thread cannot use DestroyWindow to destroy a window created by a different thread!
		DestroyWindow(window.WindowHandle);
	}

	public void RunGameLoop(CancellationToken stoppingToken) {
		Logger.LogDebug("Entering game loop.");

		var lastTimestamp = 0L;
		var clockStopwatch = Stopwatch.StartNew();

		while (stoppingToken.IsCancellationRequested.Nt()) {
			// OS messages.
			MSG msg;

			while (PeekMessage(out msg, default, 0, 0, PM_REMOVE)) {
				TranslateMessage(msg);
				DispatchMessage(msg);

				// From https://gamedev.stackexchange.com/questions/59857/game-loop-on-windows:
				//
				// > PeekMessage will only ever return WM_QUIT once the message queue is empty,
				// > therefore guaranteeing that the last message is in fact WM_QUIT.
				//
				// The author of the comment cites https://docs.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues.
				//
				// This removes the need for a duplicate "if (message == WM_QUIT || stoppingToken) break"
				// in the nested "while (PeekMessage)" loop.
				//
				// Well, I may be cursed, but I still get messages after WM_QUIT.
				// After thorough audit, I'm sorry to announce that my ass is still not chicken...
				if (msg.message == WM_QUIT || stoppingToken.IsCancellationRequested)
					break;
			}

			if (msg.message == WM_QUIT || stoppingToken.IsCancellationRequested)
				break;

			// Clock update.
			var timestamp = clockStopwatch.ElapsedMilliseconds;
			var secondsSinceLastTick = (timestamp - lastTimestamp) / 1_000f;
			var secondsSinceFirstTick = (lastTimestamp = timestamp) / 1_000f;

			_ = new GameLoopUpdateInfo(secondsSinceFirstTick, secondsSinceLastTick);

			// Render.
			foreach (var window in Windows.Values)
				window.RunRenderPass();
		}

		clockStopwatch.Stop();

		Logger.LogDebug("Exited game loop.");
	}

	#region DirectX stuff

	void InitializeDirectXDeviceIndependentResources(out DeviceIndependentResourceDictionary resources) {
		Logger.LogDebug("Initializing DirectX device-independent resources.");

		resources = ServiceProvider.GetRequiredService<IDeviceIndependentResourceDictionary>().As<DeviceIndependentResourceDictionary>();
	}

	void DisposeDirectXDeviceIndependentResources() {
		Logger.LogDebug("Disposing of DirectX device-independent resources.");

		Resources.Dispose();
	}

	#endregion
	#region Win32 stuff

	// https://stackoverflow.com/a/64419394/16501294
	//
	// > The WndProc delegate passed to unmanaged code was being freed by the GC.
	// > The simple fix was to keep a reference
	// > to the WndProc delegate passed into the window class.
	WNDPROC? Win32WindProcAntiGcReference;

	void InitializeDpiAwareness() {
		// Stolen from https://github.com/AvaloniaUI/Avalonia:
		//
		// > Ideally we'd set DPI awareness in the manifest,
		// > but this doesn't work for netcoreapp2.0 apps,
		// > as they are actually dlls run by a console loader.
		// > Instead we have to do it in code,
		// > but there are various ways to do this depending on the OS version.
		Logger.LogDebug("Setting DPI awareness.");

		var result = SetProcessDpiAwareness(PROCESS_PER_MONITOR_DPI_AWARE);

		if (result == HRESULT.E_INVALIDARG)
			throw new Bug("43D75995-3D96-451D-A223-919696EFA2B9"); // The value passed in is not valid.

		else if (result == HRESULT.E_ACCESSDENIED)
			Logger.LogWarning("The DPI awareness was already set, either by calling this API previously, or through the application manifest.");
	}

	/**
	<remarks>
		<para>
			The window class is a flyweight!
			All window must be associated with a window class.
			The class' window procedure will be called for all messages of all windows of this class.
		</para>
		<para>
			If introducing modals, modeless, and dialogs,
			it might be interesting to refactor different classes into constants.
		</para>
	</remarks>
	**/
	unsafe void RegisterWin32WindowClass() {
		Logger.LogDebug("Registering Win32 window class.");

		fixed (char* windowClassName = Win32Window.Win32WindowClassName) {
			var windowClass = new WNDCLASSEXW {
				cbSize = (uint) Unsafe.SizeOf<WNDCLASSEXW>(),
				hInstance = (HINSTANCE) InstanceHandle.DangerousGetHandle(),
				lpszClassName = windowClassName,
				lpfnWndProc = Win32WindProcAntiGcReference = ProcessWin32OsMessage,

				// From https://docs.microsoft.com/en-us/windows/win32/gdi/resized-windows:
				//
				// > The system invalidates only the newly exposed portion of the window [...].
				// > In this case, WM_PAINT is not generated when the size of the window is reduced.
				//
				// > An application must specify the CS_VREDRAW or CS_HREDRAW style,
				// > or both, when registering the window class.
				style = CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS | CS_OWNDC,

				hIcon = default,
				hbrBackground = default,
				hCursor = LoadCursor(default, IDC_ARROW),
			};

			var windowClassRegistrationAtom = RegisterClassEx(windowClass);

			if (windowClassRegistrationAtom == 0) // Unable to register window class.
				throw new Bug("E23FBF90-83F5-4992-B724-1410808ACDEF-0x" + Marshal.GetLastWin32Error().ToString("X"));
		}
	}

	void UnregisterWin32WindowClass() {
		Logger.LogDebug("Unregistering Win32 window class.");

		UnregisterClass(Win32Window.Win32WindowClassName, InstanceHandle);

		Win32WindProcAntiGcReference = null;
	}

	LRESULT ProcessWin32OsMessage(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam) {
		switch (message) {
			// From https://docs.microsoft.com/en-us/windows/win32/winmsg/window-features#window-destruction:
			//
			// - Clicking "close" sends WM_CLOSE.
			// - Process the WM_CLOSE message to confirm with the user before calling DestroyWindow().
			//   E.g at https://docs.microsoft.com/en-us/windows/win32/learnwin32/closing-the-window.
			//
			// From https://stackoverflow.com/a/3155879/16501294:
			//
			// + WM_CLOSE is sent to the window when it is being closed, i.e.
			//
			//   - When its close button is clicked;
			//   - When "Close" is chosen from the window's menu;
			//   - When Alt-F4 is pressed while the window has focus.
			//
			//   If you catch this message, this is your decision how to treat it:
			//   ignore it, or really close the window. By default, WM_CLOSE is passed
			//   to DefWindowProc(), which causes the window to be destroyed.
			//
			// + WM_DESTROY is sent to the window when it starts to be destroyed.
			//   In this stage, in opposition to WM_CLOSE, you cannot stop the process,
			//   you can only make any necessary cleanup.
			//
			//   When you catch WM_DESTROY, none of its child windows have been destroyed yet.
			//
			// + WM_QUIT is not related to any window, i.e. the hwnd of the message is NULL
			//   and no window procedure is called.
			//
			//   This message indicates that the message loop should be stopped,
			//   and the application should exit.
			case WM_CLOSE: {
				Logger.LogDebug("WM_CLOSE for {W}.", hWnd);

				return DefWindowProc(hWnd, message, wParam, lParam);
			}
			case WM_DESTROY: {
				Logger.LogDebug("WM_DESTROY for {W}.", hWnd);

				if (Windows.TryRemove(hWnd, out var window))
					window.Dispose();

				else throw new Bug("44018E69-84C4-453F-A930-8B8B2A643E20");

				if (window.IsMainWindow && Interlocked.Decrement(ref MainWindowCount) == 0)
					PostQuitMessage(0);

				break;
			}
			case WM_QUIT: {
				Logger.LogDebug("WM_QUIT for {W}.", hWnd);

				return DefWindowProc(hWnd, message, wParam, lParam);
			}

			//case WM_ACTIVATEAPP: {
			//	Logger.LogDebug("WM_ACTIVATEAPP for {W}.", hWnd);

			//	//if (wParam != 0) Application.Current?.OnActivated();
			//	//else Application.Current?.OnDeactivated();

			//	break;
			//}

			// The message was NONE of the above.
			default: {
				// Some messages will be processed at the application-level,
				// they will have an intentionally NULL hWnd parameter.
				if (hWnd.Value == 0)
					// Application-level messages should already have been processed by now...
					return DefWindowProc(hWnd, message, wParam, lParam);

				// Some messages will be addressed to specific windows.
				else if (Windows.TryGetValue(hWnd, out var window))
					return window.ProcessWin32OsMessage(message, wParam, lParam);

				else {
					Logger.LogWarning("No window {HWND} found to process Win32 message {MSG}!", hWnd.Value, message);

					return DefWindowProc(hWnd, message, wParam, lParam);
				}
			}
		}

		// The message WAS one of the above.
		return new LRESULT(0);
	}

	#endregion
}

/**
<summary>
	<para>
		Encapsulates the state and the handle of a <see cref="Vortice.Direct2D1"/>/<see cref="Windows.Win32"/> window,
		holds the dependency injection scope.
	</para>
</summary>
<remarks>
	<para>
		This class is <b>not to be registered with the dependecy injection container.</b>
		This is a DI wrapper!
	</para>
</remarks>
	**/
class Win32Window : IDisposable {
	public readonly bool IsMainWindow = true;

	readonly Win32Application Application;
	readonly IServiceScope ServiceScope;
	readonly Win32WindowBridge Bridge;
	readonly ILogger<Win32Window> Logger;
	readonly DeviceDependentResourceDictionary DeviceResources = new();
	readonly IEnumerable<IDeviceDependentResourceInitializer> DeviceResourceInitializers;
	readonly IWindowRootContent RootContent;

	readonly IDisposable Title_subscription;
	readonly IDisposable State_subscription;
	readonly IDisposable Size_subscription;

	bool RenderIsStale = true;

	/**
	<summary>
		<para>
			Opens a Win32 window, and registers it with the scoped <see cref="IWindowBridge"/>.
		</para>
	</summary>
	**/
	public Win32Window(Win32Application application, Type windowRootContentType) {
		Application = application;
		ServiceScope = Application.ServiceProvider.CreateScope();
		Bridge = ServiceScope.ServiceProvider.GetRequiredService<IWindowBridge>().As<Win32WindowBridge>();
		Logger = ServiceScope.ServiceProvider.GetRequiredService<ILogger<Win32Window>>();

		// First thing first: before we forget about it,
		// we're going to register this Win32 window with the scoped window bridge!
		InitializeWindowBridge(out Title_subscription, out State_subscription, out Size_subscription);

		// Second thing is to create the Win32 window, of course.
		CreateWin32Window(out WindowHandle);

		// We need to get back to the application and register the window,
		// otherwise, we'll miss important OS messages at the time we show the window,
		// as the application will not be able to to route the messages
		// to a window it doesn't know about...
		Application.RegisterWindow(this);

		// Then, we show the window; at this point, the window is visible to the user.
		ShowWin32Window();

		DeviceResourceInitializers = ServiceScope.ServiceProvider.GetServices<IDeviceDependentResourceInitializer>();

		// And only then, we can call in the root content.
		// It has to be initialized after the bridge and the window,
		// as the root content could get injected with the IWindowBridge,
		// so this one has to be ready!
		RootContent = (IWindowRootContent) ServiceScope.ServiceProvider.GetRequiredService(windowRootContentType);

		Bridge.WindowOpened.Cancel();
	}

	void InitializeWindowBridge(out IDisposable title_subscription, out IDisposable state_subscription, out IDisposable size_subscription) {
		Logger.LogDebug("Initializing window bridge.");

		Bridge.LinkedWindow = this;

		// - Developers can set the state programatically,
		//   in which case we update the Win32 window.
		//
		// - Users can set the state by interacting with the window,
		//   in which case the Win32 window will be updated by the system,
		//   and we just need to reflect that in the bridge.
		//
		// In order for it to work well,
		// we'll consider WM_SIZE's reason parameter to be the source of truth.
		// User interactions will be reported directly by this message,
		// in which case we just have to push to the Bridge.State_observable.
		//
		// Programatic state changes will be reported by a separate Bridge.State_observer.
		// Changes will trigger calls to ShowWindow(), which will trigger WM_SIZE,
		// which allows us to close the loop.
		//
		//                                                 ,----------------------.       ,--------------.
		//                                      ,--------> | Bridge.State_subject | ----> | Bridge.State |
		//                                     /    ,----> |  = Subject.Create()  |       `--------------'
		//                                    /     \      `----------------------'
		//                                   /       \
		//                                  /         `------------------------------------------------.
		//                                 /                                                            \
		// ,-----------------------.      /     ,-------------------------.       ,--------------.       \
		// | Bridge.State_observer | ----'----> | .DistinctUntilChanged() | ----> | ShowWindow() |        \
		// `-----------------------'            `-------------------------'       `--------------'         \
		//                                                                               *                  \
		//       ,-----------------------------------------------------------------------'                   \
		//      v                                                                                             \
		// ,---------.       ,--------.       ,-------------------------.       ,-------------------------.    \
		// | WM_SIZE | ----> | Reason | ----> | Bridge.State_observable | ----> | .DistinctUntilChanged() | ----`
		// `---------'       `--------'       `-------------------------'       `-------------------------'
		Bridge.StateSubject_observer
			.StartWith(WindowState.Restored)
			.DistinctUntilChanged()
			.Buffer(2, 1)
			.Subscribe(states => OnBridgeState_programatic(states[0], states[1]))
			.Tee(out state_subscription);

		// - Developers can set the title,
		//   in which case we need to update the Win32 window's title.
		Bridge.TitleSubject
			.Subscribe(title => SetWindowText(WindowHandle, title))
			.Tee(out title_subscription);

		// - The OS can set the DPI.
		// - Developers can set the zoom.
		// - Users can set the size.
		//
		// In all those cases, we need to:
		//
		// - Recompute the DPI scale.
		// - Recompute the scaled size.
		// - Invalidate the current render.
		Observable
			.CombineLatest(
				Bridge.DpiSubject,
				Bridge.ZoomSubject,
				Bridge.HardwareSizeSubject
					.Do(ClearDirectXStateDependentResources),
				(dpiScale, zoom, hardwareSize) => (dpiScale, zoom, hardwareSize)
			)
			.DistinctUntilChanged()
			.Subscribe(OnBridgeSize)
			.Tee(out size_subscription);
	}

	/**
	<summary>
		<para>
			Closes and destroys a Win32 window, and disposes of the associated <see cref="ServiceScope"/>.
		</para>
	</summary>
	**/
	public void Dispose() {
		Bridge.WindowClosing.Cancel();

		_ = RootContent; // Will be disposed along with the service scope.

		State_subscription.Dispose();
		Title_subscription.Dispose();
		Size_subscription.Dispose();

		ClearDirectXStateDependentResources();
		DisposeDirectXDeviceDependentResources();

		DestroyWin32Window();

		Bridge.LinkedWindow = null;
		Bridge.WindowClosed.Cancel();

		_ = Application; // Not the window's job to dispose of the application.
		ServiceScope.Dispose();
		_ = Bridge; // Already disposed with the service scope.
	}

	public void RequestRender() {
		Logger.LogDebug("Requesting next window {W} render pass.", WindowHandle);

		RenderIsStale = true;
	}

	public void RunRenderPass() {
		if (RenderIsStale) {
			Logger.LogDebug("Running window {W} render pass.", WindowHandle);

			RenderIsStale = false;

			var hardwareSize = Bridge.HardwareSize;

			if (hardwareSize.X <= 0 || hardwareSize.Y <= 0)
				return;

			if (DirectXDeviceResourcesAreUninitialized)
				InitializeDirectXDeviceDependentResources();

			if (DirectXStateResourcesAreUninitialized)
				InitializeDirectXStateDependentResources();

			InvokeDirectXRender(hardwareSize);
		}
	}

	#region DirectX stuff

	bool DirectXDeviceResourcesAreUninitialized = true;
	bool DirectXStateResourcesAreUninitialized = true;

	/**
	<summary>
		<para>
			Initializes DirectX resources that are dependent on the device.
		</para>
	</summary>
	**/
	void InitializeDirectXDeviceDependentResources() {
		Logger.LogDebug("Initializing DirectX device-dependent resources.");

		DirectXDeviceResourcesAreUninitialized = false;

		// First, create the Direct3D device.
		D3D11.D3D11CreateDevice(
			IntPtr.Zero, // Specify nullptr to use the default adapter.
			DriverType.Hardware,
			DeviceCreationFlags.BgraSupport // This flag is required in order to enable compatibility with Direct2D.
			#if DEBUG
				| DeviceCreationFlags.Debug // If the project is in a debug build, enable debugging via SDK Layers with this flag.
			#endif
			,
			new[] { // This array defines the ordering of feature levels that D3D should attempt to create.
				D3FeatureLevel.Level_11_1,
				D3FeatureLevel.Level_11_0,
				D3FeatureLevel.Level_10_1,
				D3FeatureLevel.Level_10_0,
				D3FeatureLevel.Level_9_3,
				D3FeatureLevel.Level_9_1,
			},
			out var d3Device,
			out var d3DeviceContext
		);

		// Retrieve the Direct3D 11.1 interfaces.
		DeviceResources.D3Device = d3Device.QueryInterface<ID3D11Device5>();
		DeviceResources.D3DeviceContext = d3DeviceContext.QueryInterface<ID3D11DeviceContext4>();

		d3Device.Dispose();
		d3DeviceContext.Dispose();

		using var dxgiDevice = DeviceResources.D3Device.QueryInterface<IDXGIDevice4>();

		DeviceResources.D2Device = Application.Resources.D2Factory.CreateDevice(dxgiDevice);
		DeviceResources.D2DeviceContext = DeviceResources.D2Device.CreateDeviceContext(DeviceContextOptions.None);

		foreach (var initializer in DeviceResourceInitializers)
			initializer.OnDeviceInit(Application.Resources, DeviceResources);

		RootContent.OnDeviceInit(new DeviceInitInfo(Application.Resources, DeviceResources));
	}

	void DisposeDirectXDeviceDependentResources() {
		Logger.LogDebug("Disposing of DirectX device-dependent resources.");

		DirectXDeviceResourcesAreUninitialized = true;

		RootContent.OnDeviceDispose(new DeviceDisposeInfo(Application.Resources, DeviceResources));

		foreach (var initializer in DeviceResourceInitializers)
			initializer.OnDeviceDispose(Application.Resources, DeviceResources);

		DeviceResources.Dispose();
	}

	/**
	<summary>
		<para>
			Initializes DirectX resources that are dependent on the window size.
		</para>
	</summary>
	**/
	void InitializeDirectXStateDependentResources(bool nested = false) {
		Logger.LogDebug("Initializing DirectX state-dependent resources.");

		DirectXStateResourcesAreUninitialized = false;

		// If the swap chain already exists, resize it.
		if (DeviceResources.SwapChain != null) {
			// Setting all values to 0 automatically chooses the width & height to match the client rect for HWNDs,
			// and preserves the existing buffer count and format.
			var result = DeviceResources.SwapChain.ResizeBuffers(0, 0, 0);

			if (result == Vortice.DXGI.ResultCode.DeviceRemoved || result == Vortice.DXGI.ResultCode.DeviceReset) {
				if (nested)
					throw new Bug("1FC09E31-EEA4-466F-840A-4F74445C6B8A");

				ClearDirectXStateDependentResources();
				DisposeDirectXDeviceDependentResources();
				InitializeDirectXDeviceDependentResources();
				InitializeDirectXStateDependentResources(nested: true);

				return;
			}

			else result.CheckError();
		}

		// If the swap chain does not exist, create it.
		else {
			// First, retrieve the underlying DXGI Device from the D3D Device.
			// The swap must be created on the same adapter as the existing D3D Device.
			using var dxgiDevice = DeviceResources.D3Device!.QueryInterface<IDXGIDevice4>();

			// Next, get the parent factory from the DXGI Device.
			using var dxgiAdapter = dxgiDevice.GetAdapter();
			using var dxgiFactory = dxgiAdapter.GetParent<IDXGIFactory7>();

			// Finally, create the swap chain.
			using var swapChain = dxgiFactory.CreateSwapChainForHwnd(
				DeviceResources.D3Device,
				WindowHandle,
				new SwapChainDescription1 {
					Width = 0, // Use automatic sizing.
					Height = 0,
					Format = Format.B8G8R8A8_UNorm, // This is the most common swap chain format.
					Stereo = false,
					SampleDescription = new() {
						Count = 1, // Don't use multi-sampling.
						Quality = 0,
					},
					BufferUsage = Usage.RenderTargetOutput,
					BufferCount = 2, // Use two buffers to enable flip effect.
					Scaling = Scaling.Stretch,
					SwapEffect = SwapEffect.FlipSequential, // MS recommends using this swap effect for all applications.
					Flags = 0,
				},
				new SwapChainFullscreenDescription {
					Windowed = true
				},
				null // allow on all displays
			);

			DeviceResources.SwapChain = swapChain.QueryInterface<IDXGISwapChain4>();

			swapChain.Dispose();

			// Ensure that DXGI does not queue more than one frame at a time.
			// This both reduces latency and ensures that the application will only render
			// after each VSync, minimizing power consumption.
			dxgiDevice.SetMaximumFrameLatency(1);

			dxgiFactory.MakeWindowAssociation(WindowHandle, WindowAssociationFlags.IgnoreAll);
		}

		// Get a D2D surface from the DXGI back buffer to use as the D2D render target.
		// Direct2D needs the dxgi version of the backbuffer surface pointer.
		DeviceResources.BackBuffer = DeviceResources.SwapChain.GetBuffer<IDXGISurface2>(0);

		// So now we can set the Direct2D render target.
		var dpi = Bridge.Dpi;

		DeviceResources.D2DeviceContext!.SetDpi(dpi, dpi);
		DeviceResources.D2DeviceContext.Target =
		DeviceResources.D2RenderTarget = DeviceResources.D2DeviceContext.CreateBitmapFromDxgiSurface(DeviceResources.BackBuffer, new BitmapProperties1 {
			BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw,
			PixelFormat = new() {
				Format = Format.B8G8R8A8_UNorm,
				AlphaMode = Vortice.DCommon.AlphaMode.Premultiplied,
			},
			DpiX = dpi,
			DpiY = dpi,
		});
	}

	void ClearDirectXStateDependentResources() {
		Logger.LogDebug("Clearing DirectX state-dependent resources.");

		DirectXStateResourcesAreUninitialized = true;

		DeviceResources.D2DeviceContext?.Target?.Dispose();
		DeviceResources.D2RenderTarget?.Dispose();
		DeviceResources.BackBuffer?.Dispose();

		if (DeviceResources.D2DeviceContext != null)
			DeviceResources.D2DeviceContext.Target = null;

		DeviceResources.D2RenderTarget = null;
		DeviceResources.BackBuffer = null;
	}

	void InvokeDirectXRender(Int2 windowHardwareSize) {
		Logger.LogDebug("Invoking DirectX render {SZ}.", windowHardwareSize);

		DeviceResources.D2DeviceContext!.BeginDraw();
		DeviceResources.D2DeviceContext.Transform = Matrix3x2.CreateScale(Bridge.Zoom);

		// Actual content rendering.
		RootContent.OnRender(new RenderInfo(Application.Resources, DeviceResources));

		DeviceResources.D2DeviceContext.EndDraw().Tee(out var drawResult);

		// Present the rendered image to the window.
		// Because the maximum frame latency is set to 1,
		// the render loop will generally be throttled to the screen refresh rate,
		// typically around 60Hz, by sleeping the application on Present
		// until the screen is refreshed.
		DeviceResources.SwapChain!.Present(1, 0).Tee(out var presentResult);

		if (
			   drawResult == Vortice.Direct2D1.ResultCode.RecreateTarget
			|| presentResult == Vortice.DXGI.ResultCode.DeviceRemoved
			|| presentResult == Vortice.DXGI.ResultCode.DeviceReset
		) {
			ClearDirectXStateDependentResources();
			DisposeDirectXDeviceDependentResources();
		}

		else {
			drawResult.CheckError();
			presentResult.CheckError();
		}
	}

	#endregion
	#region Bridge handlers

	void OnBridgeState_programatic(WindowState previousState, WindowState newState) {
		Logger.LogDebug("Programatically setting window state to {S}.", newState switch {
			WindowState.Minimized => "minimized",
			WindowState.Restored => "Restored",
			WindowState.Maximized => "MAXIMIZED",
			WindowState.FullScreen => "[Full-Screen]",

			_ => throw new Bug("A1057B36-71C8-4758-A0D1-FC1FA68CE634"),
		});

		if (newState == WindowState.FullScreen)
			EnterWin32FullScreen();

		else {
			if (previousState == WindowState.FullScreen)
				ExitWin32FullScreen();

			ShowWindow(WindowHandle, (SHOW_WINDOW_CMD) newState);
		}
	}

	void OnBridgeSize(int dpi, float zoom, Int2 hardwareSize) {
		Logger.LogDebug("Updating {W}'s size {SZ} dpi={DPI} zoom={ZOOM}.", WindowHandle, hardwareSize, dpi, zoom);

		var dpiScale = dpi / 96f;

		Bridge.DpiScale = dpiScale;
		Bridge.ScaledSize = ((Vector2) hardwareSize) / (dpiScale * zoom);

		RequestRender();
	}

	#endregion
	#region Win32 OS message handlers

	void On_WM_SIZE(uint reason, ushort hardwareWidth, ushort hardwareHeight) {
		var newState = reason switch {
			SIZE_MAXIMIZED => WindowState.Maximized,
			SIZE_MINIMIZED => WindowState.Minimized,
			SIZE_RESTORED => TestWin32FullScreen()
				? WindowState.FullScreen
				: WindowState.Restored,

			_ => throw new Bug("F14AD583-9C00-4C61-B6F9-B72E0553A17C"),
		};

		Logger.LogDebug("WM_SIZE for {W}, setting size to {X}, {Y} {STATE}.", WindowHandle, hardwareWidth, hardwareHeight, newState);

		Bridge.StateSubject_observable.OnNext(newState);
		Bridge.HardwareSize = new Int2(hardwareWidth, hardwareHeight);
	}

	void On_WM_DPICHANGED(ushort dpi) {
		Logger.LogDebug("WM_DPICHANGED for {H}, setting DPI to {D}.", WindowHandle, dpi);

		Bridge.Dpi = dpi;
	}

	void On_WM_PAINT() {
		Logger.LogDebug("WM_PAINT for {W}.", WindowHandle);

		// According to  https://docs.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues#queued-messages:
		//
		// > Multiple WM_PAINT messages for the same window are combined into a single WM_PAINT message,
		// > consolidating all invalid parts of the client area into a single area.
		BeginPaint(WindowHandle, out PAINTSTRUCT lpPaint);
		RequestRender(); // TODO: should we use RunRenderPass() here?
		EndPaint(WindowHandle, in lpPaint);
	}

	#endregion
	#region Win32 stuff

	internal const string Win32WindowClassName = "UyWindow";
	const int DefaultSize = unchecked((int) 0x80000000);
	const WINDOW_STYLE DefaultStyle = WS_OVERLAPPEDWINDOW;
	const WINDOW_STYLE FullScreenStyle = default;

	internal readonly HWND WindowHandle;

	WINDOWPLACEMENT PlacementBeforeGoingFullScreen;

	unsafe void CreateWin32Window(out HWND windowHandle) {
		Logger.LogDebug("Creating Win32 window.");

		windowHandle = CreateWindowEx(
			hInstance: Application.InstanceHandle,
			lpClassName: Win32WindowClassName,
			dwStyle: DefaultStyle,
			dwExStyle: WS_EX_APPWINDOW,
			lpWindowName: string.Empty,
			X: DefaultSize,
			Y: DefaultSize,
			nWidth: DefaultSize,
			nHeight: DefaultSize,
			hWndParent: default,
			hMenu: default,
			lpParam: null
		);

		if (windowHandle == default) // Unable to create window.
			throw new Bug("F69D1F39-BBA5-4EC9-93B6-93696799083C-0x" + Marshal.GetLastWin32Error().ToString("X"));
	}

	void ShowWin32Window() {
		Logger.LogDebug("Showing Win32 window.");

		ShowWindow(WindowHandle, SW_NORMAL);

		Bridge.Dpi = GetDpiForWindow(WindowHandle).CoerceToInt();
	}

	void DestroyWin32Window() {
		Logger.LogDebug("Destroying Win32 window.");

		// DestroyWindow() sends the WM_DESTROY message,
		// which is the cause for this very method being called.
		// Let's NOT create a call loop here...
	}

	internal LRESULT ProcessWin32OsMessage(uint message, WPARAM wParam, LPARAM lParam) {
		switch (message) {
			case WM_SIZE: /*         */ On_WM_SIZE(wParam, LOWORD(lParam), HIWORD(lParam)); break;
			case WM_DPICHANGED: /*   */ On_WM_DPICHANGED(LOWORD(wParam)); break;
			case WM_PAINT: /*        */ On_WM_PAINT(); break;

			//case WM_MOUSEMOVE: /*    */ On_WM_MOUSEMOVE(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)); break;
			//case WM_MOUSELEAVE: /*   */ On_WM_MOUSELEAVE(); break;

			//case WM_MOUSEWHEEL: /*   */ On_WM_MOUSE_WHEEL(GET_WHEEL_DELTA_WPARAM(wParam), false); break;
			//case WM_MOUSEHWHEEL: /*  */ On_WM_MOUSE_WHEEL(GET_WHEEL_DELTA_WPARAM(wParam), true); break;

			//case WM_LBUTTONDOWN: /*  */ On_WM__BUTTONDOWN(Key.MouseLeft, false); break;
			//case WM_LBUTTONDBLCLK: /**/ On_WM__BUTTONDOWN(Key.MouseLeft, true); break;
			//case WM_LBUTTONUP: /*    */ On_WM__BUTTONUP__(Key.MouseLeft); break;
			//case WM_RBUTTONDOWN: /*  */ On_WM__BUTTONDOWN(Key.MouseRight, false); break;
			//case WM_RBUTTONDBLCLK: /**/ On_WM__BUTTONDOWN(Key.MouseRight, true); break;
			//case WM_RBUTTONUP: /*    */ On_WM__BUTTONUP__(Key.MouseRight); break;
			//case WM_MBUTTONDOWN: /*  */ On_WM__BUTTONDOWN(Key.MouseMiddle, false); break;
			//case WM_MBUTTONDBLCLK: /**/ On_WM__BUTTONDOWN(Key.MouseMiddle, true); break;
			//case WM_MBUTTONUP: /*    */ On_WM__BUTTONUP__(Key.MouseMiddle); break;
			//case WM_XBUTTONDOWN: /*  */ On_WM__BUTTONDOWN(HIWORD(wParam) == 1 ? Key.MouseExtra1 : Key.MouseExtra2, false); break;
			//case WM_XBUTTONDBLCLK: /**/ On_WM__BUTTONDOWN(HIWORD(wParam) == 1 ? Key.MouseExtra1 : Key.MouseExtra2, true); break;
			//case WM_XBUTTONUP: /*    */ On_WM__BUTTONUP__(HIWORD(wParam) == 1 ? Key.MouseExtra1 : Key.MouseExtra2); break;

			//case WM_KEYDOWN: /*      */ On_WM_KEYDOWN(wParam, lParam); break;
			//case WM_KEYUP: /*        */ On_WM_KEYUP(wParam, lParam); break;
			//case WM_SYSKEYDOWN: {
			//	// System keys are sent to the default procedure if the events are not blocked.
			//	if (On_WM_KEYDOWN(wParam, lParam))
			//		return DefWindowProc(WindowHandle, message, wParam, lParam);

			//	break;
			//}
			//case WM_SYSKEYUP: {
			//	// System keys are sent to the default procedure if the events are not blocked.
			//	if (On_WM_KEYUP(wParam, lParam))
			//		return DefWindowProc(WindowHandle, message, wParam, lParam);

			//	break;
			//}

			//case WM_CHAR: /*         */ On_WM_CHAR((char) wParam, lParam);
			//case WM_UNICHAR: /*      */ On_WM_CHAR(char.ConvertFromUtf32(unchecked((int) wParam)), lParam);

			//case WM_SETFOCUS: /*     */ On_WM_SETFOCUS();
			//case WM_KILLFOCUS: /*    */ On_WM_KILLFOCUS();

			//case WM_SYSCOMMAND: {
			//	On_WM_SYSCOMMAND((SysCommands) wParam);

			//	return DefWindowProc(WindowHandle, message, wParam, lParam);
			//}

			// The message was NONE of the above.
			default: return DefWindowProc(WindowHandle, message, wParam, lParam);
		}

		// The message WAS one of the above.
		return new LRESULT(0);
	}

	void EnterWin32FullScreen() {
		Logger.LogDebug("Entering full screen mode.");

		var monitor = new MONITORINFO { cbSize = (uint) Unsafe.SizeOf<MONITORINFO>() };

		if (!(
			   GetWindowPlacement(WindowHandle, ref PlacementBeforeGoingFullScreen)
			&& GetMonitorInfo(MonitorFromWindow(WindowHandle, MONITOR_DEFAULTTOPRIMARY), ref monitor)
			&& SetWindowLong(WindowHandle, GWL_STYLE, (int) FullScreenStyle) != 0
			&& SetWindowPos(WindowHandle, HWND_TOP, monitor.rcMonitor.left, monitor.rcMonitor.top, monitor.rcMonitor.right - monitor.rcMonitor.left, monitor.rcMonitor.bottom - monitor.rcMonitor.top, SWP_NOOWNERZORDER | SWP_FRAMECHANGED)
		))
			Logger.LogWarning("Cannot enter full screen mode.");
	}

	bool TestWin32FullScreen() {
		// From https://stackoverflow.com/a/55542400.
		var monitor = new MONITORINFO { cbSize = (uint) Unsafe.SizeOf<MONITORINFO>() };

		GetMonitorInfo(MonitorFromWindow(WindowHandle, MONITOR_DEFAULTTOPRIMARY), ref monitor);
		GetWindowRect(WindowHandle, out var windowRect);

		return windowRect.left == monitor.rcMonitor.left
		    && windowRect.right == monitor.rcMonitor.right
		    && windowRect.top == monitor.rcMonitor.top
		    && windowRect.bottom == monitor.rcMonitor.bottom;
	}

	void ExitWin32FullScreen() {
		Logger.LogDebug("Exising full screen mode.");

		if (!(
			   SetWindowLong(WindowHandle, GWL_STYLE, (int) DefaultStyle) != 0
			&& SetWindowPlacement(WindowHandle, PlacementBeforeGoingFullScreen)
		))
			Logger.LogWarning("Cannot exit full screen mode.");
	}

	#endregion
}
