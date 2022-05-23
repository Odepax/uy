using LinqToYourDoom;
using System;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;

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
			Gets the DPI scale of the current window.
		</para>
	</summary>
	**/
	float DpiScale { get; }

	/**
	<inheritdoc cref="DpiScale"/>
	**/
	IObservable<float> DpiScaleObservable { get; }

	/**
	<summary>
		<para>
			Gets the size of the current window.
		</para>
	</summary>
	**/
	Vector2 Size { get; }

	/**
	<inheritdoc cref="Size"/>
	**/
	IObservable<Vector2> SizeObservable { get; }

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
}
