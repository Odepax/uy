using NUnit.Framework;
using System.Numerics;

namespace Uy.Tests;

class Vector4ExtensionsTests {
	[Test]
	public void Deconstruct() {
		var (x, y, z, w) = new Vector4(1, 2, 3, 4);

		Assert.AreEqual(1, x);
		Assert.AreEqual(2, y);
		Assert.AreEqual(3, z);
		Assert.AreEqual(4, w);
	}
}
