using Assets.Scripts.Entities.Items;
// No changes needed as the file already includes `using Assets.Scripts.Entities.Items`.
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Physics;

using ProtoBuf;

using System.Linq;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents a bench object in the zahrada c1 locality.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CarsonsBench : InteractiveItem
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name for the object</param>
		/// <param name="area">The coordinates of the area that the object occupies</param>
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable)
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
				Car.TakeMessage(new UnblockLocality(this, World.GetLocality("ulice v1")));
			}
		}
	}
}