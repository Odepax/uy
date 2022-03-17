using System.Linq;
using NUnit.Framework;

namespace Uy.Tests;

// CCF5F0F0-3BED-4746-9D80-F5495579332E
//
// Directories in the source code of the main assembly are not supposed to provide sub-namespaces.
//
// ReSharper has an option to prevent marked directories from participating to namespacing,
// i.e. https://www.jetbrains.com/help/resharper/Refactorings__Adjust_Namespaces.html,
// but VS2022 doesn't.
//
// I wanted to flatten the assembly into a single namespace,
// while preserving a nice organization in the solution explorer,
// so I'm using this reflection-based test to assert that a sub-namespace hasn't sneaked in
// at the occasion of a coding session.
class SingleNamespaceTests {
	[Test]
	public void SingleNamespace() {
		var uyNamespaces = typeof(Uy.Class1)
			.Assembly
			.GetTypes()
			.Select(type => type.Namespace)
			.Where(@namespace => @namespace != null && @namespace.StartsWith(nameof(Uy)))
			.Distinct()
			.ToArray();

		Assert.AreEqual(1, uyNamespaces.Length);
		Assert.AreEqual(nameof(Uy), uyNamespaces[0]);
	}
}
