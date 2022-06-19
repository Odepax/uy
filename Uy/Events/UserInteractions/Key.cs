namespace Uy;

/**
<summary>
	<para>
		Mouse button and keyboard code names, as per most of
		<see href="https://www.w3.org/TR/uievents-code">W3C's UI Events KeyboardEvent code Values</see>.
	</para>
</summary>
<seealso href="https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes"/>
<seealso cref="KeyEvent"/>
**/
public enum Key : byte {
	None = 0,

	#region Mouse buttons

	MouseLeft = 0x01,
	MouseRight = 0x02,
	MouseMiddle = 0x04,
	MouseExtra1 = 0x05,
	MouseExtra2 = 0x06,

	#endregion
	#region Escape and function keys

	Escape = 0x1B,

	F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73,
	F5 = 0x74, F6 = 0x75, F7 = 0x76, F8 = 0x77,
	F9 = 0x78, F10 = 0x79, F11 = 0x7A, F12 = 0x7B,
	F13 = 0x7C, F14 = 0x7D, F15 = 0x7E, F16 = 0x7F,
	F17 = 0x80, F18 = 0x81, F19 = 0x82, F20 = 0x83,
	F21 = 0x84, F22 = 0x85, F23 = 0x86, F24 = 0x87,

	#endregion
	#region Digits row

	/**
	<summary>
		<para>
			QWERTY's <c>`</c>, or AZERTY's <c>²</c>,
			located between <see cref="Escape"/> and <see cref="Tab"/>.
		</para>
	</summary>
	**/
	Backquote = 0xC0,

	Digit1 = 0x31,
	Digit2 = 0x32,
	Digit3 = 0x33,
	Digit4 = 0x34,
	Digit5 = 0x35,
	Digit6 = 0x36,
	Digit7 = 0x37,
	Digit8 = 0x38,
	Digit9 = 0x39,
	Digit0 = 0x30,
	Minus = 0xBD,
	Equal = 0xBB,

	///**
	//<summary>
	//	<para>
	//		Extra key on Japanese and Russian keyboards,
	//		located between <see cref="Equal"/> and <see cref="Backspace"/>.
	//	</para>
	//</summary>
	//**/
	//IntlYen, // Cannot try out this one with my keyboard...

	#endregion
	#region Main letter matrix (and some extra)

	/**
	<summary>
		<para>
			QWERTY's <c>Q</c>, or AZERTY's <c>A</c>.
		</para>
	</summary>
	**/
	KeyQ = 0x51,

	/**
	<summary>
		<para>
			QWERTY's <c>W</c>, or AZERTY's <c>Z</c>.
		</para>
	</summary>
	**/
	KeyW = 0x57,

	KeyE = 0x45,
	KeyR = 0x52,
	KeyT = 0x54,
	KeyY = 0x59,
	KeyU = 0x55,
	KeyI = 0x49,
	KeyO = 0x4F,
	KeyP = 0x50,

	/**
	<summary>
		<para>
			QWERTY's <c>[</c>, or AZERTY's <c>$</c>,
			located after <see cref="KeyP"/>.
		</para>
	</summary>
	**/
	BracketLeft = 0xDB,

	/**
	<summary>
		<para>
			QWERTY's <c>]</c>, or AZERTY's <c>^</c>,
			located second after <see cref="KeyP"/>.
		</para>
	</summary>
	**/
	BracketRight = 0xDD,

	/**
	<summary>
		<para>
			QWERTY's <c>A</c>, or AZERTY's <c>Q</c>.
		</para>
	</summary>
	**/
	KeyA = 0x41,

	KeyS = 0x53,
	KeyD = 0x44,
	KeyF = 0x46,
	KeyG = 0x47,
	KeyH = 0x48,
	KeyJ = 0x4A,
	KeyK = 0x4B,
	KeyL = 0x4C,

	/**
	<summary>
		<para>
			QWERTY's <c>;</c>, or AZERTY's <c>M</c>.
		</para>
	</summary>
	**/
	Semicolon = 0xBA,

	/**
	<summary>
		<para>
			QWERTY's <c>'</c>, or AZERTY's <c>ù</c>.
		</para>
	</summary>
	**/
	Quote = 0xDE,

	/**
	<summary>
		<para>
			QWERTY's <c>\</c>, or AZERTY's <c>*</c>.
		</para>
	</summary>
	**/
	Backslash = 0xDC,

	/**
	<summary>
		<para>
			Extra key on UK keyboards, or AZERTY's chevrons,
			located before <see cref="KeyZ"/>.
		</para>
	</summary>
	**/
	IntlBackslash = 0xE2,

