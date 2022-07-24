using Vortice.Mathematics;

namespace Uy;

public static class Box4Extensions {
	public static Rect ToRect(this Box4 @this) =>
		new(@this.Left, @this.Top, @this.Width, @this.Height);
}
