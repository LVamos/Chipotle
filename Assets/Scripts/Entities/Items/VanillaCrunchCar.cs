using Game.Messaging.Events.Physics;

using ProtoBuf;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the car of the Vanilla crunch company (auto v1) object standing in the garage of
	/// the company (garáž v1) locality.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class VanillaCrunchCar : Item
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>

		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
										=> base.Initialize(name, area, type, decorative, pickable, usable);

		/// <summary>
		/// A reference to the key hanger (věšák v1) object in the garage of the Vanilla crunch
		/// company (garáž v1) locality.
		/// </summary>
		private KeyHanger KeyHanger
			=> World.GetItem("věšák v1") as KeyHanger;

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			_sounds["action"] = KeyHanger.KeysHanging ? "snd14" : "snd4";
			base.OnObjectsUsed(message);
		}
	}
}