	/**
	<summary>
		<para>
			QWERTY's <c>Z</c>, or AZERTY's <c>W</c>.
		</para>
	</summary>
	**/
	KeyZ = 0x5A,

	KeyX = 0x58,
	KeyC = 0x43,
	KeyV = 0x56,
	KeyB = 0x42,
	KeyN = 0x4E,

	/**
	<summary>
		<para>
			QWERTY's <c>M</c>, or AZERTY's <c>,</c>.
		</para>
	</summary>
	**/
	KeyM = 0x4D,

	/**
	<summary>
		<para>
			QWERTY's <c>,</c>, or AZERTY's <c>;</c>.
		</para>
	</summary>
	**/
	Comma = 0xBC,

	/**
	<summary>
		<para>
			QWERTY's <c>.</c>, or AZERTY's <c>:</c>.
		</para>
	</summary>
	**/
	Period = 0xBE,

	/**
	<summary>
		<para>
			QWERTY's <c>/</c>, or AZERTY's <c>!</c>.
		</para>
	</summary>
	**/
	Slash = 0xBF,

	/**
	<summary>
		<para>
			AZERTY's <c>!</c> for <see cref="KeyEvent.LayoutKey"/>
		</para>
	</summary>
	**/
	Bang = 0xDF,

	///**
	//<summary>
	//	<para>
	//		Extra key on Japanese keyboards,
	//		located between <see cref="Slash"/> and <see cref="ShiftRight"/>.
	//	</para>
	//</summary>
	//**/
	//IntlRo, // Cannot try out this one with my keyboard...

	#endregion
	#region Control keys

	Tab = 0x09,
	ShiftLeft = 0xA0,
	ControlLeft = 0xA2,
	MetaLeft = 0x5B,
	AltLeft = 0xA4,
	Space = 0x20,

	/**
	<summary>
		<para>
			Original <see cref="AltGr"/>.
		</para>
	</summary>
	<remarks>
		<para>
			Some keyboards will send a <see cref="ControlLeft"/> before <see cref="AltRight"/>.
			In general, it's best <b>not</b> to use <see cref="AltRight"/> with keyboard shortcuts,
			as it also plays an essential role in typing characters for <see cref="TextInputEvent"/>s.
		</para>
	</remarks>
	**/
	AltRight = 0xA5,

	/**
	<summary>
		<para>
			Alias for <see cref="AltRight"/>.
		</para>
	</summary>
	**/
	AltGr = AltRight,

	MetaRight = 0x5C,
	ContextMenu = 0x5D,
	ControlRight = 0xA3,
	ShiftRight = 0xA1,
	Enter = 0x0D,
	Backspace = 0x08,
	Insert = 0x2D,
	Delete = 0x2E,
	Home = 0x24,
	End = 0x23,
	PageUp = 0x21,
	PageDown = 0x22,
	Pause = 0x13,

	#endregion
	#region Arrows

	ArrowUp = 0x26,
	ArrowRight = 0x27,
	ArrowDown = 0x28,
	ArrowLeft = 0x25,

	#endregion
	#region Numeric pad

	Numpad0 = 0x60,
	Numpad1 = 0x61,
	Numpad2 = 0x62,
	Numpad3 = 0x63,
	Numpad4 = 0x64,
	Numpad5 = 0x65,
	Numpad6 = 0x66,
	Numpad7 = 0x67,
	Numpad8 = 0x68,
	Numpad9 = 0x69,
	NumpadDecimal = 0x6E,
	NumpadEnter = 0x0E,

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadComma, // Cannot try out this one with my keyboard... Could it be VK_SEPARATOR = 0x6C ?

	NumpadAdd = 0x6B,
	NumpadSubtract = 0x6D,
	NumpadMultiply = 0x6A,
	NumpadDivide = 0x6F,

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadEqual, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadClear, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadParenLeft, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadParenRight, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadBackspace, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadStar, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadHash, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadMemoryStore, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadMemoryAdd, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadMemorySubtract, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadMemoryRecall, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//NumpadMemoryClear, // Cannot try out this one with my keyboard...

	#endregion
	#region Browser specific

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//BrowserHome = 0xAC, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//BrowserFavorites = 0xAB, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//BrowserBackward = 0xA6, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//BrowserForward = 0xA7, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//BrowserRefresh = 0xA8, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//BrowserSearch = 0xAA, // Cannot try out this one with my keyboard...

	///**
	//<summary>
	//	<para>
	//		Not found on all keyboards.
	//	</para>
	//</summary>
	//**/
	//BrowserStop = 0xA9, // Cannot try out this one with my keyboard...

	#endregion
}
