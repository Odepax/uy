using System.Runtime.CompilerServices;

namespace Uy;

public static class AngleFExtensions {
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static AngleF ToDegrees(this float @this) => AngleF.FromDegrees(@this);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static AngleF ToRadians(this float @this) => AngleF.FromRadians(@this);
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static AngleF ToTurns(this float @this) => AngleF.FromTurns(@this);
}
