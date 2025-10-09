using Game;
using Game.Entities.Characters;
using Game.Messaging.Commands.Movement;
using Game.Terrain;
using Game.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using YamlDotNet.Serialization;

using YamlDotNet.Serialization.NamingConventions;

namespace Assets.Scripts.Debug
{
	public class DebugManager : VirtualWindow
	{
		private const string _walkablePointsPath = "WalkablePoints.yaml";

		private Dictionary<string, List<Vector2>> _walkablePoints;

		public override void Initialize()
		{
			base.Initialize();

			LoadWalkablePoints();

			RegisterShortcuts
				(
					(new(KeyCode.F10), JumpToZoneMenu)
				);
		}

		private void LoadWalkablePoints()
		{
			IDeserializer deserializer = new DeserializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();

			string path = Path.Combine(MainScript.DebugPath, _walkablePointsPath);
			string yaml = File.ReadAllText(path);
			var raw = deserializer.Deserialize<Dictionary<string, List<float[]>>>(yaml);
			_walkablePoints = raw.ToDictionary(k => k.Key, v => v.Value.Select(p => new Vector2(p[0], p[1])).ToList());

		}

		/// <summary>
		/// Opens a menu with all zones and jumps to the nearest walkable position in the selected zone.
		/// </summary>
		private void JumpToZoneMenu()
		{
			Character player = World.Player;
			IEnumerable<Zone> zones = World.GetZones();
			List<List<string>> items =
			(
				from z in zones
				orderby z.Name.Indexed
				select (new List<string> { z.Name.Indexed })
			).ToList();

			MenuParameters parameters = new
				(
				items,
				"Vyber lokaci",
				menuClosed: (int item) => JumpToZone(item, items)
				);
			WindowHandler.Menu(parameters);
		}

		private void JumpToZone(int item, List<List<string>> items)
		{
			if (item == -1)
				return;

			string zone = items[item][0];
			Vector2 pointForPlayer = _walkablePoints[zone][0];
			SetPosition message1 = new(this, pointForPlayer);
			World.Player.TakeMessage(message1);

			// Move Tuttle
			Vector2 pointForTuttle = _walkablePoints[zone][1];
			Character tuttle = World.GetCharacter("tuttle");
			SetPosition message2 = new(null, pointForTuttle);
			tuttle.TakeMessage(message2);
		}
	}
}
