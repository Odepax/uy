using LinqToYourDoom;
using System;
using System.Diagnostics;
using System.IO;

namespace Uy;

/**
<summary>
	<para>
		<see cref="XCopyInstallation"/> reexposes the paths
		of standard data and temp' directories, both system- or user-scoped.
	</para>
	<para>
		<see cref="SoftwareGroup"/> allows for a finer granularity of data directory:
		a software program or application is thought to be composed of several <see cref="SoftwareGroup"/>s,
		independently named and versioned.
	</para>
</summary>
<remarks>
	<para>
		<u>About State Directories and Scopes</u>
	</para>
	<para>
		<i>State directories</i> are file-system folders for storing data.
		They are divided into a 4 scopes:
	</para>
	<list type="bullet">
		<item>
			<term><see cref="StateScope.Data"/> or <see cref="StateScope.Temp"/></term>
			<description>
				<para>
					<see cref="StateScope.Data">User data</see>
					is supposed to be persistent between software and system reboots,
					making it a great choice for e.g. application settings,
					whereas <see cref="StateScope.Temp">temporary data</see>
					could be removed by the system at any moment,
					making it a great choice for e.g. the local cache.
				</para>
			</description>
		</item>
		<item>
			<term><see cref="StateScope.Light"/> or <see cref="StateScope.Heavy"/></term>
			<description>
				<para>
					<see cref="XCopyInstallation"/> differentiates between
					<see cref="StateScope.Light">lightweight</see> and <see cref="StateScope.Heavy">heavyweight</see> data.
					System administrators may want to create junctions
					to move <see cref="StateScope.Heavy">heavyweight</see> data to the HDD,
					realeasing pressure off the SSD.
				</para>
			</description>
		</item>
		<item>
			<term><see cref="StateScope.AllUsers"/>, <see cref="StateScope.CurrentUser"/> or <see cref="StateScope.CurrentProcess"/></term>
			<description>
				<para>
					Defines scoping on a user or process basis.
				</para>
			</description>
		</item>
		<item>
			<term><see cref="StateScope.Versioned"/> or <see cref="StateScope.Unversioned"/></term>
			<description>
				<para>
					Defines scoping on a <see cref="SoftwareGroup.Version"/> basis.
				</para>
			</description>
		</item>
	</list>
	<para>
		For instance:
	</para>
	<code><![CDATA[
	var software = new SoftwareGroup("My Super Notebook", "1.0");
	var configFile = software.GetStateDirectory(Light | Data | CurrentUser | Versioned).FullName + "/appsettings.json";
	var videoCacheDir = software.GetStateDirectory(Heavy | Temp | CurrentProcess).FullName;
	]]></code>
	<para>
		<u>About Installation Modes</u>
	</para>
	<para>
		A <i>software program</i> supports
		different <see cref="InstallationMode">installation modes</see>;
		it can be either:
	</para>
	<list type="bullet">
		<item><see cref="InstallationMode.Portable">Portable</see>, e.g. sitting on a USB drive.</item>
		<item><see cref="InstallationMode.Machine">Installed system-wide</see>, and accessible by all users.</item>
		<item><see cref="InstallationMode.User">Installed for the current user</see> only.</item>
	</list>
	<para>
		The <see cref="CurrentInstallationMode"/>
		determines the base paths of the <i>state directories</i>.
		Its value is read from a file named after <see cref="InstallationStateFileName"/>,
		located right next to the current executable;
		the JSON object defined in this file contains a single <c>InstallationMode</c> key,
		which will be deserialized into an <see cref="InstallationMode"/> literal.
		In the absence of this file, the <see cref="CurrentInstallationMode"/>
		defaults to <see cref="InstallationMode.Portable"/>.
	</para>
</remarks>
**/
public static class XCopyInstallation {
	#region Current process

	public const string InstallationStateFileName = ".XCopy.json";

	class InstallationState {
		public InstallationMode InstallationMode { get; set; }
	}

	public static readonly FileInfo CurrentExecutable;
	public static readonly InstallationMode CurrentInstallationMode;
	public static readonly int CurrentProcessId;
	public static readonly string CurrentUserName;

	static XCopyInstallation() {
		try {
			using var currentProcess = Process.GetCurrentProcess();

			CurrentExecutable = new FileInfo(currentProcess.MainModule!.FileName!);

			var path = Path.Join(CurrentExecutable.DirectoryName!, InstallationStateFileName);

			CurrentInstallationMode = File.Exists(path)
				? File.ReadAllText(path).ToJsonObjectOrDefault<InstallationState>().InstallationMode
				: InstallationMode.Portable;

			CurrentProcessId = currentProcess.Id;
			CurrentUserName = Environment.UserName;
		}

		catch (Exception e) {
			throw new Bug("CA50FA02-CC60-442A-89AF-5FF21FA92FA5", e);
		}
	}

	#endregion
	#region Default base directories

	public const string PortableStateDirectoryName = ".XCopyPortable";
	public const string InstalledStateDirectoryName = "XCopyInstall";

	/**
	<summary>
		<para>
			Returns the default base state directory path
			for the given <paramref name="installationMode"/> and <paramref name="scope"/>.
		</para>
	</summary>
	<exception cref="ArgumentException">
		<para>
			When the <paramref name="scope"/> doesn't set at least
			<see cref="StateScope.Data"/> or <see cref="StateScope.Temp"/>.
		</para>
	</exception>
	**/
	public static string GetDefaultStateBaseDirectoryPath(InstallationMode installationMode, StateScope scope) {
		if (installationMode == InstallationMode.Portable)
			return Path.Join(CurrentExecutable.DirectoryName!, PortableStateDirectoryName);

		else if (scope.HasFlag(StateScope.Temp))
			return Path.Join(Path.GetTempPath(), InstalledStateDirectoryName); // C:/Users/John/AppData/Local/Temp/...

		else if (installationMode == InstallationMode.Machine && scope.HasFlag(StateScope.Data | StateScope.AllUsers))
			return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.DoNotVerify), InstalledStateDirectoryName); // C:/ProgramData/...

		else if (scope.HasFlag(StateScope.Data))
			return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), InstalledStateDirectoryName); // C:/Users/John/AppData/Roaming/...

		else throw new ArgumentException("Unknown scope.", nameof(scope));
	}

	#endregion
}
