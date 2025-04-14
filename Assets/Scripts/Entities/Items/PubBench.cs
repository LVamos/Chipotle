using Assets.Scripts.Entities.Items;

using Game.Entities.Characters;
using Game.Messaging.Events.Physics;

using ProtoBuf;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents a table in the pub (výčep h1) locality.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PubBench : InteractiveItem
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable)
			=> base.Initialize(name, area, type, decorative, pickable, usable);

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			Character tuttle = World.GetCharacter("tuttle");

			if (
				!World.GetLocality("ulice h1").IsItHere(tuttle)
				&& !SameLocality(tuttle)
			)
				_cutscene = "cs24";
			else
				_cutscene = "cs25";

			base.OnObjectsUsed(message);
		}
	}
}