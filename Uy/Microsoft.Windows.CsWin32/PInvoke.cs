using System.Runtime.CompilerServices;
using Windows.Win32.Foundation;

namespace Windows.Win32;

static partial class PInvoke {
	internal static readonly HWND HWND_TOP = new(0);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort LOWORD(uint value) => unchecked((ushort) (value & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort LOWORD(int value) => unchecked((ushort) (value & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort HIWORD(uint value) => unchecked((ushort) ((value >> 16) & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static ushort HIWORD(int value) => unchecked((ushort) ((value >> 16) & 0xffff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static short GET_WHEEL_DELTA_WPARAM(uint value) => unchecked((short) HIWORD(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static short GET_X_LPARAM(int value) => unchecked((short) LOWORD(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static short GET_Y_LPARAM(int value) => unchecked((short) HIWORD(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static bool GET_KEY_EXTENDED_LPARAM(int value) => ((value >> 24) & 0x1) == 1;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static int GET_KEY_REPEATCOUNT_LPARAM(int value) => value & 0xffff;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static byte GET_KEY_SCANCODE_LPARAM(int value) => unchecked((byte) ((value >> 16) & 0xff));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static int GET_KEY_PREVIOUS_LPARAM(int value) => (value >> 30) & 0x1;
}
