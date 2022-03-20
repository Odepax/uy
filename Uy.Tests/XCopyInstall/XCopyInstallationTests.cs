using LinqToYourDoom;
using NUnit.Framework;
using System;
using System.IO;
using static Uy.InstallationMode;
using static Uy.StateScope;

namespace Uy.Tests;

public class XCopyInstallationTests {
	[Test]
	public void CurrentInstallationMode() {
		Assert.AreEqual(InstallationMode.Portable, XCopyInstallation.CurrentInstallationMode);
	}

	[Test]
	[TestCaseSource(nameof(GetDefaultStateBaseDirectoryPath_test_cases))]
	public void GetDefaultStateBaseDirectoryPath(InstallationMode installationMode, StateScope scope, string expected) {
		var actual = XCopyInstallation.GetDefaultStateBaseDirectoryPath(installationMode, scope);

		Assert.AreEqual(SanitizePath(expected), SanitizePath(actual));
	}

	static readonly TestCaseData[] GetDefaultStateBaseDirectoryPath_test_cases = new[] {
		new TestCaseData(Portable, /* */ Data | Light | AllUsers /*       */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Light | AllUsers /*       */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Light | CurrentUser /*    */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Light | CurrentUser /*    */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Light | CurrentProcess /* */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Light | CurrentProcess /* */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Heavy | AllUsers /*       */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Heavy | AllUsers /*       */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Heavy | CurrentUser /*    */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Heavy | CurrentUser /*    */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Heavy | CurrentProcess /* */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Data | Heavy | CurrentProcess /* */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Light | AllUsers /*       */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Light | AllUsers /*       */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Light | CurrentUser /*    */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Light | CurrentUser /*    */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Light | CurrentProcess /* */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Light | CurrentProcess /* */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Heavy | AllUsers /*       */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Heavy | AllUsers /*       */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Heavy | CurrentUser /*    */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Heavy | CurrentUser /*    */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Heavy | CurrentProcess /* */ | Unversioned /* */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Portable, /* */ Temp | Heavy | CurrentProcess /* */ | Versioned /*   */, Path.Join(AppContext.BaseDirectory, XCopyInstallation.PortableStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Light | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/ProgramData", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Light | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/ProgramData", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Light | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Light | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Light | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Light | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Heavy | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/ProgramData", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Heavy | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/ProgramData", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Heavy | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Heavy | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Heavy | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Data | Heavy | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Light | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Light | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Light | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Light | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Light | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Light | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Heavy | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Heavy | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Heavy | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Heavy | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Heavy | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(Machine, /*  */ Temp | Heavy | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Light | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Light | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Light | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Light | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Light | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Light | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Heavy | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Heavy | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Heavy | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Heavy | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Heavy | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Data | Heavy | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Roaming", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Light | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Light | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Light | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Light | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Light | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Light | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Heavy | AllUsers /*       */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Heavy | AllUsers /*       */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Heavy | CurrentUser /*    */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Heavy | CurrentUser /*    */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Heavy | CurrentProcess /* */ | Unversioned /* */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
		new TestCaseData(User, /*     */ Temp | Heavy | CurrentProcess /* */ | Versioned /*   */, Path.Join("C:/Users/John/AppData/Local/Temp", XCopyInstallation.InstalledStateDirectoryName)),
	};

	static string SanitizePath(string path) =>
		path
			.ToBuilder()
			.Replace(Environment.UserName, "John") // For the sake of testing.
			.Replace(Path.DirectorySeparatorChar, '/')
			.ToString();
}
