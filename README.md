Uy
====

![NuGet](https://img.shields.io/nuget/v/Uy?label=NuGet&logo=nuget)

Installation
----

Install [Uy](https://www.nuget.org/packages/Uy/) from NuGet.


Usage
----

### Mantra

> Priority to the API; make it discoverable and incremental.
> Experiment outside of the main lib; integrate new ideas only once stable.

### Application

#### Application bootstrapping

> How to setup a Uy project?

Uy integrates with the .NET generic host:

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<OutputType>WinExe</OutputType>
		<TargetFramework>net6.0-windows10.0.19041.0</TargetFramework>
		<Nullable>enable</Nullable>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Uy" Version="0.0.1"/>
	</ItemGroup>
</Project>
```

```cs
using Microsoft.Extensions.Hosting;
using Uy;

Host
	.CreateDefaultBuilder(args)
	.ConfigureServices(services => {
		services.AddUy<MainWindowRootControl>();
		services.AddTransient<MainWindowRootControl>();
	})
	.Build()
	.Run();

class MainWindowRootControl : CompositeControl { // NOTE: CompositeControl doesn't actually exist, alas...
	public MainWindowRootControl(Span helloSpan) {
		helloSpan.Text = "Hello, there!";

		Children = new CenteredLayout(helloSpan);
	}
}
```

#### Close the application

Get an `IHostApplicationLifetime` from the dependency injection container, and call its `.StopApplication()` method.

#### React to the application's closing

Get an `IHostApplicationLifetime` from the dependency injection container, and `.Register()` a callback with its `ApplicationStopping` token.

#### Last resort exception handling

> Requirements...

Solution...

### Windows

Interaction with windows goes though the `IWindowBridge` service. The implementation of this interface is [scoped](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.servicelifetime?view=dotnet-plat-ext-6.0#fields). In Uy applications, a dependency injection scope is created around each window.

#### Close the current window

Get an `IWindowBridge` from the dependency injection container, and call its `.CloseWindow()` method.

#### React to the current window's opening

Get an `IWindowBridge` from the dependency injection container, and `.Register()` a callback with its `WindowOpened` token.

#### React to the current window's closing

Get an `IWindowBridge` from the dependency injection container, and `.Register()` a callback with its `WindowClosing` token.

#### Multiple windows

> How to open another window?

Solution...

##### Main windows

> - Main windows are independent from all other windows.
> - Main windows live at the root of the application's window tree.
> - Main windows count for closing the whole application: when the last main window is closed by the user, the host application shuts down automatically.

Solution...

##### Modeless windows

> - Modeless windows are independent from other windows.
> - Like main windows, modeless windows live at the root of the application's window tree.
> - Unlike main windows, closing modeless windows doesn't participate to the application's close-countdown.

Solution...

##### Tool windows

> - Tool windows are components of their parent window: when the parent window closes, the child tool windows automatically close as well.
> - Tool windows don't live at the root of the application's window tree.
> - Unlike modal windows, tool windows don't block access to their parent window.
> - Closing tool windows doesn't participate to the application's close-countdown.

Solution...

##### Pop up windows

> Requirements...

Solution...

##### Modal windows

> Requirements...

Solution...

###### Window-wide modals

> Requirements...

Solution...

###### Branch-wide modals

> Requirements...

Solution...

###### Tree-wide modals

> Requirements...

Solution...

###### Application-wide modals

> Requirements...

Solution...

##### Dialog windows

> Requirements...

Solution...

###### Open file

> Requirements...

Solution...

###### Open multiple files

> Requirements...

Solution...

###### Open directory

> Requirements...

Solution...

###### Open multiple directories

> Requirements...

Solution...

###### Save file

> Requirements...

Solution...

###### Save multiple files

> Requirements...

Solution...

###### Save directory

> Requirements...

Solution...

###### Save multiple directories

> Requirements...

Solution...

###### Alert

> Requirements...

Solution...

###### Confirm

> Requirements...

Solution...

###### Prompt

> Requirements...

Solution...

####### Prompt enum

> Requirements...

Solution...

####### Prompt simple text

> Requirements...

Solution...

####### Prompt custom types

> Requirements...

Solution...

#### Close another window

> Requirements...

Solution...

#### React to another window's opening

> Requirements...

Solution...

#### React to another window's closing

> Requirements...

Solution...

#### Get or set the title of a window

Get an `IWindowBridge` from the dependency injection container, and use either its `.Title` property, or the `.TitleSubject` for reactive piping.

#### Get or set the state of a window

Get an `IWindowBridge` from the dependency injection container, and use either its `.State` property, or the `.StateSubject` for reactive piping.

Refer to the inline documentation of the `WindowState` enumeration's literals for possible state values and their effects. By default, all windows start in a _restored_ state.

#### Get or set the zoom level of a window

> The zoom value is a developer-controlled factor that scales the render of the window's root content.
> When the zoom factor is modified, the virtual size allocated to the window's content is updated automatically, and a render pass is triggered.
> The zoom factor is transparently applied to the drawing area.

Get an `IWindowBridge` from the dependency injection container, and use either its `.Zoom` property, or the `.ZoomSubject` for reactive piping.

#### Get the DPI scale of a window

> The DPI is updated automatically by the system.
> When the DPI scale is modified, the virtual size allocated to the window's content is updated automatically, and a render pass is triggered.
> The DPI scale is transparently applied to the drawing area.

Get an `IWindowBridge` from the dependency injection container, and refer to the inline documentation for its `.Dpi`, `.DpiScale`, `.DpiObservable`, and `.DpiScaleObservable`.

#### Get the size of a window

> The size is decided solely by the end user. Forcing a window to a fixed size or making it un-resizeable is unholesome. **Let me, the empowered user, resize my fucking windows however I wish!**
>
> The size is scaled automatically to reflect the DPI of the end user's screen and the zoom factor applied to the window.
> When the user resizes a widow, a render pass is triggered.

Get an `IWindowBridge` from the dependency injection container, and refer to the inline documentation for its `.HardwareSize`, `.ScaledSize`, `.HardwareSizeObservable`, and `.ScaledSizeObservable`.

#### Restore windows' positions on app resumption

> Requirements...

Solution...

##### Restore windows' positions with changes in monitor layouts

> Requirements...

Solution...

##### Restore windows' positions with multiple instances of the same program

> Requirements...

Solution...

### Events

> Requirements...

Solution...

User and framework events are routed to corresponding `.On*()` callback methods implemented as part of `IWindowRootContent`. Developers **must not keep a reference to the `*Event` and `*Info` parameters passed to those methods.**

#### User interactions

> Requirements...

Solution...

##### Mouse events

> Requirements...

Solution...

###### Scope mouse position through a control hierarchy

> Requirements...

Solution...

###### Mouse enter and mouse leave events

> Requirements...

Solution...

###### Mouse down and mouse up events

> Requirements...

Solution...

###### Mouse move events

> Requirements...

Solution...

###### Mouse scroll events

> Requirements...

Solution...

###### Customize the mouse cursor

> Requirements...

Solution...

####### Use system defined mouse cursors

> Requirements...

Solution...

####### Use custom images as mouse cursors

> Requirements...

Solution...

##### Keyboard events

> Requirements...

Solution...

###### Focus management

> Requirements...

Solution...

####### Focus enter and focus leave events

> Requirements...

Solution...

###### Key down and key up events

> React to keys being pressed by the user.

Hook up to the `IWindowRootContent.OnKeyDown()` and `IWindowRootContent.OnKeyUp()` callback methods of the control. Refer to the inline documentation for those methods and the types of the context events passed in parameter.

###### Differentiate keyboard devices

> Requirements...

Solution...

###### Keyboard shortcuts

> Requirements...

Solution...

####### Support for long press hold shortcuts

> Requirements...

Solution...

##### Text input events

> Requirements...

Solution...

### Clipboards

> Requirements...

Solution...

#### Text and other standard formats

> Requirements...

Solution...

#### User defined formats

> Requirements...

Solution...

#### Let the user inspect the clipboard and see the available formats before pasting

> Requirements...

Solution...

#### System-wide copy paste clipboard

> Requirements...

Solution...

#### System-wide drag and drop clipboard

> Requirements...

Solution...

#### Application-wide copy paste clipboard

> Requirements...

Solution...

#### Application-wide drag and drop clipboard

> Requirements...

Solution...

#### Custom drop actions

> Requirements...

Solution...

#### Drag ghost

> Requirements...

Solution...

#### Drop ghost

> Requirements...

Solution...

### Render

> Requirements...

Solution...

#### Graphic resources

> Requirements...

Solution...

##### Graphic resources lifecycle scoping and disposal

> There are two types of graphic resources:
>
> - _Device-independent_ resources are created in the CPU's memory, they can live through the entire lifetime of the application;
> - _Device-dependent_ resources are created in the GPU's memory, the GPU, i.e. the _device_, can get _"lost"_ or _"reset"_ for shit tons of stupid reasons, making up the need to re-create these associated resources. A device is associated with a window.
>
> The graphic resources are also _scoped_, i.e. they are created and disposed along with either:
>
> - The whole application;
> - A window;
> - A specific control.

###### Application-wide device-independent resources

Add a custom implementation of `IDeviceIndependentResourceInitializer` in the dependency injection container, and look for its `.OnInit()` callback method.

###### Application-wide device-dependent resources

These don't exist. You want [window-wide device-dependent resources](#window-wide-device-dependent-resources).

###### Window-wide device-independent resources

These don't exist. You want [control-wide device-independent resources](#control-wide-device-independent-resources).

###### Window-wide device-dependent resources

Add a custom implementation of `IDeviceDependentResourceInitializer` in the dependency injection container, and look for its `.OnDeviceInit()` callback method.

###### Control-wide device-independent resources

Get the `IDeviceIndependentResourceDictionary` from the dependency injection container; the singleton implementation is already filled with a few Direct2D factories, which can be accessed as properties of the dictionary. Resources created from these factories can be stored in fields of the control, and must be disposed with it.

<!-- TODO: an example! ```cs
using Uy;

class SampleControl : Control {
	ID2D1TextLayout TextLayout;

	public SampleControl(IDeviceIndependentResourceDictionary applicationResources) {
		applicationResources
			.WriteFactory
			.CreateTextLayout(/* ... */)
			.Tee(out TextLayout)
			.DisposeWith(this);
	}
}
``` -->

###### Control-wide device-dependent resources

Hook up to the `IWindowRootContent.OnDeviceInit()` and `IWindowRootContent.OnRender()` callback methods of the control. The `IDeviceDependentResourceDictionary` passed in parameter as part of the context info is already filled with a Direct2D device context, which can be accessed as a property of the dictionary.

<!-- TODO: an example! -->

##### Import resources from embedded assembly data

> Requirements...

Solution...

##### Import resources from disk, downloaded files and archives

> Requirements...

Solution...

##### Import resources from RAM

> Requirements...

Solution...

##### Fonts

> Requirements...

Solution...

##### Bitmaps (JPG, PNG, WEBP, GIF)

> Requirements...

Solution...

###### Sprites

> Requirements...

Solution...

##### Vector graphics (SVG)

> Requirements...

Solution...

##### Audio clips

> Requirements...

Solution...

##### Video clips

> Requirements...

Solution...

##### 3D models

> Requirements...

Solution...

#### Animations

> Requirements...

Solution...

##### Time keeping

> Requirements...

Solution...

##### Synchronize the frame rate to the monitor's refresh rate

> Requirements...

Solution...

Notes
----

### Why the splitting between `IWindowBridge` and `IWindowRootContent`?

> What differentiates `IWindowBridge` from `IWindowRootContent`?

- `IWindowRootContent` represents the content of the window, i.e. the stuff that gets drawn on screen and reacts to the user interacting with it. It does so by implementing [callback methods that will be called by the framework](https://en.wiktionary.org/wiki/Hollywood_principle) at the appropriate times.

- `IWindowBridge` provides an API (to the `IWindowRootContent`) for the developer to interact with the host window. It does so by exposing various event hook and _fire-and-forget_ utility methods.

`IWindowBridge` is for the **asynchronous** stuff; `IWindowRootContent` is for the **synchronous** stuff.

> Why not porting more things (typically the mouse and keyboard events) from `IWindowRootContent` callbacks to `IWindowBridge` Rx observable properties?

As much as possible is exposed through Rx observables in `IWindowBridge`, but implementation details sometimes caught up on a _full-Rx_ framework. User interaction events may or may not be forwaded to the OS, depending on whether the event is marked as _"processed"_ by the application code.

How should the framework react if an Rx pipeline trigger some asynchronous, off-thread, or long-running operation? By waiting for the pipeline to complete? How long is it gonna take before we realize that this keyboard shortcut was or was not intended for the OS? How do we explain this time gap to the end user?

By forcing in the use of a synchronous callback method, we avoid answering, and even asking all those tricky questions.

Source Code Structure
----

<!-- CCF5F0F0-3BED-4746-9D80-F5495579332E -->

Unlike what is the default in the C# world, directories in the source code of the main assembly are not supposed to provide sub-namespaces. The assembly is flattened into a single `Uy` namespace, while preserving a nice organization in the solution explorer IDE's side bar.

This decision is enforced by `Uy.Tests.SingleNamespaceTests.SingleNamespace()`.
