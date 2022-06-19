namespace Uy;

/**
<summary>
	<para>
		The base class for all user interaction events.
	</para>
</summary>
**/
public abstract class UserInteractionEvent {
	/**
	<summary>
		<para>
			Indicates whether or not this event should be processed anymore,
			down or up the control hierarchy.
		</para>
	</summary>
	**/
	public bool ContinueProcessing { get; private set; } = true;

	protected UserInteractionEvent() {}

	/**
	<summary>
		<para>
			Sets <see cref="ContinueProcessing"/> for <see langword="false"/>,
			effectively marking this event so that it isn't processed anymore
			by controls down or up the control hierarchy.
		</para>
	</summary>
	**/
	public void StopProcessing() => ContinueProcessing = false;
}
