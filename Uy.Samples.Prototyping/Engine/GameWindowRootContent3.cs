using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Uy;
using static Uy.Key;

class GameWindowRootContent3 : IWindowRootContent, IDisposable {
	readonly Game3 GameEngine;
	readonly IWindowBridge WindowBridge;
	readonly List<DeviceResourceFactory> DeviceResources = new();
	readonly List<VolatileResourceDescriptor> VolatileResources = new();
	readonly CompositeDisposable ApplicationDisposables = new();
	readonly CompositeDisposable DeviceDisposables = new();

	public GameWindowRootContent3(Game3 gameEngine, IDeviceIndependentResourceDictionary applicationResources, IWindowBridge windowBridge) {
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
				GameEngine.Window.DpiScale = WindowBridge.DpiScale;
			})
			.DisposeWith(ApplicationDisposables);

		var ApplicationResources = new List<ApplicationResourceFactory>();
		var R = new ResourceCollection2(ApplicationResources, DeviceResources, VolatileResources, WindowBridge.GameLoopScheduler);

		GameEngine.Clock.Start(WindowBridge.GameLoopScheduler).DisposeWith(ApplicationDisposables);
		GameEngine.OnLoad(ApplicationDisposables);
		GameEngine.OnLoad(in R, ApplicationDisposables);

		foreach (var factory in ApplicationResources)
			factory.Invoke(applicationResources, ApplicationDisposables);
	}

	public void OnDeviceInit(DeviceInitInfo info) {
		foreach (var factory in DeviceResources)
			factory.Invoke(info.ApplicationResources, info.DeviceResources, DeviceDisposables);
	}

	public void OnDeviceDispose(DeviceDisposeInfo info) => DeviceDisposables.Dispose();
	public void Dispose() => ApplicationDisposables.Dispose();

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
		// Time keeping.
		GameEngine.Clock.Update();

		// Resources maintenance.
		foreach(var descriptor in VolatileResources)
			descriptor.Sync(info.ApplicationResources, info.DeviceResources, ApplicationDisposables, DeviceDisposables);

		// Game logic.
		var _updateStartTime = GameEngine.Clock.ElapsedMilliseconds;
		{
			GameEngine.OnUpdate(info.ApplicationResources, info.DeviceResources);
		}
		var _updateEndTime = GameEngine.Clock.ElapsedMilliseconds;

		GameEngine.Clock.UpdateDuration = _updateEndTime - _updateStartTime;

		GameEngine.OnRender(info.ApplicationResources, info.DeviceResources);

		// Arrange next frame.
		WindowBridge.RequestRender();
	}
}
