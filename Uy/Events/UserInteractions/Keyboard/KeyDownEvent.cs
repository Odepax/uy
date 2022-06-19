namespace Uy;

public class KeyDownEvent : KeyEvent {
	/**
	<summary>
		<para>
			A 0-based counter indicating the number of times the <see cref="KeyDownEvent"/>
			is beeing repeated due to the user holding down the pressed key over time.
		</para>
	</summary>
	<remarks>
		<para>
			When the code processing the <see cref="KeyDownEvent"/> is too slow,
			several events can be buffered and merged,
			in which case the next repeated event's <see cref="RepeatCount"/> value
			will be incremented by more than <c>1</c>.
		</para>
	</remarks>
	**/
	public readonly int RepeatCount;

	public bool IsRepeated => 0 < RepeatCount;

	public KeyDownEvent(Key hardwareKey, Key layoutKey, int repeatCount) : base(hardwareKey, layoutKey) {
		RepeatCount = repeatCount;
	}
}
