using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Uy;
using static Uy.Key;

class GameWindowRootContent2 : IWindowRootContent, IDisposable {
	readonly Game2 GameEngine;
	readonly IWindowBridge WindowBridge;
	readonly CompositeDisposable GameDisposables = new();
	readonly List<ResourceDescriptor> DeviceResources = new();
	readonly List<ResourceDescriptor> RepopulatingResources = new();

	public GameWindowRootContent2(Game2 gameEngine, IDeviceIndependentResourceDictionary applicationResources, IWindowBridge windowBridge) {
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
			.DisposeWith(GameDisposables);

		GameEngine.Clock.Start(WindowBridge.GameLoopScheduler).DisposeWith(GameDisposables);
		GameEngine.OnLoad(GameDisposables);

		var ApplicationResources = new List<ResourceDescriptor>();
		var RC = new ResourceCollection(ApplicationResources, DeviceResources, RepopulatingResources, WindowBridge.GameLoopScheduler);
		GameEngine.OnLoad(RC, GameDisposables);

		var RP1 = new ResourceProvider1(applicationResources);
		foreach (var descriptor in ApplicationResources)
			descriptor.Populate(RP1, applicationResources);
	}

	public void OnDeviceInit(DeviceInitInfo info) {
		var RP2 = new ResourceProvider2(info.ApplicationResources, info.DeviceResources);
		foreach (var descriptor in DeviceResources)
			descriptor.Populate(RP2, info.DeviceResources);
	}

	public void OnDeviceDispose(DeviceDisposeInfo info) {}

	public void Dispose() => GameDisposables.Dispose();

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
		var RP1 = new ResourceProvider1(info.ApplicationResources);
		var RP2 = new ResourceProvider2(info.ApplicationResources, info.DeviceResources);

		foreach(var descriptor in RepopulatingResources)
			descriptor.Repopulate(RP1, RP2, info.ApplicationResources, info.DeviceResources);

		// Game logic.
		var _updateStartTime = GameEngine.Clock.ElapsedMilliseconds;
		{
			GameEngine.OnUpdate(RP2);
		}
		var _updateEndTime = GameEngine.Clock.ElapsedMilliseconds;

		GameEngine.Clock.UpdateDuration = _updateEndTime - _updateStartTime;

		GameEngine.OnRender(RP2);

		// Arrange next frame.
		WindowBridge.RequestRender();
	}
}
