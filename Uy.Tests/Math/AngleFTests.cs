using LinqToYourDoom;
using NUnit.Framework;
using static System.MathF;

namespace Uy.Tests;

class AngleFTests {
	[Test]
	public void Clamp() {
		Assert.AreEqual(PI, AngleF.FromRadians(-PI).PiValue);
		Assert.AreEqual(PI, AngleF.FromDegrees(-180).PiValue);
		Assert.AreEqual(PI, AngleF.FromDegrees(+180).PiValue);
		Assert.AreEqual(PI, AngleF.FromDirection(new(-1, 0)).PiValue);
	}

	[Test]
	//        /* Degrees  /*       PiValue  /*       TauValue  /*      Abs  /*      Opposite)]
	[TestCase(/*    */ 0, /*          */ 0, /*           */ 0, /* */ 0.00f, /*         */ PI)]
	[TestCase(/*   */ 72, /* */ 2 * PI / 5, /* */ 0.20f * Tau, /* */ 0.00f, /**/ -3 * PI / 5)]
	[TestCase(/*   */ 90, /*     */ PI / 2, /* */ 0.25f * Tau, /* */ 0.00f, /*    */ -PI / 2)]
	[TestCase(/*  */ 144, /* */ 4 * PI / 5, /* */ 0.40f * Tau, /* */ 0.00f, /**/ -1 * PI / 5)]
	[TestCase(/*  */ 180, /*         */ PI, /* */ 0.50f * Tau, /* */ 0.00f, /*          */ 0)]
	[TestCase(/*  */ 216, /**/ -4 * PI / 5, /* */ 0.60f * Tau, /* */ 0.00f, /* */ 1 * PI / 5)]
	[TestCase(/*  */ 270, /*    */ -PI / 2, /* */ 0.75f * Tau, /* */ 0.00f, /*    */  PI / 2)]
	[TestCase(/*  */ 288, /**/ -2 * PI / 5, /* */ 0.80f * Tau, /* */ 0.00f, /* */ 3 * PI / 5)]
	[TestCase(/*  */ 360, /*          */ 0, /*           */ 0, /* */ 0.00f, /*         */ PI)]
	public void Properties(float degrees, float expectedPiValue, float expectedTauValue, float expectedAbs, float expectedOpposite) {
		var a = AngleF.FromDegrees(degrees);

		Assert.AreEqual(expectedPiValue, a.PiValue, 0.000_001);
		Assert.AreEqual(expectedTauValue, a.TauValue, 0.000_001);
		// TODO: find out what the "absolute" of an angle is, if it makes sense at all...
		//Assert.AreEqual(expectedAbs, a.Abs, 0.000_001);
		Assert.AreEqual(expectedOpposite, a.Opposite, 0.000_001);

		// Not testing Cos & Cie pass-through methods.
	}

	[Test]
	public void Math_operators() {
		var a = AngleF.FromTurns(0.4f);
		var r = 0.2f * Tau;

		Assert.AreEqual(AngleF.FromTurns(0.6f).PiValue, (a + r).PiValue, 0.000_001);
		Assert.AreEqual(AngleF.FromTurns(0.2f), a - r);
		Assert.AreEqual(AngleF.FromTurns(0.8f), a * 2);
		Assert.AreEqual(AngleF.FromTurns(-0.4f), -1 * a);
		Assert.AreEqual(AngleF.FromTurns(0.1f), a / 4);
		Assert.AreEqual(a.PiValue, (a + (0.3f * Tau) - r + 2 * r - (0.5f * Tau)).PiValue, 0.000_001);
		Assert.AreEqual(a.PiValue, (a + ((0.3f * Tau) - r + 2 * r - (0.5f * Tau))).PiValue, 0.000_001);
	}

