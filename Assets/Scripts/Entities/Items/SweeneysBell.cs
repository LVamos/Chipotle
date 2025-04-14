using Assets.Scripts.Entities.Items;

using Game.Messaging.Events.Physics;

using ProtoBuf;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the Sweeney's bell (zvonek p1) in the Easterby street (ulice p1) locality.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class SweeneysBell : InteractiveItem
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable,
			bool usable)
			=> base.Initialize(name, area, type, decorative, pickable, usable);

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			if (!Used)
				_cutscene = "cs23";
			else
				_sounds["action"] = "snd25";

			base.OnObjectsUsed(message);
		}
	}
}