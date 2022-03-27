using System.Numerics;

namespace Uy;

public static class Vector3Extensions {
	public static void Deconstruct(this Vector3 @this, out float x, out float y, out float z) {
		x = @this.X;
		y = @this.Y;
		z = @this.Z;
	}
}
