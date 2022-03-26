using NUnit.Framework;
using System;
using Vortice.Mathematics;

namespace Uy.Tests;

public class Color4ExtensionsTests {
	#region Conversion extensions

	[Test]
	public void ToRgba() =>
		Assert.AreEqual(new Color4(1, 1, 0, 0.2f), 0xFFFF0033.ToRgba());

	[Test]
	public void ToRgb() =>
		Assert.AreEqual(new Color4(1, 1, 0), 0xFFFF00.ToRgb());

	[Test]
	public void Deconstruct_3() {
		var (r, g, b) = new Color4(0.1f, 0.2f, 0.3f, 0.4f);

		Assert.AreEqual(0.1f, r);
		Assert.AreEqual(0.2f, g);
		Assert.AreEqual(0.3f, b);
	}

	[Test]
	public void Deconstruct_4() {
		var (r, g, b, a) = new Color4(0.1f, 0.2f, 0.3f, 0.4f);

		Assert.AreEqual(0.1f, r);
		Assert.AreEqual(0.2f, g);
		Assert.AreEqual(0.3f, b);
		Assert.AreEqual(0.4f, a);
	}

	#endregion
	#region Color-space extensions

	[Test]
	[TestCase(0f, 0f, 0f, 0f, 0f, 0f)]
	[TestCase(1f, 1f, 1f, 0f, 0f, 1f)]
	[TestCase(1f, 0f, 0f, 0f, 1f, 0.5f)]
	[TestCase(1f, 0.5f, 0f, 0.08f, 1f, 0.5f)]
	[TestCase(0.32f, 0.88f, 0.47f, 0.38f, 0.71f, 0.6f)]
	[TestCase(0.18f, 0.18f, 0.18f, 0f, 0f, 0.18f)]
	[TestCase(0.68f, 0.67f, 0.74f, 0.69f, 0.12f, 0.71f)]
	public void Hsl(float r, float g, float b, float h, float s, float l) {
		var color = new Color4(r, g, b);
		var hsl = color.Hsl();

		Assert.AreEqual(h, hsl.X, 0.01);
		Assert.AreEqual(h, color.H(), 0.01);
		Assert.AreEqual(s, hsl.Y, 0.01);
		Assert.AreEqual(s, color.S(), 0.01);
		Assert.AreEqual(l, hsl.Z, 0.01);
		Assert.AreEqual(l, color.L(), 0.01);
	}

	// There is no in-place member: Color4 is a readonly struct.

	[Test]
	[TestCaseSource(nameof(Immutability_via_HSL_color_space_members_test_cases))]
	public void Immutability_via_with_expression_and_immutable_members(G g, Color4 expected) {
		var original = new Color4(0.1f, 0.2f, 0.3f);
		var actual = g(original);

		AssertColor(expected, actual);
		Assert.AreNotEqual(original, actual);
	}

	public delegate Color4 G(Color4 color);

	static readonly TestCaseData[] Immutability_via_HSL_color_space_members_test_cases = new[] {
		new TestCaseData(new G((Color4 color) => color.WithH(0.4f)), new Color4(0.10f, 0.30f, 0.18f)).SetArgDisplayNames(nameof(Color4Extensions.WithH)),
		new TestCaseData(new G((Color4 color) => color.WithS(0.7f)), new Color4(0.06f, 0.20f, 0.35f)).SetArgDisplayNames(nameof(Color4Extensions.WithS)),
		new TestCaseData(new G((Color4 color) => color.WithL(0.6f)), new Color4(0.40f, 0.60f, 0.80f)).SetArgDisplayNames(nameof(Color4Extensions.WithL)),
	};

	[Test]
	[TestCase(0f, 0f, 0f, 0.1f, 0f, 0f, 0f)]
	[TestCase(0f, 0f, 0f, 0.5f, 0f, 0f, 0f)]
	[TestCase(0f, 0f, 0f, 0.9f, 0f, 0f, 0f)]
	[TestCase(0.42f, 0.42f, 0.42f, 0.1f, 0.42f, 0.42f, 0.42f)]
	[TestCase(0.42f, 0.42f, 0.42f, 0.5f, 0.42f, 0.42f, 0.42f)]
	[TestCase(0.42f, 0.42f, 0.42f, 0.9f, 0.42f, 0.42f, 0.42f)]
	[TestCase(1f, 1f, 1f, 0.1f, 1f, 1f, 1f)]
	[TestCase(1f, 1f, 1f, 0.5f, 1f, 1f, 1f)]
	[TestCase(1f, 1f, 1f, 0.9f, 1f, 1f, 1f)]
	public void Setting_hue_on_greys_has_no_effect(float r, float g, float b, float h, float xr, float xg, float xb) {
		var actual = new Color4(r, g, b).WithH(h);

		AssertColor(new(xr, xg, xb), actual);
	}

	[Test]
	[TestCase(0f, 0f, 0f, 0.1f, 0f, 0f, 0f)]
	[TestCase(0f, 0f, 0f, 0.5f, 0f, 0f, 0f)]
	[TestCase(0f, 0f, 0f, 0.9f, 0f, 0f, 0f)]
	[TestCase(1f, 1f, 1f, 0.1f, 1f, 1f, 1f)]
	[TestCase(1f, 1f, 1f, 0.5f, 1f, 1f, 1f)]
	[TestCase(1f, 1f, 1f, 0.9f, 1f, 1f, 1f)]
	public void Setting_saturation_on_black_or_white_has_no_effect(float r, float g, float b, float s, float xr, float xg, float xb) {
		var actual = new Color4(r, g, b).WithS(s);

		AssertColor(new(xr, xg, xb), actual);
	}

	#endregion

	static void AssertColor(Color4 expected, Color4 actual) {
		Assert.AreEqual(expected.R, actual.R, 0.01, "Expected {0}, but was {1}", expected, actual);
		Assert.AreEqual(expected.G, actual.G, 0.01, "Expected {0}, but was {1}", expected, actual);
		Assert.AreEqual(expected.B, actual.B, 0.01, "Expected {0}, but was {1}", expected, actual);
	}
}
