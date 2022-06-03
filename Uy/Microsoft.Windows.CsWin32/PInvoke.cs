using System.Runtime.CompilerServices;
using Windows.Win32.Foundation;

namespace Windows.Win32;

static partial class PInvoke {
	internal static readonly HWND HWND_TOP = new(0);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort LOWORD(nuint value) => unchecked((ushort) (value & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort LOWORD(nint value) => unchecked((ushort) (value & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort HIWORD(nuint value) => unchecked((ushort) ((value >> 16) & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort HIWORD(nint value) => unchecked((ushort) ((value >> 16) & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static short GET_WHEEL_DELTA_WPARAM(nuint value) => unchecked((short) HIWORD(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static short GET_X_LPARAM(nint value) => unchecked((short) LOWORD(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static short GET_Y_LPARAM(nint value) => unchecked((short) HIWORD(value));
}
