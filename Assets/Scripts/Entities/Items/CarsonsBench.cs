// No changes needed as the file already includes `using Assets.Scripts.Entities.Items`.
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Physics;

using ProtoBuf;

using System.Linq;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents a bench object in the zahrada c1 zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CarsonsBench : Item
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name for the object</param>
		/// <param name="area">The coordinates of the area that the object occupies</param>
		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
					=> base.Initialize(name, area, type, decorative, pickable, usable, null, null, null, "cs32", true);

		/// <summary>
		/// Returns a reference to the Chipotle's car object.
		/// </summary>
		private ChipotlesCar Car
			=> World.GetItem("detektivovo auto") as ChipotlesCar;

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			if (
				!World.GetItemsByType("lavice u Carsona")
					.Any(o => o.Used)
			)
			{
				base.OnObjectsUsed(message);
				Car.TakeMessage(new UnblockZone(this, World.GetZone("ulice v1")));
			}
		}
	}
}