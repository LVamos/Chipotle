using Game.Messaging.Events.Physics;

using ProtoBuf;

using UnityEngine;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the bin object in the Walsch's pool (bazén w1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PoolsideBin : Item
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>

		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
										=> base.Initialize(name, area, type, decorative, pickable, usable, null, null, null, "cs3", true);

		/// <summary>
		/// Processes the message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			base.OnObjectsUsed(message);

			if (UsedOnce)
				Move(new(new Vector2(911, 1042)));
		}
	}
}