using System.Numerics;

namespace Uy;

public static class Vector4Extensions {
	public static void Deconstruct(this Vector4 @this, out float x, out float y, out float z, out float w) {
		x = @this.X;
		y = @this.Y;
		z = @this.Z;
		w = @this.W;
	}
}
