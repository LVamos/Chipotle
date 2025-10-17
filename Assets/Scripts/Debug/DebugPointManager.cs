using DavyKager;

using Game.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Game.Debug
{
	/// <summary>
	/// Handles creation and persistence of debug points into a YAML file.
	/// </summary>
	public static class DebugPointManager
	{
		private static Dictionary<Vector2, string> _points;
		private const string _fileName = "DebugPoints.yaml";

		/// <summary>
		/// Initializes or reloads the debug points from the YAML file into memory.
		/// </summary>
		public static void Initialize()
		{
			string directory = MainScript.DebugPath;
			string path = Path.Combine(directory, _fileName);
			List<DebugPoint> points = LoadExistingPointsFromFile(path);

			_points = new Dictionary<Vector2, string>();
			foreach (DebugPoint point in points)
			{
				Vector2 position = point.Position.ToVector2();
				_points[position] = point.Name;
			}
		}

		/// <summary>
		/// Gets the dictionary of debug points currently in memory.
		/// </summary>
		/// <returns>Dictionary mapping positions to point names.</returns>
		public static Dictionary<Vector2, string> GetPoints()
		{
			if (_points == null)
				Initialize();

			return new Dictionary<Vector2, string>(_points);
		}

		/// <summary>
		/// Loads and returns all debug points from the YAML file. Returns an empty list when not found or on error.
		/// </summary>
		public static List<DebugPoint> LoadPoints()
		{
			if (_points == null)
				Initialize();

			return _points.Select(kvp => new DebugPoint
			{
				Position = new SerializableVector2(kvp.Key),
				Name = kvp.Value
			}).ToList();
		}

		/// <summary>
		/// Prompts for a point name and saves the current player's position with that name into a YAML list.
		/// If the user cancels or enters no name, the method exits silently.
		/// On success, the method speaks a confirmation using a screen reader.
		/// </summary>
		public static void PlaceDebugPoint()
		{
			string pointName = WindowHandler.InputBox("Zadej název testovacího bodu", "Testovací bod");
			if (string.IsNullOrWhiteSpace(pointName))
				return;

			Vector2 playerPosition = World.Player.Center;

			// Add to memory
			if (_points == null)
				Initialize();

			_points[playerPosition] = pointName;

			// Save to file
			DebugPoint newPoint = new DebugPoint
			{
				Position = new SerializableVector2(playerPosition),
				Name = pointName
			};

			string directory = MainScript.DebugPath;
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			string path = Path.Combine(directory, _fileName);

			List<DebugPoint> pointsToSave = LoadExistingPointsFromFile(path);
			pointsToSave.Add(newPoint);

			ISerializer serializer = new SerializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();

			string yaml = serializer.Serialize(pointsToSave);
			File.WriteAllText(path, yaml, Encoding.UTF8);

			Tolk.Speak("Bod uložen", true);
		}

		/// <summary>
		/// Loads existing points from the YAML file if it exists; otherwise returns an empty list.
		/// Any deserialization error results in returning an empty list (fault-tolerant).
		/// </summary>
		private static List<DebugPoint> LoadExistingPointsFromFile(string path)
		{
			if (!File.Exists(path))
				return new List<DebugPoint>();

			try
			{
				string yaml = File.ReadAllText(path, Encoding.UTF8);
				if (string.IsNullOrWhiteSpace(yaml))
					return new List<DebugPoint>();

				IDeserializer deserializer = new DeserializerBuilder()
					.WithNamingConvention(PascalCaseNamingConvention.Instance)
					.Build();

				List<DebugPoint> existing = deserializer.Deserialize<List<DebugPoint>>(yaml);
				return existing ?? new List<DebugPoint>();
			}
			catch (Exception)
			{
				return new List<DebugPoint>();
			}
		}
	}
}