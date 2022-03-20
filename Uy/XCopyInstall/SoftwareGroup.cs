using System;
using System.IO;
using static Uy.StateScope;

namespace Uy;

/**
<inheritdoc cref="XCopyInstallation"/>
**/
public sealed class SoftwareGroup {
	public readonly InstallationMode InstallationMode;
	public readonly string Name;
	public readonly string Version;

	public SoftwareGroup(string name, string version) : this(XCopyInstallation.CurrentInstallationMode, name, version) {}
	public SoftwareGroup(InstallationMode installationMode, string name, string version) {
		InstallationMode = installationMode;
		Name = name;
		Version = version;
	}

	#region State directories

	/**
	<summary>
		<para>
			Returns the state directory of <see langword="this"/> <see cref="SoftwareGroup"/>
			for the given <paramref name="scope"/>.
		</para>
	</summary>
	<param name="scope">
		<para>
			<see cref="CurrentProcess"/> is exclusive with
			<see cref="Data"/>,
			<see cref="Versioned"/>,
			or <see cref="Unversioned"/>.
		</para>
	</param>
	<exception cref="ArgumentException">
		<para>
			When the <paramref name="scope"/> is invalid.
		</para>
	</exception>
	**/
	public DirectoryInfo GetStateDirectory(StateScope scope) => new(Path.Join(
		XCopyInstallation.GetDefaultStateBaseDirectoryPath(InstallationMode, scope),
		scope switch {
			Data | Light | AllUsers /*       */ | Unversioned /* */ => Path.Join("LD", 'G' + Name, "A", /*           */ "A"),
			Data | Light | AllUsers /*       */ | Versioned /*   */ => Path.Join("LD", 'G' + Name, 'V' + Version, /* */ "A"),
			Data | Light | CurrentUser /*    */ | Unversioned /* */ => Path.Join("LD", 'G' + Name, "A", /*           */ 'U' + XCopyInstallation.CurrentUserName),
			Data | Light | CurrentUser /*    */ | Versioned /*   */ => Path.Join("LD", 'G' + Name, 'V' + Version, /* */ 'U' + XCopyInstallation.CurrentUserName),
			Data | Heavy | AllUsers /*       */ | Unversioned /* */ => Path.Join("HD", 'G' + Name, "A", /*           */ "A"),
			Data | Heavy | AllUsers /*       */ | Versioned /*   */ => Path.Join("HD", 'G' + Name, 'V' + Version, /* */ "A"),
			Data | Heavy | CurrentUser /*    */ | Unversioned /* */ => Path.Join("HD", 'G' + Name, "A", /*           */ 'U' + XCopyInstallation.CurrentUserName),
			Data | Heavy | CurrentUser /*    */ | Versioned /*   */ => Path.Join("HD", 'G' + Name, 'V' + Version, /* */ 'U' + XCopyInstallation.CurrentUserName),
			Temp | Light | AllUsers /*       */ | Unversioned /* */ => Path.Join("LT", 'G' + Name, "A", /*           */ "A"),
			Temp | Light | AllUsers /*       */ | Versioned /*   */ => Path.Join("LT", 'G' + Name, 'V' + Version, /* */ "A"),
			Temp | Light | CurrentUser /*    */ | Unversioned /* */ => Path.Join("LT", 'G' + Name, "A", /*           */ 'U' + XCopyInstallation.CurrentUserName),
			Temp | Light | CurrentUser /*    */ | Versioned /*   */ => Path.Join("LT", 'G' + Name, 'V' + Version, /* */ 'U' + XCopyInstallation.CurrentUserName),
			Temp | Light | CurrentProcess /*                     */ => Path.Join("LT", 'G' + Name, "P", /*           */ 'P' + XCopyInstallation.CurrentProcessId.ToString()),
			Temp | Heavy | AllUsers /*       */ | Unversioned /* */ => Path.Join("HT", 'G' + Name, "A", /*           */ "A"),
			Temp | Heavy | AllUsers /*       */ | Versioned /*   */ => Path.Join("HT", 'G' + Name, 'V' + Version, /* */ "A"),
			Temp | Heavy | CurrentUser /*    */ | Unversioned /* */ => Path.Join("HT", 'G' + Name, "A", /*           */ 'U' + XCopyInstallation.CurrentUserName),
			Temp | Heavy | CurrentUser /*    */ | Versioned /*   */ => Path.Join("HT", 'G' + Name, 'V' + Version, /* */ 'U' + XCopyInstallation.CurrentUserName),
			Temp | Heavy | CurrentProcess /*                     */ => Path.Join("HT", 'G' + Name, "P", /*           */ 'P' + XCopyInstallation.CurrentProcessId.ToString()),

			_ => throw new ArgumentException($"Unknown scope. Keep in mind that '{ nameof(CurrentProcess) }' is exclusive with '{ nameof(Data) }', '{ nameof(Versioned) }' or '{ nameof(Unversioned) }'. Make sure the scope sets at least a bit for each bit mask: {{ {nameof(Data)}|{nameof(Temp)} }}{{ {nameof(Light)}|{nameof(Heavy)} }}{{ {nameof(AllUsers)}|{nameof(CurrentUser)}|{nameof(CurrentProcess)} }}{{ {nameof(Versioned)}|{nameof(Unversioned)} }}.", nameof(scope))
		}
	));

	#endregion
}
