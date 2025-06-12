using Game.Entities.Characters;

using ProtoBuf;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a door in the hall of the Vanilla crunch company (hala v1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class HallDoor : Door
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the door</param>
		/// <param name="area">Coordinates of the area the door occupies</param>
		/// <param name="zones">The zones connected by the door</param>
		public override void Initialize(Name name, Rectangle area, IEnumerable<string> zones)
			=> base.Initialize(name, PassageState.Locked, area, zones);
		/// <summary>
		/// Opens the door if possible.
		/// </summary>
		protected override void Open(object sender, Vector2 point)
		{
			if (!World.GetItem("lavička w1").Used)
				Rattle(sender as Character, point);
			else base.Open(sender, point);
		}
	}
}