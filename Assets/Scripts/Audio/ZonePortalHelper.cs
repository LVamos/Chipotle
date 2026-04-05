using Assets.Scripts.Models;

using Game.Terrain;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Audio
{
	/// <summary>
	/// Provides utility methods for portal-related calculations such as position and volume.
	/// Does not depend on Unity components or game state mutation.
	/// </summary>
	public static class ZonePortalHelper
	{
		private static float CalculatePortalVolume(Passage passage, Zone zone, ZoneLoopInfo loop)
		{
			if (PlayersZone.GetZonesBehindDoor().FirstOrDefault(a => a.IsBehindDoor(zone)) is Zone between)
				if (PlayersZone.GetOpenExits(between).IsNullOrEmpty())
					return loop.Volume * .01f;

			return passage.State == PassageState.Closed
				? Sounds.GetOverClosedDoorVolume(loop.Volume)
				: loop.Volume;
		}

		public static string GetDescription(Passage passage, string zoneName)
		{
			Zone[] zones = passage.Zones.ToArray();
			string description = $"3d portal ambient for {zoneName}; passage between {zones[0].Name.Inner} and {zones[1].Name.Inner}";
			return description;
		}

		private static Dictionary<Passage, Vector3> GetPortalPositions(Zone zone)
		{
			Dictionary<Passage, Vector3> positions = new Dictionary<Passage, Vector3>();
			Vector2 player = World.Player.Center;
			Zone playersZone = World.Player.Zone;

			foreach (Passage exit in zone.Exits)
			{
				Vector2 position = exit.Area.Value.Contains(player)
					? exit.AnotherZone(playersZone).Area.Value.GetClosestPoint(player)
					: exit.Area.Value.GetAlignedPoint(player) ?? exit.Area.Value.GetClosestPoint(player);

				if (exit.LeadsTo(playersZone))
					position = playersZone.Area.Value.GetAlignedPoint(position).Value;

				positions[exit] = position.ToVector3(2);
			}

			return positions;
		}

		public static List<PortalAnchor> GetAnchors(Zone zone, ZoneLoopInfo loop)
		{
			Dictionary<Passage, Vector3> positions = GetPortalPositions(zone);
			List<PortalAnchor> anchors = new();

			foreach (Passage exit in zone.Exits)
			{
				float volume = CalculatePortalVolume(exit, zone, loop);
				anchors.Add(new PortalAnchor(exit, positions[exit], volume < loop.Volume * .02f));
			}

			return anchors;
		}


		private static Zone PlayersZone => World.Player.Zone;
	}
}
