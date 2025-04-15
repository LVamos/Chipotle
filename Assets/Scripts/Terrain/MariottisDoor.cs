using ProtoBuf;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Represents the door between the hall of the Vanilla crunch company and (hala v1) zone
	/// and the office of the Paolo Mariotti office (kancelář v1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class MariottisDoor : Door
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the door</param>
		/// <param name="area">Coordinates of the area the door occupies</param>
		/// <param name="zones">The zones connected by the door</param>
		public override void Initialize(Name name, Rectangle area, IEnumerable<string> zones)
			=> base.Initialize(name, PassageState.Closed, area, zones);

		/// <summary>
		/// Opens the door if possible.
		/// </summary>
		/// <param name="coords">
		/// The coordinates of the place on the door that an NPC is pushing on
		/// </param>
		protected override void Open(object sender, Vector2 point)
			=> World.PlayCutscene(this, "cs11");
	}
}