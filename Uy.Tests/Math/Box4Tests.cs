using NUnit.Framework;
using System;

namespace Uy.Tests;

class Box4Tests {
	[Test]
	[TestCaseSource(nameof(Static_factory_methods_test_cases))]
	public void Static_factory_methods(Box4 actual) {
		var expected = Box4.FromLeftTop(7, 1, 2, 4);

		Assert.AreEqual(expected, actual);
	}

	static readonly TestCaseData[] Static_factory_methods_test_cases = new[] {
		new TestCaseData(Box4.FromLeftTop(7, 1, 2, 4)).SetName(nameof(Box4.FromLeftTop)),
		new TestCaseData(Box4.FromRightTop(9, 1, 2, 4)).SetName(nameof(Box4.FromRightTop)),
		new TestCaseData(Box4.FromRightBottom(9, 5, 2, 4)).SetName(nameof(Box4.FromRightBottom)),
		new TestCaseData(Box4.FromLeftBottom(7, 5, 2, 4)).SetName(nameof(Box4.FromLeftBottom)),
		new TestCaseData(Box4.FromCenter(8, 3, 2, 4)).SetName(nameof(Box4.FromCenter)),
		new TestCaseData(Box4.FromLeftCenter(7, 3, 2, 4)).SetName(nameof(Box4.FromLeftCenter)),
		new TestCaseData(Box4.FromRightCenter(9, 3, 2, 4)).SetName(nameof(Box4.FromRightCenter)),
		new TestCaseData(Box4.FromCenterTop(8, 1, 2, 4)).SetName(nameof(Box4.FromCenterTop)),
		new TestCaseData(Box4.FromCenterBottom(8, 5, 2, 4)).SetName(nameof(Box4.FromCenterBottom)),
		new TestCaseData(Box4.FromLerp(0.50f, 0.25f, 8, 2, 2, 4)).SetName(nameof(Box4.FromLerp)),
		new TestCaseData(Box4.FromTo(7, 1, 9, 5)).SetName(nameof(Box4.FromTo)),
		new TestCaseData(Box4.FromSides(1, 9, 5, 7)).SetName(nameof(Box4.FromSides)),
	};

	[Test]
	[TestCaseSource(nameof(Anchor_coordinates_related_properties_test_cases))]
	[TestCaseSource(nameof(Side_coordinates_related_properties_test_cases))]
	[TestCaseSource(nameof(Size_related_properties_test_cases))]
	[TestCaseSource(nameof(In_place_members_test_cases))]
	public void In_place_members(F f, Box4 expected) {
		var actual = Box4.FromLeftTop(1, 2, 3, 4);

		f(ref actual);

		Assert.AreEqual(expected, actual);
	}

	public delegate void F(ref Box4 box);

	static readonly TestCaseData[] Anchor_coordinates_related_properties_test_cases = new[] {
		new TestCaseData(new F((ref Box4 box) => box.LeftAnchor += 4), Box4.FromLeftTop(5, 2, 3, 4)).SetName(nameof(Box4.LeftAnchor)),
		new TestCaseData(new F((ref Box4 box) => box.TopAnchor -= 3), Box4.FromLeftTop(1, -1, 3, 4)).SetName(nameof(Box4.TopAnchor)),
		new TestCaseData(new F((ref Box4 box) => box.RightAnchor += 2), Box4.FromLeftTop(3, 2, 3, 4)).SetName(nameof(Box4.RightAnchor)),
		new TestCaseData(new F((ref Box4 box) => box.BottomAnchor -= 1), Box4.FromLeftTop(1, 1, 3, 4)).SetName(nameof(Box4.BottomAnchor)),
		new TestCaseData(new F((ref Box4 box) => box.CenterXAnchor += 5), Box4.FromLeftTop(6, 2, 3, 4)).SetName(nameof(Box4.CenterXAnchor)),
		new TestCaseData(new F((ref Box4 box) => box.CenterYAnchor -= 6), Box4.FromLeftTop(1, -4, 3, 4)).SetName(nameof(Box4.CenterYAnchor)),
		new TestCaseData(new F((ref Box4 box) => box.SetLerpXAnchor(0.5f, box.LerpXAnchor(0.25f))), Box4.FromLeftTop(0.25f, 2, 3, 4)).SetName(nameof(Box4.SetLerpXAnchor)),
		new TestCaseData(new F((ref Box4 box) => box.SetLerpYAnchor(0.25f, box.LerpYAnchor(0.75f))), Box4.FromLeftTop(1, 4, 3, 4)).SetName(nameof(Box4.SetLerpYAnchor)),
	};

