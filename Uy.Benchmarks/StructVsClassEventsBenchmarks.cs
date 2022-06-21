using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Uy.Benchmarks;

// + Consider UI events as immutable value types.
// => E OnMouseEnter(E e) {
//    // tunel
//
//    e = child.OnMouseEnter(e with { Position = e.Position.RelativeTo(child) }); // struct event
//    // -VS-
//    using (e.Scope(e.Position.RelativeTo(child))) child.OnMouseEnter(e); // class event
//
//    // bubble
// }
// => Looks like a case for a benchmark!

/*

MouseDownEvent: 10ns
MouseDownEvent_singleton: 5ns
ValueMouseDownEvent: 17ns
ValueMouseDownEvent_in: 15ns

*/

class UserInputEvent {
	public bool ContinueProcessing { get; private set; } = true;

	protected UserInputEvent() { }

	public void StopProcessing() => ContinueProcessing = false;
}

class MouseEvent : UserInputEvent {
	public Vector2 Position { get; private set; }

	protected MouseEvent(Vector2 position) => Position = position;

	public PScope PositionScope(Vector2 scopedPosition) {
		var savedPosition = Position;

		Position = scopedPosition;

		return new PScope(this, savedPosition);
	}

	public readonly struct PScope : IDisposable {
		readonly MouseEvent Event;
		readonly Vector2 SavedPosition;

		public PScope(MouseEvent @event, Vector2 savedPosition) {
			Event = @event;
			SavedPosition = savedPosition;
		}

		public void Dispose() => Event.Position = SavedPosition;
	}
}

class MouseDownEvent : MouseEvent {
	public readonly ConsoleKey Button;
	public readonly bool IsDoubleClick;

	public MouseDownEvent(Vector2 position, ConsoleKey button, bool isDoubleClick) : base(position) {
		Button = button;
		IsDoubleClick = isDoubleClick;
	}
}

class SingletonUserInputEvent {
	public bool ContinueProcessing { get; internal set; } = true;

	protected SingletonUserInputEvent() {}

	public void StopProcessing() => ContinueProcessing = false;
}

class SingletonMouseEvent : SingletonUserInputEvent {
	public Vector2 Position { get; internal set; }

	protected SingletonMouseEvent() {}

	public PScope PositionScope(Vector2 scopedPosition) {
		var savedPosition = Position;

		Position = scopedPosition;

		return new PScope(this, savedPosition);
	}

	public readonly struct PScope : IDisposable {
		readonly SingletonMouseEvent Event;
		readonly Vector2 SavedPosition;

		public PScope(SingletonMouseEvent @event, Vector2 savedPosition) {
			Event = @event;
			SavedPosition = savedPosition;
		}

		public void Dispose() => Event.Position = SavedPosition;
	}
}

class SingletonMouseDownEvent : SingletonMouseEvent {
	public ConsoleKey Button { get; internal set; }
	public bool IsDoubleClick { get; internal set; }
}

// A ref struct to wrap the singleton, preventing from keeping a reference to the event object.
readonly ref struct MouseDownEventWrapper {
	readonly SingletonMouseDownEvent Event;
	internal MouseDownEventWrapper(SingletonMouseDownEvent @event) => Event = @event;

	public bool ContinueProcessing => Event.ContinueProcessing;
	public Vector2 Position => Event.Position;
	public ConsoleKey Button => Event.Button;
	public bool IsDoubleClick => Event.IsDoubleClick;
	public void StopProcessing() => Event.StopProcessing();
	public SingletonMouseEvent.PScope PositionScope(Vector2 scopedPosition) => Event.PositionScope(scopedPosition);
}

readonly ref struct ValueMouseDownEvent {
	public bool ContinueProcessing { get; init; } = true;

	public Vector2 Position { get; init; }
	public ConsoleKey Button { get; init; }
	public bool IsDoubleClick { get; init; }

	public ValueMouseDownEvent(Vector2 position, ConsoleKey button, bool isDoubleClick) {
		Position = position;
		Button = button;
		IsDoubleClick = isDoubleClick;
	}
}

class SampleCompositeControl {
	public readonly Vector2 Position;
	public readonly List<SampleCompositeControl> Children = new();

	public SampleCompositeControl(Vector2 position) => Position = position;

	public void OnMouseDown(MouseDownEvent @event) {
		foreach (var child in Children)
			using (@event.PositionScope(@event.Position - child.Position))
				child.OnMouseDown(@event);

		@event.StopProcessing();
	}

	public void OnMouseDown(MouseDownEventWrapper @event) {
		foreach (var child in Children)
			using (@event.PositionScope(@event.Position - child.Position))
				child.OnMouseDown(@event);

		@event.StopProcessing();
	}

	public ValueMouseDownEvent OnMouseDown(ValueMouseDownEvent @event) {
		foreach (var child in Children)
			@event = child.OnMouseDown(@event with { Position = @event.Position - child.Position });

		return @event with { ContinueProcessing = false };
	}

	public ValueMouseDownEvent OnMouseDown(in ValueMouseDownEvent @event) {
		var e = @event;

		foreach (var child in Children) {
			e = e with { Position = @event.Position - child.Position };
			e = child.OnMouseDown(in e);
		}

		return @event with { ContinueProcessing = false };
	}
}

public class StructVsClassEventsBenchmarks {
	[Params(10, 100)] public int Depth { get; set; }
	[Params(10, 100)] public int Breadth { get; set; }

	readonly Random random = new(666);
	readonly SampleCompositeControl RootControl;

	public StructVsClassEventsBenchmarks() => RootControl = NewSampleControl(0);

	SampleCompositeControl NewSampleControl(int depth) {
		var control = new SampleCompositeControl(new Vector2(random.NextSingle()));

		if (depth < Depth)
			for (var i = 0; i < Breadth; ++i)
				control.Children.Add(NewSampleControl(depth + 1));

		return control;
	}

	[Benchmark]
	public bool MouseDownEvent() {
		var @event = new MouseDownEvent(new Vector2(42, 42), ConsoleKey.Enter, false);

		RootControl.OnMouseDown(@event);

		return @event.ContinueProcessing;
	}

	static readonly SingletonMouseDownEvent Event = new();

	[Benchmark]
	public bool MouseDownEvent_singleton() {
		Event.ContinueProcessing = true;
		Event.Position = new Vector2(42, 42);
		Event.Button = ConsoleKey.Enter;
		Event.IsDoubleClick = false;

		var @event = new MouseDownEventWrapper(Event);

		RootControl.OnMouseDown(@event);

		return Event.ContinueProcessing;
	}

	[Benchmark]
	public bool ValueMouseDownEvent() {
		var @event = new ValueMouseDownEvent(new Vector2(42, 42), ConsoleKey.Enter, false);

		@event = RootControl.OnMouseDown(@event);

		return @event.ContinueProcessing;
	}

	[Benchmark]
	public bool ValueMouseDownEvent_in() {
		var @event = new ValueMouseDownEvent(new Vector2(42, 42), ConsoleKey.Enter, false);

		@event = RootControl.OnMouseDown(in @event);

		return @event.ContinueProcessing;
	}
}
