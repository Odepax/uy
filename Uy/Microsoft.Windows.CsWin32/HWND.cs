namespace Windows.Win32.Foundation;

readonly partial struct HWND {
	public override readonly string ToString() => $"{ nameof(HWND) } { Value }";
}
