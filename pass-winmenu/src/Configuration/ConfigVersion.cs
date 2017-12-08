using System;
using System.Text.RegularExpressions;

namespace PassWinmenu.Configuration
{
	public class ConfigVersion
	{
		public int Major { get; }
		public int Minor { get; }

		public static ConfigVersion V0_1 = new ConfigVersion(0, 1);
		public static ConfigVersion V1_0 = new ConfigVersion(1, 0);

		// When adding a new config version, update this pointer
		public static ConfigVersion LatestVersion => V1_0;

		private ConfigVersion(int major, int minor)
		{
			Major = major;
			Minor = minor;
		}

		public bool IsLatestVersion() => this.Equals(LatestVersion);

		public static ConfigVersion Parse(string versionString)
		{
			var match = new Regex(@"(\d+)\.(\d+)").Match(versionString);
			if(!match.Success) throw new ArgumentException("Invalid config file version string.");

			var major = int.Parse(match.Groups[1].Value);
			var minor = int.Parse(match.Groups[2].Value);
				
			return new ConfigVersion(major, minor);
		}

		public override bool Equals(object obj)
		{
			var other = obj as ConfigVersion;
			return other != null &&
					Major == other.Major &&
					Minor == other.Minor;
		}

		public override int GetHashCode()
		{
			var hashCode = 317314336;
			hashCode = hashCode * -1521134295 + Major.GetHashCode();
			hashCode = hashCode * -1521134295 + Minor.GetHashCode();
			return hashCode;
		}

		public override string ToString() => $"{Major}.{Minor}";
	}
}
