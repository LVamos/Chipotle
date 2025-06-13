using Game.Entities.Items;
using Game.Messaging.Commands.Physics;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a door in the hall of the Vanilla crunch company (hala v1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class HallDoor : Door
	{
		protected override void OnUseDoor(UseDoor message)
		{
			if (State == PassageState.Locked)
			{
				Item bench = World.GetItem("lavička w1");
				if (bench.Used)
					State = PassageState.Closed;
			}

			base.OnUseDoor(message);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the door</param>
		/// <param name="area">Coordinates of the area the door occupies</param>
		/// <param name="zones">The zones connected by the door</param>
		public override void Initialize(Name name, Rectangle area, IEnumerable<string> zones)
			=> base.Initialize(name, PassageState.Locked, area, zones);
	}
}