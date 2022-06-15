using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace Uy;

/**
<remarks>
	<para>
		<see cref="GameLoopScheduler"/> <b>IS</b> thread-safe.
	</para>
</remarks>
**/
class GameLoopScheduler : IScheduler/*, ISchedulerPeriodic*/ {
	readonly ActionQueue Queue = new();

	public DateTimeOffset Now { get; set; } = DateTimeOffset.Now;

	public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) => Schedule(state, Now, action);
	public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) => Schedule(state, Now + dueTime, action);
	public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) {
		var scheduledAction = new ScheduledAction<TState>(state, dueTime, action);

		lock (Queue)
			Queue.Enqueue(scheduledAction);

		return Disposable.Create(() => {
			lock (Queue)
				Queue.Remove(scheduledAction);
		});
	}

	public void Flush() {
		lock (Queue)
			while (Queue.TryDequeue(Now, out var action))
				action.Invoke(this);
	}

	#region Inner gears

	abstract class ScheduledAction : IComparable<ScheduledAction> {
		public readonly DateTimeOffset DueTime;

		public ScheduledAction(DateTimeOffset dueTime) => DueTime = dueTime;

		public int CompareTo(ScheduledAction? other) =>
			other == null ? 1 : DueTime.CompareTo(other.DueTime);

		public abstract IDisposable Invoke(IScheduler scheduler);
	}

	class ScheduledAction<TState> : ScheduledAction {
		readonly TState State;
		readonly Func<IScheduler, TState, IDisposable> Action;

		public ScheduledAction(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) : base(dueTime) {
			State = state;
			Action = action;
		}

		public override IDisposable Invoke(IScheduler scheduler) =>
			Action.Invoke(scheduler, State);
	}

	/**
	<remarks>
		<para>
			<see cref="ActionQueue"/> is <b>NOT thread-safe</b>.
		</para>
	</remarks>
	**/
	class ActionQueue {
		ScheduledAction[] Items = new ScheduledAction[16];
		int ItemCount = 0;

		/**
		<summary>
			<para>
				Tries to dequeue the first action
				that was scheduled to run before or at <paramref name="dueTime"/>.
			</para>
		</summary>
		**/
		public bool TryDequeue(DateTimeOffset dueTime, [NotNullWhen(true)] out ScheduledAction? action) {
			if (ItemCount == 0 || dueTime < Items[0].DueTime) {
				action = default;
				return false;
			}

			else {
				action = Items[0];
				RemoveAt(0);
				return true;
			}
		}

		public void Enqueue(ScheduledAction action) {
			// Resize up if needed.
			if (Items.Length <= ItemCount)
				Array.Resize(ref Items, Items.Length * 2);

			var i = ItemCount++;

			Items[i] = action;
			Percolate(i);
		}

		public void Remove(ScheduledAction action) {
			// It's sorted by .DueTime, right?
			// So why would I loop through the entire array?
			var i = Array.BinarySearch(Items, 0, ItemCount, action);

			if (0 <= i) RemoveAt(i);
		}

		void RemoveAt(int index) {
			Items[index] = Items[--ItemCount];
			Items[ItemCount] = default!;

			if (Percolate(index) == index)
				Heapify(index);

			// Resize down if needed.
			if (ItemCount < Items.Length / 4)
				Array.Resize(ref Items, Items.Length / 2);
		}

		bool IsHigherPriority(int i, int j) =>
			Items[i].DueTime < Items[j].DueTime;

		int Percolate(int i) {
			if (i < 0 || ItemCount <= i)
				return i;

			var parentI = (i - 1) / 2;
			while (0 <= parentI && parentI != i && IsHigherPriority(i, parentI)) {
				// Swap current and parent.
				(Items[parentI], Items[i]) = (Items[i], Items[parentI]);

				i = parentI;
				parentI = (i - 1) / 2;
			}

			return i;
		}

		void Heapify(int i) {
			if (i >= ItemCount || i < 0)
				return;

			while (true) {
				var leftI = 2 * i + 1;
				var rightI = 2 * i + 2;
				var firstI = i;

				if (leftI < ItemCount && IsHigherPriority(leftI, firstI))
					firstI = leftI;

				if (rightI < ItemCount && IsHigherPriority(rightI, firstI))
					firstI = rightI;

				if (firstI == i)
					break;

				// Swap current and first.
				(Items[firstI], Items[i]) = (Items[i], Items[firstI]);

				i = firstI;
			}
		}
	}

	#endregion
}