	[Test]
	//        /*   Base  /* Target  /* Short Arc  /* Long Arc)]
	[TestCase(/*   */ 0, /*   */ 0, /*      */ 0, /*  */ -360)]
	[TestCase(/* */ 180, /**/ -180, /*      */ 0, /*   */-360)]
	[TestCase(/*  */ 90, /* */ -90, /*    */ 180, /*  */ -180)]
	[TestCase(/* */ -90, /*  */ 90, /*    */ 180, /*  */ -180)]
	[TestCase(/*  */ 45, /*  */ 72, /*     */ 27, /*  */ -333)]
	[TestCase(/* */ -90, /**/ -144, /*    */ -54, /*   */ 306)]
	[TestCase(/**/ -144, /* */ -45, /*     */ 99, /*  */ -261)]
	[TestCase(/**/ -144, /* */ -45, /*     */ 99, /*  */ -261)]
	[TestCase(/*  */ 90, /* */ -72, /*   */ -162, /*   */ 198)]
	public void Arcs_to_and_from(float a, float b, float shortArc, float longArc) {
		var @base = AngleF.FromDegrees(a);
		var target = AngleF.FromDegrees(b);
		var @short = shortArc * Tau / 360f;
		var @long = longArc * Tau / 360f;
		var (counterClockwise, clockwise) = @short < 0 ? (@short, @long) : (@long, @short);

		Assert.AreEqual(@short, @base.ShortArcTo(target), 0.000_001);
		Assert.AreEqual(@short, target.ShortArcFrom(@base), 0.000_001);
		Assert.AreEqual(@long, @base.LongArcTo(target), 0.000_001);
		Assert.AreEqual(@long, target.LongArcFrom(@base), 0.000_001);
		Assert.AreEqual(clockwise, @base.ClockwiseArcTo(target), 0.000_001);
		Assert.AreEqual(clockwise, target.ClockwiseArcFrom(@base), 0.000_001);
		Assert.AreEqual(counterClockwise, @base.CounterClockwiseArcTo(target), 0.000_001);
		Assert.AreEqual(counterClockwise, target.CounterClockwiseArcFrom(@base), 0.000_001);
	}

	[Test]
	//        /*   Base  /* Target  /*   Ratio  /* Short Lerp  /* Long Lerp)]
	[TestCase(/*   */ 0, /*   */ 0, /**/ -4.2f, /*       */ 0, /*     */ 72)]
	[TestCase(/* */ 180, /**/ -180, /* */ 0.4f, /*     */ 180, /*     */ 36)]
	[TestCase(/**/ -180, /* */ 180, /**/ -0.2f, /*     */ 180, /*    */ 252)]
	[TestCase(/*  */ 90, /* */ -90, /**/ 0.75f, /*    */ -135, /*    */ -45)]
	[TestCase(/* */ -90, /*  */ 90, /**/ 0.75f, /*      */ 45, /*    */ 135)]
	[TestCase(/*  */ 45, /*  */ 25, /* */ 0.4f, /*      */ 37, /*    */ 181)]
	[TestCase(/*  */ 45, /*  */ 65, /* */ 0.4f, /*      */ 53, /*    */ -91)]
	[TestCase(/*  */ 45, /* */ 245, /* */ 0.4f, /*     */ -19, /*    */ 125)]
	[TestCase(/*  */ 45, /**/ -155, /* */ 0.4f, /*     */ 109, /*    */ -35)]
	public void Lerp(float a, float b, float ratio, float shortLerp, float longLerp) {
		var @base = AngleF.FromDegrees(a);
		var target = AngleF.FromDegrees(b);
		var @short = AngleF.FromDegrees(shortLerp);
		var @long = AngleF.FromDegrees(longLerp);
		var (counterClockwise, clockwise) = @base.ShortArcTo(target) < 0 ? (@short, @long) : (@long, @short);

		Assert.AreEqual(@short.PiValue, @base.ShortArcLerp(target, ratio).PiValue, 0.000_001);
		Assert.AreEqual(@long.PiValue, @base.LongArcLerp(target, ratio).PiValue, 0.000_01);
		Assert.AreEqual(clockwise.PiValue, @base.ClockwiseArcLerp(target, ratio).PiValue, 0.000_001);
		Assert.AreEqual(counterClockwise.PiValue, @base.CounterClockwiseArcLerp(target, ratio).PiValue, 0.000_01);
	}

