using System;
using System.Reactive.Linq;

namespace Uy;

public static class ObservableExtensions {
	/**
	<inheritdoc cref="System.ObservableExtensions.Subscribe{T}(IObservable{T}, Action{T})"/>
	**/
	public static IDisposable Subscribe<T>(this IObservable<T> source, Action onNext) => source.Subscribe(_ => onNext());

	/**
	<inheritdoc cref="System.ObservableExtensions.Subscribe{T}(IObservable{T}, Action{T})"/>
	<remarks>
		<para>
			This override discards the returned value of <paramref name="onNext"/>
			instead of blocking the compilation when it's a method that doesn't return <see cref="void"/>.
		</para>
	</remarks>
	**/
	public static IDisposable Subscribe<T, T_>(this IObservable<T> source, Func<T_> onNext) => source.Subscribe(__ => _ = onNext());
}
