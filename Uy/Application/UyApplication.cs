using LinqToYourDoom;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpGen.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.PEEK_MESSAGE_REMOVE_TYPE;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.UI.WindowsAndMessaging.SYSTEM_METRICS_INDEX;
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
	readonly ILogger<UyApplication<TMainWindowContent>> Logger;
	readonly IHostApplicationLifetime ApplicationLifetime;
	readonly IServiceProvider ServiceProvider;

	public UyApplication(ILogger<UyApplication<TMainWindowContent>> logger, IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider) {
		Logger = logger;
		ApplicationLifetime = applicationLifetime;
		ServiceProvider = serviceProvider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		using var application = new Win32Application(ServiceProvider);

		application.OpenWindow(typeof(TMainWindowContent));
		application.RunGameLoop(stoppingToken);

		//Logger.LogDebug("This application will close in 2 seconds...");

		//await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

		//ApplicationLifetime.StopApplication();

		//InitializeWin32WindowClass();
		//InitializeWin32Window();
		//ShowWin32Window();
		//InitializeDirectX();
		//RunGameLoop();
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
	}

	public void OpenWindow(Type windowRootContentType) {
		var window = new Win32Window(this, windowRootContentType);

		if (Windows.TryAdd(window.WindowHandle, window).Nt())
			throw new Bug("2FDB1820-D621-4DF8-995A-5368DFF02774");
	}

	// FROM VORTICE
	public void RunGameLoop(CancellationToken stoppingToken) {
		Logger.LogDebug("Entering game loop.");

		while (stoppingToken.IsCancellationRequested.Nt()) {
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
				// After thorough audit, I'm sorry to announce that my ass isn't chicken, though...
				if (msg.message == WM_QUIT || stoppingToken.IsCancellationRequested)
					break;
			}

			if (msg.message == WM_QUIT || stoppingToken.IsCancellationRequested)
				break;
		}

		Logger.LogDebug("Exited game loop.");
	}

	// FROM PREVIOUS UY
	//public unsafe void RunMessageLoop(CancellationToken stoppingToken) {
	//	var lastTimestamp = 0L;
	//	var clockStopwatch = Stopwatch.StartNew();

	//	try {
	//		User32.ShowWindow(WindowHandle, ShowWindowCommand.Show);
	//		Content.OnAppear();

	//		while (true) {
	//			// OS Messages.
	//			MSG msg;

	//			while (User32.PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_REMOVE)) {
	//				User32.TranslateMessage(ref msg);
	//				User32.DispatchMessage(ref msg);

	//				// From https://gamedev.stackexchange.com/questions/59857/game-loop-on-windows:
	//				//
	//				// > PeekMessage will only ever return WM_QUIT once the message queue is empty,
	//				// > therefore guaranteeing that the last message is in fact WM_QUIT.
	//				//
	//				// The author of the comment cites https://docs.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues.
	//				//
	//				// This removes the need for a duplicate "if (message == WM_QUIT || stoppingToken) break"
	//				// in the nested "while (PeekMessage)" loop.
	//				//
	//				// Well, I may be cursed, but I still get messages after WM_QUIT.
	//				// After thorough audit, I'm sorry to announce that my ass isn't chicken, though...
	//				if (msg.message == WindowsMessages.WM_QUIT || stoppingToken.IsCancellationRequested)
	//					break;
	//			}

	//			if (msg.message == WindowsMessages.WM_QUIT || stoppingToken.IsCancellationRequested)
	//				break;

	//			// Clock/frame tick/update.
	//			var timestamp = clockStopwatch.ElapsedMilliseconds;
	//			var secondsSinceLastTick = (timestamp - lastTimestamp) / 1_000f;
	//			var secondsSinceFirstTick = (lastTimestamp = timestamp) / 1_000f;

	//			TopLog.Event(nameof(Content) + '-' + nameof(Content.OnClockUpdate), "Seconds since last tick: " + secondsSinceLastTick);

	//			Content.OnClockUpdate(new(secondsSinceFirstTick, secondsSinceLastTick));

	//			// Re-render.
	//			RenderAsRequested();
	//		}
	//	}

	//	catch (Exception e) {
	//		TopLog.Event(e.GetType().Name + " in the game loop", e.Message);
	//	}

	//	finally {
	//		// At this point, the main window got WM_CLOSE-ed, and our implementation
	//		// of the window procedure sent WM_DESTROY in response.

	//		if (State_output.Value != WindowState.Minimized)
	//			Content.OnDisappear();
	//		// TODO: There is no detach upon WM_DESTROY!

	//		// TODO: Determine how we want to close windows.
	//		//CloseWindow(WindowHandle);
	//	}

	//	clockStopwatch.Stop();
	//}

	#region Win32 stuff

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
				lpfnWndProc = Win32WindowProcedure,

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

			// TODO: UnregisterClass() must be called manually (to be confirmed)!
			var windowClassRegistrationAtom = RegisterClassEx(windowClass);

			if (windowClassRegistrationAtom == 0) // Unable to register window class.
				throw new Bug("E23FBF90-83F5-4992-B724-1410808ACDEF-0x" + Marshal.GetLastWin32Error().ToString("X"));
		}
	}

	LRESULT Win32WindowProcedure(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam) {
		if (Windows.TryGetValue(hWnd, out var window))
			return window.ProcessOsMessage(message, wParam, lParam);

		else {
			Logger.LogWarning("No window {HWND} found to process message {MSG}!", hWnd.Value, message);

			return DefWindowProc(hWnd, message, wParam, lParam);
		}
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

		if (false)
		throw new NotImplementedException("DI-get and install the IWindowRootContent. Get the System.type from a constructor parameter, no generic here.");

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

		throw new NotImplementedException("Actually close and destroy the Win32 window.");

		Bridge.LinkedWindow = null;
		Bridge.WindowClosed.Cancel();

		ServiceScope.Dispose();
		_ = Bridge; // Already disposed with the service scope.
	}

	internal LRESULT ProcessOsMessage(uint message, WPARAM wParam, LPARAM lParam) {
		// FROM VORTICE

		//switch (message) {
		//	case WM_ACTIVATEAPP: {
		//		//if (wParam != 0)
		//		//	Current?.OnActivated();
		//		//else
		//		//	Current?.OnDeactivated();
		//	} break;

		//	case WM_KEYDOWN:
		//	case WM_KEYUP:
		//	case WM_SYSKEYDOWN:
		//	case WM_SYSKEYUP:
		//		//OnKey(message, wParam, lParam);
		//		break;

		//	case WM_DESTROY:
		//		PostQuitMessage(0);
		//		break;
		//}

		return DefWindowProc(WindowHandle, message, wParam, lParam);
	}

	// FROM PREVIOUS UY
	//[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
	//static unsafe IntPtr WindowProc(IntPtr hWnd, WindowsMessages msg, nuint wparam, nint lparam) {
	//	if (Windows.TryGetValue(hWnd, out var window)) {
	//		/**/ if (msg == WindowsMessages.WM_SIZE) /*         */ window.On_WM_SIZE((WindowResizeReason) wparam, LOWORD(lparam), HIWORD(lparam));
	//		else if (msg == WindowsMessages.WM_DPICHANGED) /*   */ window.On_WM_DPICHANGED(LOWORD(wparam));
	//		else if (msg == WindowsMessages.WM_PAINT) /*        */ window.On_WM_PAINT();

	//		else if (msg == WindowsMessages.WM_MOUSEMOVE) /*    */ window.On_WM_MOUSEMOVE(GET_X_LPARAM(lparam), GET_Y_LPARAM(lparam));
	//		else if (msg == WindowsMessages.WM_MOUSEWHEEL) /*   */ window.On_WM_MOUSE_WHEEL(GET_WHEEL_DELTA_WPARAM(wparam), false);
	//		else if (msg == WindowsMessages.WM_MOUSEHWHEEL) /*  */ window.On_WM_MOUSE_WHEEL(GET_WHEEL_DELTA_WPARAM(wparam), true);
	//		else if (msg == WindowsMessages.WM_LBUTTONDOWN) /*  */ window.On_WM__BUTTONDOWN(Key.MouseLeft, false);
	//		else if (msg == WindowsMessages.WM_LBUTTONDBLCLK) /**/ window.On_WM__BUTTONDOWN(Key.MouseLeft, true);
	//		else if (msg == WindowsMessages.WM_LBUTTONUP) /*    */ window.On_WM__BUTTONUP__(Key.MouseLeft);
	//		else if (msg == WindowsMessages.WM_RBUTTONDOWN) /*  */ window.On_WM__BUTTONDOWN(Key.MouseRight, false);
	//		else if (msg == WindowsMessages.WM_RBUTTONDBLCLK) /**/ window.On_WM__BUTTONDOWN(Key.MouseRight, true);
	//		else if (msg == WindowsMessages.WM_RBUTTONUP) /*    */ window.On_WM__BUTTONUP__(Key.MouseRight);
	//		else if (msg == WindowsMessages.WM_MBUTTONDOWN) /*  */ window.On_WM__BUTTONDOWN(Key.MouseMiddle, false);
	//		else if (msg == WindowsMessages.WM_MBUTTONDBLCLK) /**/ window.On_WM__BUTTONDOWN(Key.MouseMiddle, true);
	//		else if (msg == WindowsMessages.WM_MBUTTONUP) /*    */ window.On_WM__BUTTONUP__(Key.MouseMiddle);
	//		else if (msg == WindowsMessages.WM_XBUTTONDOWN) /*  */ window.On_WM__BUTTONDOWN(HIWORD(wparam) == 1 ? Key.MouseExtra1 : Key.MouseExtra2, false);
	//		else if (msg == WindowsMessages.WM_XBUTTONDBLCLK) /**/ window.On_WM__BUTTONDOWN(HIWORD(wparam) == 1 ? Key.MouseExtra1 : Key.MouseExtra2, true);
	//		else if (msg == WindowsMessages.WM_XBUTTONUP) /*    */ window.On_WM__BUTTONUP__(HIWORD(wparam) == 1 ? Key.MouseExtra1 : Key.MouseExtra2);
	//		else if (msg == WindowsMessages.WM_MOUSELEAVE) /*   */ window.On_WM_MOUSELEAVE();

	//		else if (msg == WindowsMessages.WM_KEYDOWN) /*      */ window.On_WM_KEYDOWN(wparam, lparam);
	//		else if (msg == WindowsMessages.WM_KEYUP) /*        */ window.On_WM_KEYUP(wparam, lparam);
	//		else if (msg == WindowsMessages.WM_SYSKEYDOWN) {
	//			// Sys' keys are sent to the default procedure if the events are not blocked.
	//			if (window.On_WM_KEYDOWN(wparam, lparam))
	//				return User32.DefWindowProc(hWnd, msg, wparam, lparam);
	//		}
	//		else if (msg == WindowsMessages.WM_SYSKEYUP) {
	//			// Sys' keys are sent to the default procedure if the events are not blocked.
	//			if (window.On_WM_KEYUP(wparam, lparam))
	//				return User32.DefWindowProc(hWnd, msg, wparam, lparam);
	//		}
	//		else if (msg == WindowsMessages.WM_CHAR) /*         */ window.On_WM_CHAR((char) wparam, lparam);
	//		else if (msg == WindowsMessages.WM_UNICHAR) /*      */ window.On_WM_CHAR(char.ConvertFromUtf32(unchecked((int) wparam)), lparam);

	//		//else if (msg == WindowsMessages.WM_SETFOCUS) /*     */ window.On_WM_SETFOCUS();
	//		//else if (msg == WindowsMessages.WM_KILLFOCUS) /*    */ window.On_WM_KILLFOCUS();

	//		else if (msg == WindowsMessages.WM_DESTROY) {
	//			TopLog.Event(nameof(WindowsMessages.WM_DESTROY));

	//			User32.PostQuitMessage(0);
	//		}

	//		//else if (msg == WindowsMessages.WM_SYSCOMMAND) {
	//		//	window.On_WM_SYSCOMMAND((SysCommands) wparam);

	//		//	return User32.DefWindowProc(hWnd, msg, wparam, lparam);
	//		//}

	//		// The message is NONE of the above.
	//		else return User32.DefWindowProc(hWnd, msg, wparam, lparam);

	//		// The message IS one of the above.
	//		return IntPtr.Zero;

	//		// TODO: should we handle WM_DESTROY with PostQuitMessage(0) ?
	//		// TODO: should we handle WM_CLOSE with PostQuitMessage(0) ?
	//		//
	//		// From https://docs.microsoft.com/en-us/windows/win32/winmsg/window-features#window-destruction:
	//		//
	//		// - Clicking "close" sends WM_CLOSE.
	//		// - Process the WM_CLOSE message to confirm with the user before calling User32.DestroyWindow().
	//		//   E.g at https://docs.microsoft.com/en-us/windows/win32/learnwin32/closing-the-window
	//	}

	//	// Initialization might not be complete yet.
	//	else return User32.DefWindowProc(hWnd, msg, wparam, lparam);
	//}

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

	/*
	void DestroyWin32Window() {
		Logger.LogDebug("Destroying Win32 window.");

		// FROM VORTICE
			HWND destroyHandle = WindowHandle;
			WindowHandle = default;

			Debug.WriteLine($"[WIN32] - Destroying window: {destroyHandle}");
			DestroyWindow(destroyHandle);

		// FROM PREVIOUS UY
			// Nothing?!
	}
	*/

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
