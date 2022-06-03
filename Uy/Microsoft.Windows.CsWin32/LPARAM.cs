namespace Windows.Win32.Foundation;

readonly partial struct LPARAM {
	public static implicit operator int(LPARAM value) => (int) value.Value;
}