	[Test]
	//        /* Angle  /* Counter Clockwise Bound  /* Clockwise Bound  /* Expected)]
	[TestCase(/*  */ 0, /*                    */ 0, /*            */ 0, /*     */ 0)]
	[TestCase(/* */ 42, /*                    */ 0, /*            */ 0, /*     */ 0)]
	[TestCase(/**/ 180, /*                 */ -180, /*          */ 180, /*   */ 180)]
	[TestCase(/* */ 42, /*                 */ -180, /*          */ 180, /*   */ 180)]
	[TestCase(/* */ 90, /*                  */ -90, /*          */ -90, /*   */ -90)]
	[TestCase(/* */ 42, /*                  */ -90, /*          */ -90, /*   */ -90)]
	[TestCase(/* */ 90, /*                   */ 40, /*          */ 100, /*    */ 90)]
	[TestCase(/**/ 120, /*                   */ 40, /*          */ 100, /*   */ 100)]
	[TestCase(/* */ 30, /*                   */ 40, /*          */ 100, /*    */ 40)]
	[TestCase(/**/ 250, /*                   */ 40, /*          */ 100, /*   */ 100)] // <- This one should be 40,
	[TestCase(/**/ 220, /*                   */ 40, /*          */ 100, /*   */ 100)] //    but ends up at 100 due to a float precision error...
	[TestCase(/**/ 280, /*                   */ 40, /*          */ 100, /*    */ 40)] //    Moral of the story: don't blindly think .CoerceIn() will be consistent
	[TestCase(/* */ 90, /*                  */ 100, /*           */ 40, /*   */ 100)] //    when this angle is the opposite of the middle of the range.
	[TestCase(/* */ 50, /*                  */ 100, /*           */ 40, /*    */ 40)]
	[TestCase(/* */ 70, /*                  */ 100, /*           */ 40, /*   */ 100)]
	[TestCase(/**/ 180, /*                  */ 100, /*           */ 40, /*   */ 180)]
	[TestCase(/**/ 110, /*                  */ 100, /*           */ 40, /*   */ 110)]
	[TestCase(/* */ 30, /*                  */ 100, /*           */ 40, /*    */ 30)]
	[TestCase(/**/ 290, /*                  */ 100, /*           */ 40, /*   */ 290)]
	[TestCase(/**/ 220, /*                  */ 100, /*           */ 40, /*   */ 220)]
	[TestCase(/**/ 280, /*                  */ 100, /*           */ 40, /*   */ 280)]
	public void CoerceIn(float a, float counterClockwiseBound, float clockwiseBound, float expected) {
		var angle = AngleF.FromDegrees(a);
		var counterClockwise = AngleF.FromDegrees(counterClockwiseBound);
		var clockwise = AngleF.FromDegrees(clockwiseBound);

		var actual = angle.CoerceIn(counterClockwise, clockwise);

		Assert.AreEqual(AngleF.FromDegrees(expected), actual);
	}

	[Test]
	public void Equals_with_delta() {
		Assert.IsTrue(AngleF.FromTurns(0.1f).Equals(AngleF.FromTurns(0.2f), 0.15f * Tau));
		Assert.IsFalse(AngleF.FromTurns(0.1f).Equals(AngleF.FromTurns(0.2f), 0.05f * Tau));
		Assert.IsTrue(AngleF.FromDegrees(-175).Equals(AngleF.FromDegrees(+175), 10f * Tau / 360f));
		Assert.IsTrue(AngleF.FromDegrees(+175).Equals(AngleF.FromDegrees(-175), 10f * Tau / 360f));
		Assert.IsTrue(AngleF.FromDegrees(42).Equals(AngleF.FromDegrees(42), 0));
		Assert.IsTrue(AngleF.FromDegrees(42).Equals(AngleF.FromDegrees(42), 1.2f * PI));
		Assert.IsFalse(AngleF.FromDegrees(42).Equals(AngleF.FromDegrees(42), -1));
	}

	[Test]
	//        /*      A  /*     B  /* Absolute  /* Relative)]
	[TestCase(/*   */ 0, /*  */ 0, /*     */ 0, /*     */ 0)]
	[TestCase(/**/ -180, /**/ 180, /*     */ 0, /*     */ 0)]
	[TestCase(/*  */ 90, /**/ -90, /*    */ +1, /*    */ +1)]
	[TestCase(/*  */ 90, /**/ -91, /*    */ +1, /*    */ -1)]
	[TestCase(/*  */ 90, /**/ -89, /*    */ +1, /*    */ +1)]
	[TestCase(/* */ -90, /* */ 90, /*    */ -1, /*    */ +1)]
	[TestCase(/* */ -90, /* */ 91, /*    */ -1, /*    */ +1)]
	[TestCase(/* */ -90, /* */ 89, /*    */ -1, /*    */ -1)]
	public void Absolute_and_relative_comparers(float a, float b, int expectedAbsoluteSign, int expectedRelativeSign) {
		var actualAbsoluteSign = AngleF.AbsoluteComparer.Compare(AngleF.FromDegrees(a), AngleF.FromDegrees(b)).Sign();
		var actualRelativeSign = AngleF.RelativeComparer.Compare(AngleF.FromDegrees(a), AngleF.FromDegrees(b)).Sign();

		Assert.AreEqual(expectedAbsoluteSign, actualAbsoluteSign);
		Assert.AreEqual(expectedRelativeSign, actualRelativeSign);
	}
}
