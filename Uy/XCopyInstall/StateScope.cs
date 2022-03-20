using System;

namespace Uy;

/**
<inheritdoc cref="XCopyInstallation"/>
**/
[Flags]
public enum StateScope {
	Data /*           */ = 0b_10_00_000_00,
	Temp /*           */ = 0b_01_00_000_00,

	Light /*          */ = 0b_00_10_000_00,
	Heavy /*          */ = 0b_00_01_000_00,

	AllUsers /*       */ = 0b_00_00_100_00,
	CurrentUser /*    */ = 0b_00_00_010_00,
	CurrentProcess /* */ = 0b_00_00_001_00,

	Unversioned /*    */ = 0b_00_00_000_10,
	Versioned /*      */ = 0b_00_00_000_01,
}
