using ProtoBuf;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the tělo w1 object in the bazén w1 locality.
	/// </summary>
	/// <remarks>
	/// The object is destroyed when the Detective's car object moves out of the příjezdová cesta w1 locality.
	/// </remarks>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class Corpse : Item
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name for the object</param>
		/// <param name="area">The coordinates of the area that the object occupies</param>
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable)
			=> base.Initialize(name, area, type, decorative, pickable, usable, null, null, null, "cs5", true);

		/// <summary>
		/// Returns a reference to the Chipotle's car object.
		/// </summary>
		private ChipotlesCar Car
			=> World.GetItem("detektivovo auto") as ChipotlesCar;

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();

			if (Car.Moved)
				DestroyObject();
		}
	}
}