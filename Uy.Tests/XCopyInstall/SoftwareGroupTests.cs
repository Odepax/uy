using LinqToYourDoom;
using NUnit.Framework;
using System;
using System.IO;
using static Uy.StateScope;

namespace Uy.Tests;

public class SoftwareGroupTests {
	[Test]
	[TestCaseSource(nameof(GetStateDirectory_test_cases))]
	public void GetStateDirectory(StateScope scope, string expected) {
		var actual = new SoftwareGroup("El Softo", "0.0.666")
			.GetStateDirectory(scope)
			.FullName;

		StringAssert.EndsWith(SanitizePath(expected), SanitizePath(actual));
	}

	static readonly TestCaseData[] GetStateDirectory_test_cases = new[] {
		new TestCaseData(Data | Light | AllUsers /*       */ | Unversioned /* */, "LD/GEl Softo/A/A"),
		new TestCaseData(Data | Light | CurrentUser /*    */ | Unversioned /* */, "LD/GEl Softo/A/U" + Environment.UserName),
		new TestCaseData(Data | Light | AllUsers /*       */ | Versioned /*   */, "LD/GEl Softo/V0.0.666/A"),
		new TestCaseData(Data | Light | CurrentUser /*    */ | Versioned /*   */, "LD/GEl Softo/V0.0.666/U" + Environment.UserName),
		new TestCaseData(Data | Heavy | AllUsers /*       */ | Unversioned /* */, "HD/GEl Softo/A/A"),
		new TestCaseData(Data | Heavy | CurrentUser /*    */ | Unversioned /* */, "HD/GEl Softo/A/U" + Environment.UserName),
		new TestCaseData(Data | Heavy | AllUsers /*       */ | Versioned /*   */, "HD/GEl Softo/V0.0.666/A"),
		new TestCaseData(Data | Heavy | CurrentUser /*    */ | Versioned /*   */, "HD/GEl Softo/V0.0.666/U" + Environment.UserName),
		new TestCaseData(Temp | Light | AllUsers /*       */ | Unversioned /* */, "LT/GEl Softo/A/A"),
		new TestCaseData(Temp | Light | CurrentUser /*    */ | Unversioned /* */, "LT/GEl Softo/A/U" + Environment.UserName),
		new TestCaseData(Temp | Light | AllUsers /*       */ | Versioned /*   */, "LT/GEl Softo/V0.0.666/A"),
		new TestCaseData(Temp | Light | CurrentUser /*    */ | Versioned /*   */, "LT/GEl Softo/V0.0.666/U" + Environment.UserName),
		new TestCaseData(Temp | Heavy | AllUsers /*       */ | Unversioned /* */, "HT/GEl Softo/A/A"),
		new TestCaseData(Temp | Heavy | CurrentUser /*    */ | Unversioned /* */, "HT/GEl Softo/A/U" + Environment.UserName),
		new TestCaseData(Temp | Heavy | AllUsers /*       */ | Versioned /*   */, "HT/GEl Softo/V0.0.666/A"),
		new TestCaseData(Temp | Heavy | CurrentUser /*    */ | Versioned /*   */, "HT/GEl Softo/V0.0.666/U" + Environment.UserName),
	};

	[Test]
	[TestCase(null, TestName = "GetStateDirectory(PID)")]
	public void GetStateDirectory_process_id(object? _) {
		GetStateDirectory(Temp | Light | CurrentProcess, "LT/GEl Softo/P/P" + Environment.ProcessId);
		GetStateDirectory(Temp | Heavy | CurrentProcess, "HT/GEl Softo/P/P" + Environment.ProcessId);
	}

	[Test]
	[TestCaseSource(nameof(GetStateDirectory_invalid_scope_test_cases))]
	public void GetStateDirectory_invalid_scope(StateScope scope) =>
		Assert.Throws<ArgumentException>(() => new SoftwareGroup("El Softo", "0.0.666").GetStateDirectory(scope));

	static readonly TestCaseData[] GetStateDirectory_invalid_scope_test_cases = new[] {
		// CurrentProcess excludes Data.
		new TestCaseData(Data | Light | CurrentProcess),
		new TestCaseData(Data | Heavy | CurrentProcess),

		// CurrentProcess excludes Unversioned or Versioned.
		new TestCaseData(Temp | Light | CurrentProcess | Unversioned),
		new TestCaseData(Temp | Heavy | CurrentProcess | Versioned),

		// Not all masks are set.
		new TestCaseData(Heavy | Data),
		new TestCaseData(Data | CurrentProcess),
		new TestCaseData(Temp | Unversioned),
	};

	static string SanitizePath(string path) =>
		path
			.ToBuilder()
			.Replace(Environment.UserName, "John") // For the sake of testing.
			.Replace(Path.DirectorySeparatorChar, '/')
			.ToString();
}
