using System;
using System.IO;

using UnityEngine;

using YamlDotNet.Serialization;

using YamlDotNet.Serialization.NamingConventions;

namespace Game.Serialization
{
	public static class YamlHelper
	{
		private static ISerializer _serializer;

		/// <summary>
		/// Serializes data to a YAML file at the given path.
		/// </summary>
		public static void SaveToFile<T>(string path, T data)
		{
			try
			{
				string directoryPath = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);

				string yamlText = _serializer.Serialize(data);
				File.WriteAllText(path, yamlText);
			}
			catch (Exception e)
			{
				Logger.LogError("Chyba při serializaci do YAML", e.ToString());
				throw;
			}
		}

		public static void Initialize()
		{
			_deserializer = new DeserializerBuilder()
						   .WithNamingConvention(PascalCaseNamingConvention.Instance)
						   .Build();
		}

		private static IDeserializer _deserializer;

		private static string LoadYamlFromFile(string path)
		{
			if (!File.Exists(path))
			{
				Logger.LogError($"Yaml soubor nenalezen: {path}");
				throw new InvalidOperationException($"Yaml file not found: {path}");
			}

			return File.ReadAllText(path);
		}

		public static void LoadFromFile<T>(string path, out T result)
		{
			try
			{
				string yamlText = LoadYamlFromFile(path);
				IDeserializer deserializer = new DeserializerBuilder()
					.WithNamingConvention(PascalCaseNamingConvention.Instance)
					.Build();
				result = deserializer.Deserialize<T>(yamlText);
			}
			catch (Exception e)
			{
				Logger.LogError("Chyba při deserializaci z YAML", e.ToString());
				throw;
			}
		}

		public static void LoadFromResources<T>(string path, out T result)
		{
			try
			{
				string yamlText = LoadYamlFromResources(path);
				result = _deserializer.Deserialize<T>(yamlText);
			}
			catch (Exception e)
			{
				Logger.LogError("Chyba při deserializaci z YAML", e.ToString());
				throw;
			}
		}

		private static string LoadYamlFromResources(string path) => Resources.Load<TextAsset>(path).text;
	}
}
