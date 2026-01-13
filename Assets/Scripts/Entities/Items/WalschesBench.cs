using Assets.Scripts.Entities.Items;
using Assets.Scripts.Messaging.Commands.Characters;

using Game.Entities.Characters;
using Game.Messaging.Events.Physics;

using ProtoBuf;

using UnityEngine;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents a bench in bazén w1 zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class WalshesBench : Item
	{
		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, bool passable = false, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
		{
			base.Initialize(name, area, type, decorative, pickable, usable, passable, cutscene: "cs1");
			CreateKeys();
		}

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			if (Used || message.Sender != World.Player)
				return;

			TakeItem newMessage = new(this, _keys);
			World.Player.TakeMessage(newMessage);
			base.OnObjectsUsed(message);
		}

		private void CreateKeys()
		{
			Name name = new("klíče w1", "Walshovy klíče");
			GameObject obj = new(name.Indexed);
			obj.AddComponent<WalshesKeys>();
			_keys = ItemFactory.CreateItem(obj, name, default, "walshovy klíče", pickable: true, usable: false, passable: true);
			World.Add(_keys);
		}

		private Item _keys;
	}
}