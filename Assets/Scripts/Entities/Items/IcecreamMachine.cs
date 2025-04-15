using Game.Entities.Characters;
using Game.Messaging.Events.Physics;
using Game.Terrain;

using ProtoBuf;

using System.Linq;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the icecream machine object (automat v1) )in the hall of the Vanilla crunch
	/// company (hala v1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class IcecreamMachine : Item
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>

		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
										=> base.Initialize(name, area, type, decorative, pickable, usable, null, null, "VendingMachineLoop");

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			Character tuttle = World.GetCharacter("tuttle");
			Zone garage = World.GetZone("garáž v1");
			if (
				SameZone(tuttle)
				&& tuttle.VisitedZones.Contains(garage)
				&& World.Player.VisitedZones.Contains(garage)
			)
				_cutscene = "cs10";
			else
				_cutscene = "cs9";

			base.OnObjectsUsed(message);
		}
	}
}