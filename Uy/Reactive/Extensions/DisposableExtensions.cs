using System;
using System.Reactive.Disposables;

namespace Uy;

public static class DisposableExtensions {
	/**
	<summary>
		<para>
			Ensures the provided disposable is disposed with the specified <see cref="CompositeDisposable"/>.
		</para>
	</summary>
	<returns>
		<para>
			<paramref name="this"/>
		</para>
	</returns>
	**/
	public static TDisposable DisposeWith<TDisposable>(this TDisposable @this, CompositeDisposable compositeDisposable) where TDisposable : IDisposable {
		compositeDisposable.Add(@this);

		return @this;
	}

	/**
	<summary>
		<para>
			Ensures the provided disposable is disposed with the specified <see cref="SerialDisposable"/>.
		</para>
	</summary>
	<returns>
		<para>
			<paramref name="this"/>
		</para>
	</returns>
	**/
	public static TDisposable DisposeWith<TDisposable>(this TDisposable @this, SerialDisposable compositeDisposable) where TDisposable : IDisposable {
		compositeDisposable.Disposable = @this;

		return @this;
	}

	/**
	<summary>
		<para>
			Ensures the provided disposable is disposed with the specified <see cref="MultipleAssignmentDisposable"/>.
		</para>
	</summary>
	<returns>
		<para>
			<paramref name="this"/>
		</para>
	</returns>
	**/
	public static TDisposable DisposeWith<TDisposable>(this TDisposable @this, MultipleAssignmentDisposable compositeDisposable) where TDisposable : IDisposable {
		compositeDisposable.Disposable = @this;

		return @this;
	}
}
