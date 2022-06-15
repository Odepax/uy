using System;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using Vortice.Mathematics;

namespace Uy;

/**
<summary>
	<para>
		Allows consumers to be notified of current window's state and lifetime events.
		This interface is not intended to be user-replaceable.
	</para>
</summary>
**/
public interface IWindowBridge {
	/**
	<summary>
		<para>
			Represents an object that schedules units of work
			on the <see cref="Uy"/> application's game loop.
		</para>
	</summary>
	<remarks>
		<para>
			Work items are run after processing the OS' messages,
			but before the render pass.
			Their execution <b>blocks the UI thread</b>;
			work items scheduled on this scheduler must be kept light and swift!
		</para>
		<para>
			The <see cref="GameLoopScheduler"/> must be thread-safe.
		</para>
	</remarks>
	**/
	IScheduler GameLoopScheduler { get; }

	/**
	<summary>
		<para>
			Gets or sets the state of the current window.
		</para>
	</summary>
	**/
	WindowState State { get; set; }

	/**
	<inheritdoc cref="State"/>
	**/
	ISubject<WindowState> StateSubject { get; }

	/**
	<summary>
		<para>
			Gets or sets the title of the current window.
		</para>
	</summary>
	**/
	string Title { get; set; }

	/**
	<inheritdoc cref="Title"/>
	**/
	ISubject<string> TitleSubject { get; }

	/**
	<summary>
		<para>
			Gets or sets the zoom level of the current window.
		</para>
	</summary>
	**/
	float Zoom { get; set; }

	/**
	<inheritdoc cref="Zoom"/>
	**/
	ISubject<float> ZoomSubject { get; }

	/**
	<summary>
		<para>
			Gets the DPI of the current window.
		</para>
	</summary>
	<remarks>
		<para>
			The difference between <see cref="Dpi"/> and <see cref="DpiScale"/>
			is that <see cref="Dpi"/> returns the a number of virtual pixels per inch,
			whereas <see cref="DpiScale"/> returns the scale compared to the standard of 96dpi.
		</para>
	</remarks>
	**/
	int Dpi { get; }

	/**
	<inheritdoc cref="Dpi"/>
	**/
	IObservable<int> DpiObservable { get; }

	/**
	<inheritdoc cref="Dpi"/>
	**/
	float DpiScale { get; }

	/**
	<inheritdoc cref="Dpi"/>
	**/
	IObservable<float> DpiScaleObservable { get; }

	/**
	<summary>
		<para>
			Gets the size of the current window's drawaing area,
			expressed in hardware pixels,
			effectively mapping to the pixels on the user's screen.
		</para>
	</summary>
	**/
	Int2 HardwareSize { get; }

	/**
	<inheritdoc cref="HardwareSize"/>
	**/
	IObservable<Int2> HardwareSizeObservable { get; }

	/**
	<summary>
		<para>
			Gets the size of the current window's drawaing area,
			expressed in arbitrary units,
			i.e. after applying <see cref="DpiScale"/> and <see cref="Zoom"/>.
		</para>
	</summary>
	**/
	Vector2 ScaledSize { get; }

	/**
	<inheritdoc cref="ScaledSize"/>
	**/
	IObservable<Vector2> ScaledSizeObservable { get; }

	/**
	<summary>
		<para>
			Triggered when the host window has fully initialized and is visible to the user.
		</para>
	</summary>
	**/
	CancellationToken WindowOpened { get; }

	/**
	<summary>
		<para>
			Triggered when the host window is starting a graceful close.
			Closing will block until all callbacks registered on this token have completed.
		</para>
	</summary>
	**/
	CancellationToken WindowClosing { get; }

	/**
	<summary>
		<para>
			Triggered when the host window has completed a graceful close.
			The window will not close until all callbacks registered on this token have completed.
		</para>
	</summary>
	**/
	CancellationToken WindowClosed { get; }

	/**
	<summary>
		<para>
			Requests closing of the current window.
		</para>
	</summary>
	**/
	void CloseWindow();

	// React to another window's closing => Use the scopped window bridge for its root content
	// React to another window's opening => Use the scopped window bridge for its root content
	// Close another window => Use the scopped window bridge for its root content

	// Open another window => *inspires*
	/*
		.OpenMainWindow<T>() where T : IWindowRootContent =>
			.OpenMainWindow<T>(serviceProvider => serviceProvider.GetRequiredService<T>());

		.OpenMainWindow<T>(Func<IServiceProvider, T> rootContentFactory) where T : IWindowRootContent

		.OpenModelessWindow()
		.OpenToolWindow()
		.OpenPopUpWindow()
		.OpenModalWindow(public enum ModalScope { Window, Branch, Tree, Application } scope)

		.OpenDialogWindow()
		// Could it require a separate service by itself?
		// Should everything related to another window than the current one moved to a separate interface?
		// Should the .Open*Window methods return the root content? (conflicts with below)
		//	`-> Should the dialog windows return a Task<T>? (conflicts with above)
			-> Open file
			-> Open multiple files
			-> Open directory
			-> Open multiple directories
			-> Save file
			-> Save multiple files
			-> Save directory
			-> Save multiple directories
			-> Alert
			-> Confirm
			-> Prompt
				-> Prompt enum
				-> Prompt simple text
				-> Prompt custom types
	*/

	/**
	<summary>
		<para>
			Marks the drawing that's currently painted on the current window's client area as stale,
			forcing the next iteration of the game loop to invoke a render pass.
		</para>
	</summary>
	**/
	void RequestRender();
}
