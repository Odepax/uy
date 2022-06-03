using System;
using System.Numerics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Uy;

class Win32WindowBridge : IWindowBridge, IDisposable {
	ISubject<WindowState> IWindowBridge.StateSubject => StateSubject;
	ISubject<string> IWindowBridge.TitleSubject => TitleSubject;
	ISubject<float> IWindowBridge.ZoomSubject => ZoomSubject;
	IObservable<float> IWindowBridge.DpiScaleObservable => DpiScaleSubject;
	IObservable<Vector2> IWindowBridge.SizeObservable => SizeSubject;

	public readonly ISubject<WindowState> StateSubject;
	public readonly Subject<WindowState> StateSubject_observer;
	public readonly BehaviorSubject<WindowState> StateSubject_observable;
	public readonly BehaviorSubject<string> TitleSubject = new(string.Empty);
	public readonly BehaviorSubject<float> ZoomSubject = new(1f);
	public readonly BehaviorSubject<float> DpiScaleSubject = new(1f);
	public readonly BehaviorSubject<Vector2> SizeSubject = new(Vector2.Zero);

	public WindowState State { get => StateSubject_observable.Value; set => StateSubject_observer.OnNext(value); }
	public string Title { get => TitleSubject.Value; set => TitleSubject.OnNext(value); }
	public float Zoom { get => ZoomSubject.Value; set => ZoomSubject.OnNext(value); }
	public float DpiScale { get => DpiScaleSubject.Value; set => DpiScaleSubject.OnNext(value); }
	public Vector2 Size { get => SizeSubject.Value; set => SizeSubject.OnNext(value); }

	CancellationToken IWindowBridge.WindowOpened => WindowOpened.Token;
	CancellationToken IWindowBridge.WindowClosing => WindowClosing.Token;
	CancellationToken IWindowBridge.WindowClosed => WindowClosed.Token;

	public readonly CancellationTokenSource WindowOpened = new();
	public readonly CancellationTokenSource WindowClosing = new();
	public readonly CancellationTokenSource WindowClosed = new();

	/**
	<summary>
		<para>
			The current underlying window.
		</para>
	</summary>
	<remarks>
		<para>
			This property is set by the <see cref="Win32Window"/> upon creation,
			providing a cyclic reference between the window wrapper and the dependency injection bridge.
		</para>
		<para>
			This object should <b>not be disposed along with the bridge</b>.
			It's the role of the <see cref="Win32Window"/> to dispose of the dependency injection scope!
		</para>
	</remarks>
	**/
	public Win32Window? LinkedWindow { private get; set; }

	public Win32WindowBridge() {
		StateSubject_observer = new();
		StateSubject_observable = new(WindowState.Restored);
		StateSubject = Subject.Create<WindowState>(
			StateSubject_observer,
			StateSubject_observable.DistinctUntilChanged()
		);
	}

	public void Dispose() {
		StateSubject_observer.Dispose();
		StateSubject_observable.Dispose();
		TitleSubject.Dispose();
		ZoomSubject.Dispose();
		DpiScaleSubject.Dispose();
		SizeSubject.Dispose();

		WindowOpened.Dispose();
		WindowClosing.Dispose();
		WindowClosed.Dispose();
	}

	public void CloseWindow() => throw new NotImplementedException();
}
