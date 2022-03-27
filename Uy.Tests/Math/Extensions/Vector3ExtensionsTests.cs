using NUnit.Framework;
using System.Numerics;

namespace Uy.Tests;

class Vector3ExtensionsTests {
	[Test]
	public void Deconstruct() {
		var (x, y, z) = new Vector3(1, 2, 3);

		Assert.AreEqual(1, x);
		Assert.AreEqual(2, y);
		Assert.AreEqual(3, z);
	}
}
