using System;
using System.Reactive.Concurrency;

namespace Uy;

class GameLoopScheduler : VirtualTimeScheduler<DateTimeOffset, TimeSpan> {
	protected override DateTimeOffset Add(DateTimeOffset absolute, TimeSpan relative) => absolute + relative;
	protected override DateTimeOffset ToDateTimeOffset(DateTimeOffset absolute) => absolute;
	protected override TimeSpan ToRelative(TimeSpan timeSpan) => timeSpan;
}
