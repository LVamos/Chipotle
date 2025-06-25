using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Physics;
using Game.Terrain;

using ProtoBuf;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the killer's car object in the garage of Vanilla crunch (garáž v1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class KillersCar : Item
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>

		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
					=> base.Initialize(name, area, type, decorative, pickable, usable);

		/// <summary>
		/// Indicates if the Detective Chipotle NPC had icecream from the icecream machine (automat
		/// v1) object.
		/// </summary>
		private bool ChipotleHadIcecream
			=> (World.GetItem("automat v1") as IcecreamMachine).Used;

		/// <summary>
		/// Reference to the Detective's car (detektivovo auto) object.
		/// </summary>
		private ChipotlesCar ChipotlesCar
			=> World.GetItem("detektivovo auto") as ChipotlesCar;

		/// <summary>
		/// Indicates if the keys are on the hanger.
		/// </summary>
		private bool KeysOnHanger
			=> (World.GetItem("věšák v1") as KeyHanger).KeysHanging;

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			if (KeysOnHanger)
				_sounds["action"] = "snd14";
			else if (ChipotleHadIcecream)
				_cutscene = "cs7";
			else
			{
				_cutscene = "cs8";
				Zone destination = World.GetZone("ulice h1");
				MoveChipotlesCar newMessage = new(this, destination);
				ChipotlesCar.TakeMessage(newMessage);
			}

			base.OnObjectsUsed(message);
		}
	}
}