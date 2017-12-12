using System;
using System.Collections.Generic;
using System.Linq;

namespace PassWinmenu.Configuration
{

	public class VersionedConfigData
	{
		public ConfigVersion Version { get; }
		public Dictionary<string, object> ConfigData { get; }

		public VersionedConfigData(ConfigVersion version, Dictionary<string, object> configData)
		{
			Version = version;
			ConfigData = configData;
		}
	}

	public static class UpgradeHelpers
	{
		public static void MoveNode(this Dictionary<string, object> configData, string source, string destination)
		{
			;
			var value = configData.GetValue(source);
		}

		public static object GetValue(this Dictionary<string, object> configData, string path)
		{
			var split = path.Split('.');
			var node = split.First();
			var hasNode = configData.ContainsKey(node);
			if (split.Length == 1)
			{
				return hasNode ? configData[node] : null;
			}
			else if(hasNode)
			{
				var asDict = configData[node] as Dictionary<string, object>;
				if (asDict == null)
				{
					throw new InvalidConfigException($"Expected {path} to be a property.");
				}
				else
				{
					
				}
			}
		}
	}

	public class UpgradeException : Exception
	{
		public UpgradeException(string message) : base(message) { }
	}

	public class InvalidConfigException : UpgradeException
	{
		public InvalidConfigException(string message) : base(message) { }
	}

	public static class UpgradeFunctions
	{
		public static VersionedConfigData UpgradeFrom_V0_1(Dictionary<string, object> configData)
		{
			configData.MoveNode("gpg-path", "gpg.gpg-path");
			configData.MoveNode("gnupghome-override", "gpg.gnpghome-override");
			configData.MoveNode("preload-gpg-agent", "gpg.gpg-agent.preload");
			configData.MoveNode("pinentry-fix", "gpg.pinentry-fix");

			return new VersionedConfigData(ConfigVersion.V1_0, configData);
		}
	}

	public class ConfigUpgrader
	{
		// Defines the signature for a function that transforms a config tree from one version to another.
		private delegate VersionedConfigData UpgradeFunction(Dictionary<string, object> source);
		// Maps config file versions to upgrade functions.
		private readonly Dictionary<ConfigVersion, UpgradeFunction> upgrades;

		public ConfigUpgrader()
		{
			upgrades = new Dictionary<ConfigVersion, UpgradeFunction>
			{
				{ConfigVersion.V0_1, UpgradeFunctions.UpgradeFrom_V0_1}
			};
		}

		internal Config UpgradeFrom(ConfigVersion fileVersion, Dictionary<string, object> configData)
		{
			VersionedConfigData currentConfig = new VersionedConfigData(fileVersion, configData);

			while (currentConfig.Version != ConfigVersion.LatestVersion)
			{
				currentConfig = UpgradeFrom(currentConfig);
			}

			// Transform data into a Config object.
			return null;
		}

		internal VersionedConfigData UpgradeFrom(VersionedConfigData source)
		{
			return upgrades[source.Version](source.ConfigData);
		}
	}
}
