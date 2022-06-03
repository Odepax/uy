namespace Windows.Win32.Foundation;

readonly partial struct WPARAM {
	public static implicit operator uint(WPARAM value) => (uint) value.Value;
	public static implicit operator bool(WPARAM value) => value.Value != 0;
}
