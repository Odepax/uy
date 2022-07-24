using System;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Uy;
using static Uy.Key;

class GameWindowRootContent : IWindowRootContent, IDisposable {
	readonly Game GameEngine;
	readonly IWindowBridge WindowBridge;

	readonly CompositeDisposable ApplicationDisposables = new();
	readonly CompositeDisposable DeviceDisposables = new();
	readonly CompositeDisposable ResizeDisposables = new();

	bool Resized = true;

	public GameWindowRootContent(Game gameEngine, IDeviceIndependentResourceDictionary applicationResources, IWindowBridge windowBridge) {
		GameEngine = gameEngine;
		WindowBridge = windowBridge;

		#if !DEBUG
		WindowBridge.State = WindowState.FullScreen;
		#endif

		WindowBridge.ScaledSizeObservable
			.ObserveOn(WindowBridge.GameLoopScheduler)
			.Subscribe(size => {
				GameEngine.Window.Size = size;
				GameEngine.Window.Box = Box4.FromLeftTop(Vector2.Zero, size);

				ResizeDisposables.Clear();
				Resized = true;
			})
			.DisposeWith(ApplicationDisposables);

		GameEngine.Clock.Start().DisposeWith(ApplicationDisposables);
		GameEngine.OnLoad();
		GameEngine.OnApplicationInit(applicationResources, ApplicationDisposables);
	}

	public void OnDeviceInit(DeviceInitInfo info) {
		GameEngine.OnDeviceInit(info.ApplicationResources, info.DeviceResources, DeviceDisposables);
	}

	public void OnDeviceDispose(DeviceDisposeInfo info) {
		ResizeDisposables.Clear();
		DeviceDisposables.Clear();
	}

	public void Dispose() {
		ResizeDisposables.Dispose();
		DeviceDisposables.Dispose();
		ApplicationDisposables.Dispose();
	}

	public void OnKeyDown(KeyDownEvent @event) {
		if (@event.ContinueProcessing && @event.IsNotRepeated) {
			switch (@event.HardwareKey) {
				case KeyW: GameEngine.Stick1.Up = true; break;
				case KeyA: GameEngine.Stick1.Left = true; break;
				case KeyS: GameEngine.Stick1.Down = true; break;
				case KeyD: GameEngine.Stick1.Right = true; break;

				case ArrowUp: GameEngine.Stick2.Up = true; break;
				case ArrowLeft: GameEngine.Stick2.Left = true; break;
				case ArrowDown: GameEngine.Stick2.Down = true; break;
				case ArrowRight: GameEngine.Stick2.Right = true; break;

				default: return;
			}

			@event.StopProcessing();
		}
	}

	public void OnKeyUp(KeyUpEvent @event) {
		if (@event.ContinueProcessing) {
			switch (@event.HardwareKey) {
				case KeyW: GameEngine.Stick1.Up = false; break;
				case KeyA: GameEngine.Stick1.Left = false; break;
				case KeyS: GameEngine.Stick1.Down = false; break;
				case KeyD: GameEngine.Stick1.Right = false; break;

				case ArrowUp: GameEngine.Stick2.Up = false; break;
				case ArrowLeft: GameEngine.Stick2.Left = false; break;
				case ArrowDown: GameEngine.Stick2.Down = false; break;
				case ArrowRight: GameEngine.Stick2.Right = false; break;

				default: return;
			}

			@event.StopProcessing();
		}
	}

	public void OnRender(RenderInfo info) {
		if (Resized) {
			Resized = false;
			GameEngine.OnResizeInit(info.ApplicationResources, info.DeviceResources, ResizeDisposables);
		}

		// Time keeping.
		GameEngine.Clock.Update();

		// Game logic.
		GameEngine.OnUpdate();
		GameEngine.OnRender(info.ApplicationResources, info.DeviceResources);

		// Arrange next frame.
		WindowBridge.RequestRender();
	}
}
