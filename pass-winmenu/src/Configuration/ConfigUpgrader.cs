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
			var value = configData.GetValue(source);
			configData.Remove(source);
			configData.SetValue(destination, value);
			;
		}

		public static void RemoveNode(this Dictionary<string, object> configData, string path)
		{
			var split = path.Split('.');
			var node = split.First();
			if (!configData.ContainsKey(node)) return;
			if (split.Length == 1)
			{
				configData.Remove(node);
			}
			else
			{
				configData.RemoveNode(string.Join(".", split.Skip(1)));
			}
		}

		public static void SetValue(this Dictionary<string, object> configData, string path, object value)
		{
			var split = path.Split('.');
			var node = split.First();
			if (configData.ContainsKey(node))
			{
				// The config file already contains a key here.
				if (split.Length == 1)
				{
					// We're supposed to set this config key, but it already has a value.
					// This is probably an error.
					throw new InvalidConfigException($"A config key already exists at {path}");
				}
				else
				{
					// We're supposed to set a subkey of this key.
					// First, define its path.
					var subkeyPath = string.Join(".", split.Skip(1));
					// Now check the type of the node we need to set
					if (configData[node] is Dictionary<string, object> asDict)
					{
						// It's a dictionary type, so let's insert our subkey into it.
						asDict.SetValue(subkeyPath, value);
					}
					else if (configData[node] == null)
					{
						// This key is not set, so let's insert a dictionary here,
						// and then set the subkey to its correct value.
						var newDict = new Dictionary<string, object>();
						configData[node] = newDict;
						newDict.SetValue(subkeyPath, value);
					}
					else
					{
						// This key is already set to a non-dictionary value.
						// This is probably an error.
						throw new InvalidConfigException($"A config key already exists at {path}");
					}
				}
			}
			else
			{
				if (split.Length == 1)
				{
					// We don't need to insert any additional nodes, so we can set our value here.
					configData[node] = value;
				}
				else
				{
					// The config file does not yet contain a key here, so it can be created.
					var newDict = new Dictionary<string, object>();
					configData[node] = newDict;
					var subkeyPath = string.Join(".", split.Skip(1));
					newDict.SetValue(subkeyPath, value);
				}

			}
		}

		public static object GetValue(this Dictionary<string, object> configData, string path)
		{
			var split = path.Split('.');
			var node = split.First();
			if (!configData.ContainsKey(node)) return null;
			if (split.Length == 1)
			{
				return configData[node];
			}
			if (configData[node] is Dictionary<string, object> asDict)
			{
				return asDict.GetValue(string.Join(".", split.Skip(1)));
			}
			throw new InvalidConfigException($"Expected {path} to be a dictionary.");
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
			configData.MoveNode("pinentry-fix", "gpg.pinentry-fix");
			configData.MoveNode("preload-gpg-agent", "gpg.gpg-agent.preload");

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
