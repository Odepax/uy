using LinqToYourDoom;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.HiDpi.PROCESS_DPI_AWARENESS;
using static Windows.Win32.UI.WindowsAndMessaging.PEEK_MESSAGE_REMOVE_TYPE;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_EX_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;

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
		This is a wrapper!
	</para>
</remarks>
**/
class Win32Application : IDisposable {
	internal readonly IServiceProvider ServiceProvider;
	internal readonly FreeLibrarySafeHandle InstanceHandle;

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

		UnregisterWin32WindowClass();

		InstanceHandle.Close();
		InstanceHandle.Dispose();

		_ = ServiceProvider; // Not the application's job to dispose of the service provider.
	}

	public void OpenWindow(Type windowRootContentType) {
		var window = new Win32Window(this, windowRootContentType);

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
		}

		clockStopwatch.Stop();

		Logger.LogDebug("Exited game loop.");
	}

	#region Win32 stuff

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
				lpfnWndProc = ProcessOsMessage,

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

	void UnregisterWin32WindowClass() =>
		UnregisterClass(Win32Window.Win32WindowClassName, InstanceHandle);

	LRESULT ProcessOsMessage(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam) {
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
			//case WM_CLOSE: {
			//	break;
			//}
			case WM_DESTROY: {
				Logger.LogDebug(nameof(WM_DESTROY));

				if (Windows.TryRemove(hWnd, out var window))
					window.Dispose();
				
				else throw new Bug("44018E69-84C4-453F-A930-8B8B2A643E20");

				if (window.IsMainWindow && Interlocked.Decrement(ref MainWindowCount) == 0)
					PostQuitMessage(0);

				break;
			}
			//case WM_QUIT: {
			//	break;
			//}

			//case WM_ACTIVATEAPP: {
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
					return window.ProcessOsMessage(message, wParam, lParam);

				else {
					Logger.LogWarning("No window {HWND} found to process message {MSG}!", hWnd.Value, message);

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
		This is a wrapper!
	</para>
</remarks>
	**/
class Win32Window : IDisposable {
	public readonly bool IsMainWindow = true;

	readonly Win32Application Application;
	readonly IServiceScope ServiceScope;
	readonly WindowBridge Bridge;
	readonly ILogger<Win32Window> Logger;

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
		Bridge = ServiceScope.ServiceProvider.GetRequiredService<IWindowBridge>().As<WindowBridge>();
		Logger = ServiceScope.ServiceProvider.GetRequiredService<ILogger<Win32Window>>();

		// First thing first: before we forget about it,
		// we're going to register this Win32 window with the scoped window bridge!
		Bridge.LinkedWindow = this;

		CreateWin32Window(out WindowHandle);
		ShowWin32Window();

		// TODO: DI-get and install the IWindowRootContent. Get the System.type from a constructor parameter, no generic here.");

		Bridge.WindowOpened.Cancel();
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

		DestroyWin32Window();

		Bridge.LinkedWindow = null;
		Bridge.WindowClosed.Cancel();

		_ = Application; // Not the window's job to dispose of the application.
		ServiceScope.Dispose();
		_ = Bridge; // Already disposed with the service scope.
	}

	#region Win32 stuff

	internal const string Win32WindowClassName = "UyWindow";
	const int DefaultSize = unchecked((int) 0x80000000);

	internal readonly HWND WindowHandle;

	unsafe void CreateWin32Window(out HWND windowHandle) {
		Logger.LogDebug("Creating Win32 window.");

		windowHandle = CreateWindowEx(
			hInstance: Application.InstanceHandle,
			lpClassName: Win32WindowClassName,
			dwStyle: WS_OVERLAPPEDWINDOW | WS_CLIPSIBLINGS | WS_BORDER | WS_DLGFRAME | WS_GROUP | WS_TABSTOP | WS_SIZEBOX,
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

		ShowWindow(WindowHandle, SW_SHOW);
	}

	void DestroyWin32Window() {
		Logger.LogDebug("Destroying Win32 window.");

		// DestroyWindow() sends the WM_DESTROY message,
		// which is the cause for this very method being called.
		// Let's NOT create a call loop here...
	}

	internal LRESULT ProcessOsMessage(uint message, WPARAM wParam, LPARAM lParam) {
		switch (message) {
			//case WM_SIZE: /*         */ On_WM_SIZE((WindowResizeReason) wParam, LOWORD(lParam), HIWORD(lParam)); break;
			//case WM_DPICHANGED: /*   */ On_WM_DPICHANGED(LOWORD(wParam)); break;
			//case WM_PAINT: /*        */ On_WM_PAINT(); break;

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

	#endregion
}


/*



class Application : IDisposable {

	IGraphicsDevice? _graphicsDevice;

	public unsafe Application() {

		// init window class was here

		MainWindow = new Window("Vortice", 800, 600);
	}

	public static Application? Current { get; private set; }

	public Window? MainWindow { get; private set; }

	public virtual void Dispose() {
		_graphicsDevice?.Dispose();
	}

	public void Tick() {
		if (_graphicsDevice != null) {
			_graphicsDevice.DrawFrame(OnDraw);
		}
		else {
			OnDraw(MainWindow!.ClientSize.Width, MainWindow.ClientSize!.Height);
		}
	}

	public unsafe void Run() {
		InitializeBeforeRun();

		while (true) {
			if (PeekMessage(out var msg, default, 0, 0, PM_REMOVE)) {
				TranslateMessage(&msg);
				DispatchMessage(&msg);

				if (msg.message == WM_QUIT)
					break;
			}

			Tick();
		}
	}

	protected override void InitializeBeforeRun() {
		_graphicsDevice = new D3D11GraphicsDevice(MainWindow!);
	}


	protected override void OnDraw(int width, int height) {
		((D3D11GraphicsDevice) _graphicsDevice!).DeviceContext.Flush();

		if (_screenshot) {
			SaveScreenshot("Screenshot.jpg");
			_screenshot = false;
		}
	}


	public string Title { get; private set; }
	public SizeI ClientSize { get; private set; }

	public unsafe Window(string title, int width, int height) {
		Title = title;
		ClientSize = new(width, height);


	}

	
}


interface IGraphicsDevice { }


class CoreView {

	public void Run() {
		// First, create the Direct3D device.

		// This flag is required in order to enable compatibility with Direct2D.
		var creationFlags = DeviceCreationFlags.BgraSupport;

#if DEBUG
		// If the project is in a debug build, enable debugging via SDK Layers with this flag.
		creationFlags |= DeviceCreationFlags.Debug;
#endif

		// This array defines the ordering of feature levels that D3D should attempt to create.
		var featureLevels = new[] {
			FeatureLevel.Level_11_1,
			FeatureLevel.Level_11_0,
			FeatureLevel.Level_10_1,
			FeatureLevel.Level_10_0,
			FeatureLevel.Level_9_3,
			FeatureLevel.Level_9_1,
		};

		ID3D11Device d3dDevice;
		ID3D11DeviceContext d3dDeviceContext;

		D3D11.D3D11CreateDevice(
			IntPtr.Zero, // Specify nullptr to use the default adapter.
			DriverType.Hardware,
			creationFlags, // Optionally set debug and Direct2D compatibility flags.
			featureLevels,
			out d3dDevice,
			out d3dDeviceContext
		);

		// Retrieve the Direct3D 11.1 interfaces.
		D3dDevice = ComObject.As<ID3D11Device1>(d3dDevice);
		D3dDeviceContext = ComObject.As<ID3D11DeviceContext1>(d3dDeviceContext);

		// After the D3D device is created, create additional application resources.
		InitializeWindowSizeDependentResources();

		// Enter the render loop.
		// Note that a UWP app should never exit.
		while (true) {
			// Process events incoming to the window.
			Window!.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

			// Specify the render target we created as the output target.
			// Clear the render target to a solid color.
			D3dDeviceContext.OMSetRenderTargets(RenderTargetView!);
			D3dDeviceContext.ClearRenderTargetView(RenderTargetView, new Color4(0.071f, 0.04f, 0.561f));

			// Present the rendered image to the window.
			// Because the maximum frame latency is set to 1,
			// the render loop will generally be throttled to the screen refresh rate,
			// typically around 60Hz, by sleeping the application on Present
			// until the screen is refreshed.
			SwapChain!.Present(1, 0);
		}
	}

	void InitializeWindowSizeDependentResources() {
		RenderTargetView = null;

		// If the swap chain already exists, resize it.
		if (SwapChain != null)
			SwapChain.ResizeBuffers(2, 0, 0, Format.B8G8R8A8_UNorm, 0);

		// If the swap chain does not exist, create it.
		else {
			var swapChainDesc = new SwapChainDescription1 {
				Stereo = false,
				BufferUsage = Usage.RenderTargetOutput,
				Scaling = Scaling.None,
				Flags = 0,
				Width = 0, // Use automatic sizing.
				Height = 0,
				Format = Format.B8G8R8A8_UNorm, // This is the most common swap chain format.
				SampleDescription = new() {
					Count = 1, // Don't use multi-sampling.
					Quality = 0,
				},
				BufferCount = 2, // Use two buffers to enable flip effect.
				SwapEffect = SwapEffect.FlipSequential, // MS recommends using this swap effect for all applications.
			};

			// Once the swap chain description is configured,
			// it must be created on the same adapter as the existing D3D Device.

			// First, retrieve the underlying DXGI Device from the D3D Device.
			var dxgiDevice = ComObject.As<IDXGIDevice2>(D3dDevice);

			// Ensure that DXGI does not queue more than one frame at a time.
			// This both reduces latency and ensures that the application will only render
			// after each VSync, minimizing power consumption.
			dxgiDevice.SetMaximumFrameLatency(1);

			// Next, get the parent factory from the DXGI Device.
			var dxgiAdapter = dxgiDevice.GetAdapter();
			var dxgiFactory = dxgiAdapter.GetParent<IDXGIFactory2>();

			// Finally, create the swap chain.
			SwapChain = dxgiFactory.CreateSwapChainForCoreWindow(
				D3dDevice,
				Window.As<IUnknown>(),
				swapChainDesc,
				null // allow on all displays
			);
		}

		// Once the swap chain is created, create a render target view.
		// This will allow Direct3D to render graphics to the window.

		var backBuffer = SwapChain.GetBuffer<ID3D11Texture2D>(0);

		RenderTargetView = D3dDevice!.CreateRenderTargetView(backBuffer);

		// After the render target view is created, specify that the viewport,
		// which describes what portion of the window to draw to,
		// should cover the entire window.

		var backBufferDesc = backBuffer.Description;
		var viewport = new Viewport(0.0f, 0.0f, backBufferDesc.Width, backBufferDesc.Height, 0, 1);

		D3dDeviceContext!.RSSetViewports(new[] { viewport });
	}

	ID3D11Device1? D3dDevice;
	ID3D11DeviceContext1? D3dDeviceContext;
	IDXGISwapChain1? SwapChain;
	ID3D11RenderTargetView? RenderTargetView;

	public void SetWindow(CoreWindow window) {
		Window = window;

		Window.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
		Window.SizeChanged += (_, _) => InitializeWindowSizeDependentResources();
	}
}
*/
