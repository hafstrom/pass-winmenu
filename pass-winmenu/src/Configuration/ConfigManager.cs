using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PassWinmenu.Configuration
{
	internal partial class ConfigManager
	{
		public static Config Config { get; private set; } = new Config();

		public static LoadResult Load(string fileName)
		{
			if (!File.Exists(fileName))
			{
				Log.Send($"No config file with name {fileName} exists, creating a new one.");
				try
				{
					using (var defaultConfig = EmbeddedResources.DefaultConfig)
					using (var configFile = File.Create(fileName))
					{
						defaultConfig.CopyTo(configFile);
					}
				}
				catch (Exception e) when (e is FileNotFoundException || e is FileLoadException || e is IOException)
				{
					Log.Send("Failed to create a new config file: An exception occurred.", LogLevel.Error);
					Log.ReportException(e);
					return LoadResult.FileCreationFailure;
				}

				return LoadResult.NewFileCreated;
			}
			ConfigVersion fileVersion;
			try
			{
				fileVersion = GetVersion(fileName);
			}
			catch (Exception e)
			{
				throw new Exception("Could not determine configuration file version: " + e.Message);
			}
			if (fileVersion.IsLatestVersion())
			{
				Config = ParseConfigFile<Config>(fileName);
				return LoadResult.Success;
			}
			else
			{
				Log.Send($"Config file is at version {fileVersion}, upgrading to {ConfigVersion.LatestVersion}");
				Config = new ConfigUpgrader().UpgradeFrom(fileVersion, ParseConfigFile<dynamic>(fileName));
				return LoadResult.FileUpgraded;
			}

		}

		private static T ParseConfigFile<T>(string fileName)
		{
			var deserialiser = new Deserializer(namingConvention: new HyphenatedNamingConvention());
			using (var reader = File.OpenText(fileName))
			{
				return deserialiser.Deserialize<T>(reader);
			}
		}

		private static ConfigVersion GetVersion(string fileName)
		{
			var config = ParseConfigFile<Dictionary<string, object>>(fileName);
			if (config.ContainsKey("config-version"))
			{
				var versionString = config["config-version"] as string ?? throw new InvalidOperationException("Config version is invalid");
				return ConfigVersion.Parse(versionString);
			}
			else
			{
				// If no version string is available, the config version is considered to be v0.1.
				return ConfigVersion.V0_1;
			}
		}
	}
}