	static readonly TestCaseData[] Side_coordinates_related_properties_test_cases = new[] {
		new TestCaseData(new F((ref Box4 box) => box.Left += 4), Box4.FromLeftTop(5, 2, -1, 4)).SetName(nameof(Box4.Left)),
		new TestCaseData(new F((ref Box4 box) => box.Top -= 3), Box4.FromLeftTop(1, -1, 3, 7)).SetName(nameof(Box4.Top)),
		new TestCaseData(new F((ref Box4 box) => box.Right += 2), Box4.FromLeftTop(1, 2, 5, 4)).SetName(nameof(Box4.Right)),
		new TestCaseData(new F((ref Box4 box) => box.Bottom -= 1), Box4.FromLeftTop(1, 2, 3, 3)).SetName(nameof(Box4.Bottom)),
	};

	static readonly TestCaseData[] Size_related_properties_test_cases = new[] {
		new TestCaseData(new F((ref Box4 box) => box.LeftAnchoredWidth += 4), Box4.FromLeftTop(1, 2, 7, 4)).SetName(nameof(Box4.LeftAnchoredWidth)),
		new TestCaseData(new F((ref Box4 box) => box.TopAnchoredHeight -= 3), Box4.FromLeftTop(1, 2, 3, 1)).SetName(nameof(Box4.TopAnchoredHeight)),
		new TestCaseData(new F((ref Box4 box) => box.RightAnchoredWidth += 2), Box4.FromLeftTop(-1, 2, 5, 4)).SetName(nameof(Box4.RightAnchoredWidth)),
		new TestCaseData(new F((ref Box4 box) => box.BottomAnchoredHeight -= 1), Box4.FromLeftTop(1, 3, 3, 3)).SetName(nameof(Box4.BottomAnchoredHeight)),
		new TestCaseData(new F((ref Box4 box) => box.CenterAnchoredWidth += 5), Box4.FromLeftTop(-1.5f, 2, 8, 4)).SetName(nameof(Box4.CenterAnchoredWidth)),
		new TestCaseData(new F((ref Box4 box) => box.CenterAnchoredHeight -= 6), Box4.FromLeftTop(1, 5, 3, -2)).SetName(nameof(Box4.CenterAnchoredHeight)),
		new TestCaseData(new F((ref Box4 box) => box.SetLerpAnchoredWidth(0.5f, box.Height)), Box4.FromLeftTop(0.5f, 2, 4, 4)).SetName(nameof(Box4.SetLerpAnchoredWidth)),
		new TestCaseData(new F((ref Box4 box) => box.SetLerpAnchoredHeight(0.25f, box.Width)), Box4.FromLeftTop(1, 2.25f, 3, 3)).SetName(nameof(Box4.SetLerpAnchoredHeight)),
	};

	static readonly TestCaseData[] In_place_members_test_cases = new[] {
		new TestCaseData(new F((ref Box4 box) => box.OffsetBy(12, -3)), Box4.FromLeftTop(13, -1, 3, 4)).SetName(nameof(Box4.OffsetBy)),
		// new TestCaseData(new F((ref Box4 box) => box.AngularOffsetBy(0.25f * MathF.Tau, 2)), Box4.FromLeftTop(1, 4, 3, 4)).SetName(nameof(Box4.AngularOffsetBy)),
		new TestCaseData(new F((ref Box4 box) => box.InflateBy(4, 5)), Box4.FromLeftTop(-4, -2, 13, 12)).SetName(nameof(Box4.InflateBy)),
		new TestCaseData(new F((ref Box4 box) => box.DeflateBy(1, 2, -3)), Box4.FromLeftTop(3, 3, -1, 6)).SetName(nameof(Box4.DeflateBy)),
	};

	[Test]
	[TestCaseSource(nameof(Immutability_via_with_expression_for_anchor_coordinates_related_properties))]
	[TestCaseSource(nameof(Immutability_via_with_expression_for_side_coordinates_related_properties))]
	[TestCaseSource(nameof(Immutability_via_with_expression_for_size_related_properties_test_cases))]
	[TestCaseSource(nameof(Immutability_via_immutable_members_test_cases))]
	public void Immutability_via_with_expression_and_immutable_members(G g, Box4 expected) {
		var original = Box4.FromLeftTop(1, 2, 3, 4);
		var actual = g(original);

		Assert.AreEqual(expected, actual);
		Assert.AreNotEqual(original, actual);
	}

	public delegate Box4 G(Box4 box);

