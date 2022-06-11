using LinqToYourDoom;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Uy;

public interface IResourceDictionary {
	/**
	<summary>
		<para>
			Gets the resource registered under the specified <paramref name="key"/>.
		</para>
	</summary>
	<exception cref="KeyNotFoundException">
		<para>
			No resource is registered under <paramref name="key"/>.
		</para>
	</exception>
	<exception cref="InvalidCastException">
		<para>
			There is a resource registered under <paramref name="key"/>,
			but it isn't of type <typeparamref name="T"/>.
		</para>
	</exception>
	**/
	T Get<T>(Symbol<T> key);

	/**
	<summary>
		<para>
			Attempts to get the resource registered under the specified <paramref name="key"/>.
		</para>
	</summary>
	<exception cref="InvalidCastException">
		<para>
			There is a resource registered under <paramref name="key"/>,
			but it isn't of type <typeparamref name="T"/>.
		</para>
	</exception>
	<returns>
		<para>
			<see langword="true"/> if a resource is found,
			or <see langword="false"/> if no resource is registered under <paramref name="key"/>,
			in which case <paramref name="value"/> is set to <c><see langword="default"/>(<typeparamref name="T"/>)</c>.
		</para>
	</returns>
	**/
	bool TryGet<T>(Symbol<T> key, [NotNullWhen(true)] out T? value);

	/**
	<summary>
		<para>
			Removes the resource registered under the specified <paramref name="key"/>.
			If the resource implements <see cref="IDisposable"/>, it is disposed immediately.
		</para>
	</summary>
	<returns>
		<para>
			<see langword="true"/> if the resource is removed,
			or <see langword="false"/> if no resource is registered under <paramref name="key"/>.
		</para>
	</returns>
	**/
	bool Remove(Symbol key);

	/**
	<summary>
		<para>
			Registers a <paramref name="value">resource</paramref>
			under the specified <paramref name="key"/>,
			<see cref="Remove(Symbol)">overriding</see> any existing resource.
		</para>
	</summary>
	<remarks>
		<para>
			<see cref="UncheckedSet{T}(Symbol{T}, T)"/> is said to be <i>unchecked</i>
			because it <b>does not validate the device-dependency of the new resource</b>,
			therefore allowing, for instance, a device-dependent resource
			to be added to a <see cref="IDeviceIndependentResourceDictionary"/>,
			or vice versa.
		</para>
	</remarks>
	<returns>
		<para>
			<paramref name="value"/>.
		</para>
	</returns>
	**/
	T UncheckedSet<T>(Symbol<T> key, T value) where T : notnull;
}
