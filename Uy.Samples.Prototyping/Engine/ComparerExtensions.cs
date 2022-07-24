using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ComparerExtensions {
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static int CompareTo<T>(this T @this, T other, IComparer<T> comparer) => comparer.Compare(@this, other);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool Equals<T>(this T @this, T other, IComparer<T> comparer) => comparer.Compare(@this, other) == 0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsGreaterThan<T>(this T @this, T other, IComparer<T> comparer) => 0 < comparer.Compare(@this, other);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsLessThan<T>(this T @this, T other, IComparer<T> comparer) => comparer.Compare(@this, other) < 0;
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsGreaterOrEquals<T>(this T @this, T other, IComparer<T> comparer) => 0 <= comparer.Compare(@this, other);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsLessOrEquals<T>(this T @this, T other, IComparer<T> comparer) => comparer.Compare(@this, other) <= 0;
}
