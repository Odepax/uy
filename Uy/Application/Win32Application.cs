using LinqToYourDoom;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.HiDpi.PROCESS_DPI_AWARENESS;
using static Windows.Win32.UI.WindowsAndMessaging.PEEK_MESSAGE_REMOVE_TYPE;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;

namespace Uy;

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
	internal readonly GameLoopScheduler GameLoopScheduler = new();

	readonly ILogger<Win32Application> Logger;
	readonly ConcurrentDictionary<HWND, Win32Window> Windows = new();

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
		Logger = ServiceProvider.GetRequiredService<ILogger<Win32Application>>();
		InstanceHandle = GetModuleHandle(null as string) ?? throw new Bug("6ADC2291-23FF-4829-82BE-666FBAD0961F"); // NULL => Unable to get the Win32 instance handle.

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

		GameLoopScheduler.Now = DateTimeOffset.Now;

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

			// Scheduled work.
			GameLoopScheduler.Now += clockStopwatch.Elapsed;
			GameLoopScheduler.Flush();

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
