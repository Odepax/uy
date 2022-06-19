using Microsoft.Extensions.DependencyInjection;
using Vortice.Direct2D1;

namespace Uy;

/**
<summary>
	<para>
		Represents the content of a window.
		Responsible for handling events that must stay <i>synchronized</i> with the game loop.
	</para>
	<para>
		Implementations of this interface are expected to be registered
		as either <see cref="ServiceLifetime.Scoped"/>
		or <see cref="ServiceLifetime.Transient"/>
		in the dependency injection container;
		they will be pulled whenever new windows are created.
	</para>
</summary>
**/
public interface IWindowRootContent {
	#region Keyboard events

	/**
	<summary>
		<para>
			Called when a keyboard key is pressed down by the user while the host window is focused.
		</para>
	</summary>
	<remarks>
		<para>
			The <see cref="KeyDownEvent"/> <b>IS <see cref="KeyDownEvent.RepeatCount">repeated</see></b>
			while the user holds down the pressed key over time.
		</para>
		<para>
			Handling a <see cref="KeyDownEvent"/>,
			i.e. calling <c><paramref name="event"/>.<see cref="UserInputEvent.StopProcessing">StopProcessing</see>()</c>,
			will <b>discard</b> the upcoming <see cref="TextInputEvent"/>.
		</para>
	</remarks>
	**/
	void OnKeyDown(KeyDownEvent @event);

	/**
	<summary>
		<para>
			Called when a keyboard key is released by the user while the host window is focused.
		</para>
	</summary>
	**/
	void OnKeyUp(KeyUpEvent @event);

	#endregion
	#region Render events

	/**
	<summary>
		<para>
			See <see cref="IDeviceDependentResourceInitializer.OnDeviceInit(IDeviceIndependentResourceDictionary, IDeviceDependentResourceDictionary)"/>.
			This callback method is called just <b>after</b> the initializers, with the same goals.
		</para>
	</summary>
	**/
	void OnDeviceInit(DeviceInitInfo info);

	/**
	<summary>
		<para>
			A callback method that is invoked when the host window window runs a render pass.
			Implementations are responsible for drawing whatever contents are relevant for the application.
		</para>
	</summary>
	<remarks>
		<para>
			At call time, <see cref="ID2D1RenderTarget.BeginDraw"/> has already be called,
			the <see cref="ID2D1RenderTarget.Transform"/> will be set
			to reflect the current <see cref="IWindowBridge.Zoom"/>,
			and <see cref="ID2D1RenderTarget.EndDraw"/> will be called
			after <see cref="OnRender(RenderInfo)"/>.
		</para>
	</remarks>
	**/
	void OnRender(RenderInfo info);

	/**
	<summary>
		<para>
			See <see cref="IDeviceDependentResourceInitializer.OnDeviceDispose(IDeviceIndependentResourceDictionary, IDeviceDependentResourceDictionary)"/>.
			This callback method is called just <b>before</b> the initializers, with the same goals.
		</para>
	</summary>
	**/
	void OnDeviceDispose(DeviceDisposeInfo info) {}

	#endregion
}
