using NUnit.Framework;
using System.Numerics;

namespace Uy.Tests;

class Vector2ExtensionsTests {
	[Test]
	public void Deconstruct() {
		var (x, y) = new Vector2(1, 2);

		Assert.AreEqual(1, x);
		Assert.AreEqual(2, y);
	}

	[Test]
	[TestCaseSource(nameof(Lerp_and_LerpTo_test_cases))]
	public void Lerp_and_LerpTo(Vector2 start, Vector2 target, float ratio, Vector2 expected, Vector2 expectedTo) {
		var actual = start.Lerp(target, ratio);
		var actualTo = start.LerpTo(target, ratio);

		AssertVector(expected, actual);
		AssertVector(expectedTo, actualTo);
	}

	static readonly TestCaseData[] Lerp_and_LerpTo_test_cases = new[] {
		new TestCaseData(new Vector2(0f, 0f), new Vector2(0f, 0f), 0.5f, new Vector2(0f, 0f), new Vector2(0f, 0f)),
		new TestCaseData(new Vector2(1f, 1f), new Vector2(1f, 1f), 0.5f, new Vector2(1f, 1f), new Vector2(0f, 0f)),
		new TestCaseData(new Vector2(1f, 2f), new Vector2(3f, 4f), 0f, new Vector2(1f, 2f), new Vector2(0f, 0f)),
		new TestCaseData(new Vector2(1f, 2f), new Vector2(3f, 4f), 1f, new Vector2(3f, 4f), new Vector2(2f, 2f)),

		new TestCaseData(new Vector2(1f, 2f), new Vector2(4f, 2f), 0.333_333f, new Vector2(2f, 2f), new Vector2(1f, 0f)),
		new TestCaseData(new Vector2(1f, 3f), new Vector2(9f, -1f), 0.75f, new Vector2(7f, 0f), new Vector2(6f, -3f)),
		new TestCaseData(new Vector2(-3f, 0f), new Vector2(2f, 0f), 0.4f, new Vector2(-1f, 0f), new Vector2(2f, 0f)),
		new TestCaseData(new Vector2(-3f, -4f), new Vector2(-3f, 2f), 0.666_666f, new Vector2(-3f, 0f), new Vector2(0f, 4f)),
	};

	static void AssertVector(Vector2 expected, Vector2 actual) {
		Assert.AreEqual(expected.X, actual.X, 0.01, "Expected {0}, but was {1}", expected, actual);
		Assert.AreEqual(expected.Y, actual.Y, 0.01, "Expected {0}, but was {1}", expected, actual);
	}
}
