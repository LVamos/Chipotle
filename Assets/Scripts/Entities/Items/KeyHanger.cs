using Game.Messaging.Events.Physics;

using ProtoBuf;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the key hanger object in the garage in Vanilla crunch company (garáž v1) locality.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class KeyHanger : Item
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable)
			=> base.Initialize(name, area, type, decorative, pickable, usable, volume: .5f);

		/// <summary>
		/// Indicates if the keys are on the hanger.
		/// </summary>
		public bool KeysHanging { get; private set; } = true;

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			_sounds["action"] = KeysHanging ? "snd6" : "snd5";
			KeysHanging = !KeysHanging;
			base.OnObjectsUsed(message);
		}
	}
}