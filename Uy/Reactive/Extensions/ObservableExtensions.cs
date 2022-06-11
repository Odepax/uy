using System;
using System.Reactive.Linq;

namespace Uy;

public static class ObservableExtensions {
	/**
	<inheritdoc cref="System.ObservableExtensions.Do{T}(IObservable{T}, Action{T})"/>
	**/
	public static IObservable<T> Do<T>(this IObservable<T> source, Action onNext) => source.Do(_ => onNext.Invoke());

	/**
	<inheritdoc cref="System.ObservableExtensions.Do{T}(IObservable{T}, Action{T})"/>
	<remarks>
		<para>
			This override discards the returned value of <paramref name="onNext"/>
			instead of blocking the compilation when it's a method that doesn't return <see cref="void"/>.
		</para>
	</remarks>
	**/
	public static IObservable<T> Do<T, T_>(this IObservable<T> source, Func<T_> onNext) => source.Do(__ => _ = onNext.Invoke());

	/**
	<inheritdoc cref="System.ObservableExtensions.Subscribe{T}(IObservable{T}, Action{T})"/>
	**/
	public static IDisposable Subscribe<T>(this IObservable<T> source, Action onNext) => source.Subscribe(_ => onNext.Invoke());

	/**
	<inheritdoc cref="System.ObservableExtensions.Subscribe{T}(IObservable{T}, Action{T})"/>
	<remarks>
		<para>
			This override discards the returned value of <paramref name="onNext"/>
			instead of blocking the compilation when it's a method that doesn't return <see cref="void"/>.
		</para>
	</remarks>
	**/
	public static IDisposable Subscribe<T, T_>(this IObservable<T> source, Func<T_> onNext) => source.Subscribe(__ => _ = onNext.Invoke());

	/**
	<inheritdoc cref="System.ObservableExtensions.Subscribe{T}(IObservable{T}, Action{T})"/>
	**/
	public static IDisposable Subscribe<T1, T2>(this IObservable<(T1, T2)> source, Action<T1, T2> onNext) => source.Subscribe(tuple => onNext.Invoke(tuple.Item1, tuple.Item2));

	/**
	<inheritdoc cref="System.ObservableExtensions.Subscribe{T}(IObservable{T}, Action{T})"/>
	**/
	public static IDisposable Subscribe<T1, T2, T3>(this IObservable<(T1, T2, T3)> source, Action<T1, T2, T3> onNext) => source.Subscribe(tuple => onNext.Invoke(tuple.Item1, tuple.Item2, tuple.Item3));
}
