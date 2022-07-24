using LinqToYourDoom;
using System;
using System.Numerics;
using Uy;

struct Stick {
	public Vector2 Vector;

	const float Cos45 = MathD.SQRT1_2F;

	public bool Up {
		set {
			if (value == false) Down = true;
			else Vector = Vector switch {
				(00, 00) => new(0, -1),
				(>0, 00) => new(Cos45, -Cos45),
				(>0, >0) => new(1, 0),
				(00, >0) => new(0, 0),
				(<0, >0) => new(-1, 0),
				(<0, 00) => new(-Cos45, -Cos45),
				(<0, <0) => new(-Cos45, -Cos45),
				(00, <0) => new(0, 1),
				(>0, <0) => new(Cos45, -Cos45),

				_ => default
			};
		}
	}

	public bool Down {
		set {
			if (value == false) Up = true;
			else Vector = Vector switch {
				(00, 00) => new(0, 1),
				(>0, 00) => new(Cos45, Cos45),
				(>0, >0) => new(Cos45, Cos45),
				(00, >0) => new(0, 1),
				(<0, >0) => new(-Cos45, Cos45),
				(<0, 00) => new(-Cos45, Cos45),
				(<0, <0) => new(-1, 0),
				(00, <0) => new(0, 0),
				(>0, <0) => new(1, 0),

				_ => default
			};
		}
	}

	public bool Left {
		set {
			if (value == false) Right = true;
			else Vector = Vector switch {
				(00, 00) => new(-1, 0),
				(>0, 00) => new(0, 0),
				(>0, >0) => new(0, 1),
				(00, >0) => new(-Cos45, Cos45),
				(<0, >0) => new(-Cos45, Cos45),
				(<0, 00) => new(-1, 0),
				(<0, <0) => new(-Cos45, -Cos45),
				(00, <0) => new(-Cos45, -Cos45),
				(>0, <0) => new(0, -1),

				_ => default
			};
		}
	}

	public bool Right {
		set {
			if (value == false) Left = true;
			else Vector = Vector switch {
				(00, 00) => new(1, 0),
				(>0, 00) => new(1, 0),
				(>0, >0) => new(Cos45, Cos45),
				(00, >0) => new(Cos45, Cos45),
				(<0, >0) => new(0, 1),
				(<0, 00) => new(0, 0),
				(<0, <0) => new(0, -1),
				(00, <0) => new(Cos45, -Cos45),
				(>0, <0) => new(Cos45, -Cos45),

				_ => default
			};
		}
	}
}
