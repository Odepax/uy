namespace Uy;

/**
<summary>
	<para>
		The base class for keyboard-related events.
	</para>
</summary>
**/
public abstract class KeyEvent : UserInteractionEvent {
	/**
	<summary>
		<para>
			The phyical key related to this event.
		</para>
		<para>
			Could be thought of as an equivalent to <see href="https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/code">JavaScript's <c>KeyboardEvent.code</c></see>,
			in that the keyboard layout is ignored, e.g. <see cref="Key.KeyQ"/> will be returned
			if <c>Q</c> is used on a QWERTY keyboard
			as well as if <c>A</c> is used on an AZERTY keyboard.
		</para>
	</summary>
	<remarks>
		<para>
			Got to implement a WASD-based movement controller,
			and you don't want it to be messed depending on the user's keyboard language?
			You're in the right place!
		</para>
	</remarks>
	**/
	public readonly Key HardwareKey;

	/**
	<summary>
		<para>
			The virtual key, i.e. with keyboard layout applied, related to this event.
		</para>
		<para>
			E.g. <see cref="Key.KeyQ"/> will be returned when <c>Q</c> is used on a QWERTY keyboard,
			whereas <see cref="Key.KeyA"/> will be returned when <c>A</c> is used on an AZERTY keyboard,
			despite the two keys having the same physical slot in both layouts.
		</para>
	</summary>
	<remarks>
		<para>
			Got to implement <c>[Ctrl][A]</c>, <c>[Ctrl][Z]</c>, <c>[Ctrl][Q]</c> &amp; Cie shortcuts,
			and you want those to always be the same keys, no matter where they are on the user's keyboard?
			Say no more!
		</para>
	</remarks>
	**/
	public readonly Key LayoutKey;

	protected KeyEvent(Key hardwareKey, Key layoutKey) {
		HardwareKey = hardwareKey;
		LayoutKey = layoutKey;
	}
}