	static readonly TestCaseData[] Immutability_via_with_expression_for_anchor_coordinates_related_properties = new[] {
		new TestCaseData(new G((Box4 box) => box with { LeftTopAnchor = new(4, -3) }), Box4.FromLeftTop(4, -3, 3, 4)).SetName("with " + nameof(Box4.LeftTopAnchor)),
		new TestCaseData(new G((Box4 box) => box with { RightBottomAnchor = new(2, -1) }), Box4.FromLeftTop(-1, -5, 3, 4)).SetName("with " + nameof(Box4.RightBottomAnchor)),
		new TestCaseData(new G((Box4 box) => box with { CenterAnchor = new(5, -6) }), Box4.FromLeftTop(3.5f, -8, 3, 4)).SetName("with " + nameof(Box4.CenterAnchor)),
		new TestCaseData(new G((Box4 box) => box with { LeftCenterAnchor = new(1, -2) }), Box4.FromLeftTop(1, -4, 3, 4)).SetName("with " + nameof(Box4.LeftCenterAnchor)),
		new TestCaseData(new G((Box4 box) => box with { RightCenterAnchor = new(-3, 4) }), Box4.FromLeftTop(-6, 2, 3, 4)).SetName("with " + nameof(Box4.RightCenterAnchor)),
		new TestCaseData(new G((Box4 box) => box with { CenterTopAnchor = new(5, -6) }), Box4.FromLeftTop(3.5f, -6, 3, 4)).SetName("with " + nameof(Box4.CenterTopAnchor)),
		new TestCaseData(new G((Box4 box) => box with { CenterBottomAnchor = new(-7, 8) }), Box4.FromLeftTop(-8.5f, 4, 3, 4)).SetName("with " + nameof(Box4.CenterBottomAnchor)),
		new TestCaseData(new G((Box4 box) => box.WithLerpAnchor(new(0.5f, 0.25f), box.LerpAnchor(new(0.25f, 0.75f)))), Box4.FromLeftTop(0.25f, 4, 3, 4)).SetName(nameof(Box4.WithLerpAnchor)),
	};

	static readonly TestCaseData[] Immutability_via_with_expression_for_side_coordinates_related_properties = new[] {
		new TestCaseData(new G((Box4 box) => box with { LeftBottom = new(4, -1) }), Box4.FromLeftTop(4, 2, 0, -3)).SetName("with " + nameof(Box4.LeftBottom)),
		new TestCaseData(new G((Box4 box) => box with { RightTop = new(2, -3) }), Box4.FromLeftTop(1, -3, 1, 9)).SetName("with " + nameof(Box4.RightTop)),
	};

	static readonly TestCaseData[] Immutability_via_with_expression_for_size_related_properties_test_cases = new[] {
		new TestCaseData(new G((Box4 box) => box with { LeftTopAnchoredSize = new(4, -3) }), Box4.FromLeftTop(1, 2, 4, -3)).SetName("with " + nameof(Box4.LeftTopAnchoredSize)),
		new TestCaseData(new G((Box4 box) => box with { RightBottomAnchoredSize = new(2, -1) }), Box4.FromLeftTop(2, 7, 2, -1)).SetName("with " + nameof(Box4.RightBottomAnchoredSize)),
		new TestCaseData(new G((Box4 box) => box with { CenterAnchoredSize = new(5, -6) }), Box4.FromLeftTop(0, 7, 5, -6)).SetName("with " + nameof(Box4.CenterAnchoredSize)),
		new TestCaseData(new G((Box4 box) => box with { LeftCenterAnchoredSize = new(1, -2) }), Box4.FromLeftTop(1, 5, 1, -2)).SetName("with " + nameof(Box4.LeftCenterAnchoredSize)),
		new TestCaseData(new G((Box4 box) => box with { RightCenterAnchoredSize = new(-3, 4) }), Box4.FromLeftTop(7, 2, -3, 4)).SetName("with " + nameof(Box4.RightCenterAnchoredSize)),
		new TestCaseData(new G((Box4 box) => box with { CenterTopAnchoredSize = new(5, -6) }), Box4.FromLeftTop(0, 2, 5, -6)).SetName("with " + nameof(Box4.CenterTopAnchoredSize)),
		new TestCaseData(new G((Box4 box) => box with { CenterBottomAnchoredSize = new(-7, 8) }), Box4.FromLeftTop(6, -2, -7, 8)).SetName("with " + nameof(Box4.CenterBottomAnchoredSize)),
		new TestCaseData(new G((Box4 box) => box.WithLerpAnchoredSize(new(0.5f, 0.25f), new(9, -1))), Box4.FromLeftTop(-2, 3.25f, 9, -1)).SetName(nameof(Box4.WithLerpAnchoredSize)),
	};

