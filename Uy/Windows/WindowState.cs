using Windows.Win32.UI.WindowsAndMessaging;

namespace Uy;

public enum WindowState : uint {
	/**
	<summary>
		<para>
			The window is not displayed on the desktop,
			but a button can be found in the Task Bar in order to get it back.
		</para>
		<code><![CDATA[
		,---------------------------------------------------------.
		|                                                         |
		|                             ,-------------------.       |
		|                             | [B] … [_] [O] [X] |       |
		|                             |-------------------|       |
		|                             | B B B B B B B B B |       |
		|                             |  B B B B B B B B  |       |
		|                             | B B B B B B B B B |       |
		|                             `-------------------'       |
		|---------------------------------------------------------|
		| 88 | 9 Search…    | = [A] [B]        ^ || @ <) 3:05 [,] |
		`---------------------------------------------------------'
		]]></code>
		<para>
			Here, <i>A</i> is your application,
			while <i>B</i> is another application showcased for comparison;
			<i>B</i>'s window is <see cref="Restored"/>.
		</para>
	</summary>
	**/
	Minimized = SHOW_WINDOW_CMD.SW_MINIMIZE,

	/**
	<summary>
		<para>
			The window is happily floating somewhere on the desktop.
		</para>
		<code><![CDATA[
		,---------------------------------------------------------.
		|    ,-----------------------------------.                |
		|    | [A] Your Application  [_] [O] [X] | -------.       |
		|    |-----------------------------------| O] [X] |       |
		|    | A A A A A A A A A A A A A A A A A | -------|       |
		|    |  A A A A A A A A A A A A A A A A  |  B B B |       |
		|    `-----------------------------------' B B B  |       |
		|                             | B B B B B B B B B |       |
		|                             `-------------------'       |
		|---------------------------------------------------------|
		| 88 | 9 Search…    | = [A] [B]        ^ || @ <) 3:05 [,] |
		`---------------------------------------------------------'
		]]></code>
		<para>
			Here, <i>A</i> is your application,
			while <i>B</i> is another application showcased for comparison;
			<i>B</i>'s window is <see cref="Restored"/>.
		</para>
	</summary>
	**/
	Restored = SHOW_WINDOW_CMD.SW_RESTORE,

	/**
	<summary>
		<para>
			The window takes all the space available on the desktop.
		</para>
		<code><![CDATA[
		,---------------------------------------------------------.
		| [A] Your Application                        [_] [o] [X] |
		|---------------------------------------------------------|
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		|---------------------------------------------------------|
		| 88 | 9 Search…    | = [A] [B]        ^ || @ <) 3:05 [,] |
		`---------------------------------------------------------'
		]]></code>
		<para>
			Here, <i>A</i> is your application,
			while <i>B</i> is another application showcased for comparison;
			<i>B</i>'s window is <see cref="Restored"/>.
		</para>
	</summary>
	<remarks>
		<para>
			Other applications are still running.
		</para>
	</remarks>
	**/
	Maximized = SHOW_WINDOW_CMD.SW_MAXIMIZE,

	/**
	<summary>
		<para>
			The window phagocytizes the entire screen, covering even the Task Bar and the title bar.
		</para>
		<code><![CDATA[
		,-----------------------------------------------------------.
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		| A A A A A A A A A A A A A A A A A A A A A A A A A A A A A |
		|  A A A A A A A A A A A A A A A A A A A A A A A A A A A A  |
		`-----------------------------------------------------------'
		]]></code>
		<para>
			Here, <i>A</i> is your application,
			while <i>B</i> is another application showcased for comparison;
			<i>B</i>'s window is <see cref="Restored"/>.
		</para>
	</summary>
	<remarks>
		<para>
			Other applications are still running.
			I hope you know what you're doing with this value...
		</para>
	</remarks>
	**/
	FullScreen = 666
}
