using LinqToYourDoom;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Uy;

class ResourceDictionary : Dictionary<Symbol, object>, IResourceDictionary, IDisposable {
	public T Get<T>(Symbol<T> key) => (T) this[key];

	public bool TryGet<T>(Symbol<T> key, [NotNullWhen(true)] out T? value) {
		if (TryGetValue(key, out var v)) {
			value = (T) v;
			return true;
		}

		else {
			value = default;
			return false;
		}
	}
	public new bool Remove(Symbol key) {
		if (Remove(key, out var value)) {
			if (value is IDisposable disposable)
				disposable.Dispose();

			return true;
		}

		else return false;
	}

	public T UncheckedSet<T>(Symbol<T> key, T value) where T : notnull {
		Remove(key); // Dispose any existing value.
		Add(key, value);

		return value;
	}

	public virtual void Dispose() {
		Values.OfType<IDisposable>().DisposeAll();
		Clear();
	}
}