	static readonly TestCaseData[] Immutability_via_immutable_members_test_cases = new[] {
		new TestCaseData(new G((Box4 box) => box.WithOffset(12, -3)), Box4.FromLeftTop(13, -1, 3, 4)).SetName(nameof(Box4.WithOffset)),
		// new TestCaseData(new G((Box4 box) => box.WithAngularOffset(0.25f * MathF.Tau, 2)), Box4.FromLeftTop(1, 4, 3, 4)).SetName(nameof(Box4.WithAngularOffset)),
		new TestCaseData(new G((Box4 box) => box.Inflated(4, 5)), Box4.FromLeftTop(-4, -2, 13, 12)).SetName(nameof(Box4.Inflated)),
		new TestCaseData(new G((Box4 box) => box.Deflated(1, 2, -3)), Box4.FromLeftTop(3, 3, -1, 6)).SetName(nameof(Box4.Deflated)),
	};

	[Test] // Box           */ Min           */ Expected
	[TestCase(2, 3, 2, 1, /**/ 1, 1, 2, 1, /**/ 1, 1, 3, 3)]
	[TestCase(1, 1, 2, 1, /**/ 2, 3, 2, 1, /**/ 1, 1, 3, 3)]
	[TestCase(1, 3, 1, 1, /**/ 2, 1, 2, 1, /**/ 1, 1, 3, 3)]
	[TestCase(2, 1, 2, 1, /**/ 1, 3, 1, 1, /**/ 1, 1, 3, 3)]
	[TestCase(1, 1, 3, 3, /**/ 2, 2, 1, 1, /**/ 1, 1, 3, 3)]
	[TestCase(2, 2, 1, 1, /**/ 1, 1, 3, 3, /**/ 1, 1, 3, 3)]
	[TestCase(1, 2, 3, 1, /**/ 2, 1, 1, 3, /**/ 1, 1, 3, 3)]
	[TestCase(2, 1, 1, 3, /**/ 1, 2, 3, 1, /**/ 1, 1, 3, 3)]
	public static void CoerceAtLeast(
		float bl, float bt, float bw, float bh,
		float ml, float mt, float mw, float mh,
		float xl, float xt, float xw, float xh
	) {
		var box = Box4.FromLeftTop(bl, bt, bw, bh);
		var min = Box4.FromLeftTop(ml, mt, mw, mh);
		var expected = Box4.FromLeftTop(xl, xt, xw, xh);

		box.CoerceAtLeast(min);

		Assert.AreEqual(expected, box);
	}

	[Test] // Box           */ Max           */ Expected
	[TestCase(2, 3, 2, 1, /**/ 1, 1, 2, 1, /**/ 2, 2, 1, 0)]
	[TestCase(1, 1, 2, 1, /**/ 2, 3, 2, 1, /**/ 2, 3, 1, 0)]
	[TestCase(1, 3, 1, 1, /**/ 2, 1, 2, 1, /**/ 2, 2, 0, 0)]
	[TestCase(2, 1, 2, 1, /**/ 1, 3, 1, 1, /**/ 2, 3, 0, 0)]
	[TestCase(1, 1, 3, 3, /**/ 2, 2, 1, 1, /**/ 2, 2, 1, 1)]
	[TestCase(2, 2, 1, 1, /**/ 1, 1, 3, 3, /**/ 2, 2, 1, 1)]
	[TestCase(1, 2, 3, 1, /**/ 2, 1, 1, 3, /**/ 2, 2, 1, 1)]
	[TestCase(2, 1, 1, 3, /**/ 1, 2, 3, 1, /**/ 2, 2, 1, 1)]
	public static void CoerceAtMost(
		float bl, float bt, float bw, float bh,
		float ml, float mt, float mw, float mh,
		float xl, float xt, float xw, float xh
	) {
		var box = Box4.FromLeftTop(bl, bt, bw, bh);
		var max = Box4.FromLeftTop(ml, mt, mw, mh);
		var expected = Box4.FromLeftTop(xl, xt, xw, xh);

		box.CoerceAtMost(max);

		Assert.AreEqual(expected, box);
	}

	[Test]
	public static void Normalize() {
		var a = Box4.FromLeftTop(3, 1, -2, -7);
		var b = Box4.FromLeftTop(3, 1, 2, -7);
		var c = Box4.FromLeftTop(3, 1, -2, 7);
		var d = Box4.FromLeftTop(3, 1, 2, 7);

		a.Normalize();
		b.Normalize();
		c.Normalize();
		d.Normalize();

		Assert.AreEqual(Box4.FromLeftTop(1, -6, 2, 7), a);
		Assert.AreEqual(Box4.FromLeftTop(3, -6, 2, 7), b);
		Assert.AreEqual(Box4.FromLeftTop(1, 1, 2, 7), c);
		Assert.AreEqual(Box4.FromLeftTop(3, 1, 2, 7), d);
	}
}
