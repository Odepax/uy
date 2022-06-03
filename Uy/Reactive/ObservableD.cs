using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Uy;

/**
<summary>
	<para>
		D stands for Doom, of course...
	</para>
</summary>
**/
public static class ObservableD {
	public static IObservable<Unit> UnitMerge<T1, T2>(IObservable<T1> a, IObservable<T2> b) =>
		Observable.Merge(
			a.Select(_ => Unit.Default),
			b.Select(_ => Unit.Default)
		);

	public static IObservable<Unit> UnitMerge<T1, T2, T3>(IObservable<T1> a, IObservable<T2> b, IObservable<T3> c) =>
		Observable.Merge(
			a.Select(_ => Unit.Default),
			b.Select(_ => Unit.Default),
			c.Select(_ => Unit.Default)
		);

	public static IObservable<Unit> UnitMerge<T1, T2, T3, T4>(IObservable<T1> a, IObservable<T2> b, IObservable<T3> c, IObservable<T4> d) =>
		Observable.Merge(
			a.Select(_ => Unit.Default),
			b.Select(_ => Unit.Default),
			c.Select(_ => Unit.Default),
			d.Select(_ => Unit.Default)
		);

	public static IObservable<Unit> UnitMerge<T1, T2, T3, T4, T5>(IObservable<T1> a, IObservable<T2> b, IObservable<T3> c, IObservable<T4> d, IObservable<T5> e) =>
		Observable.Merge(
			a.Select(_ => Unit.Default),
			b.Select(_ => Unit.Default),
			c.Select(_ => Unit.Default),
			d.Select(_ => Unit.Default),
			e.Select(_ => Unit.Default)
		);

	public static IObservable<Unit> UnitMerge<T1, T2, T3, T4, T5, T6>(IObservable<T1> a, IObservable<T2> b, IObservable<T3> c, IObservable<T4> d, IObservable<T5> e, IObservable<T6> f) =>
		Observable.Merge(
			a.Select(_ => Unit.Default),
			b.Select(_ => Unit.Default),
			c.Select(_ => Unit.Default),
			d.Select(_ => Unit.Default),
			e.Select(_ => Unit.Default),
			f.Select(_ => Unit.Default)
		);
}
