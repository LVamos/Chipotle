using Game.Mapping.Saves;
using Game.Messaging.Commands.Physics;
using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;



namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the key hanger object in the garage in Vanilla crunch company (garáž v1) zone.
	/// </summary>

	public class KeyHanger : Item
	{
		public override void Restore(ItemSave save)
		{
			if (save is not KeyHangerSave data)
				return;

			Initialize
	(
							data.Name.ToName(),
							data.Area.ToRectangle(),
							data.Type,
							data.Decorative,
							data.Pickable,
							data.Usable,
							data.Passable,
							data.CollisionSound,
							data.ActionSound,
							data.LoopSound,
							data.Cutscene,
			data.UsableOnce,
			data.AudibleOverWalls,
			_defaultVolume,
			data.StopWhenPlayerMoves,
			data.QuickActionsAllowed,
			data.PickingSound,
			data.PlacingSound,
			data.UsableWith
							);
			KeysHanging = data.KeysHanging;
			_descriptionID = data.DescriptionID;
		}

		public override ItemSave Export()
		{
			var save = base.Export().ToKeyHangerSave();

			save.KeysHanging = KeysHanging;

			return save;
		}

		/// <summary>
		/// Indicates if the keys are on the hanger.
		/// </summary>
		public bool KeysHanging { get; private set; } = true;

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnUseObjects(UseObjects message)
		{
			string usedObject = message.UsedObject?.Name.Inner;
			string target = message.Target?.Name.Inner;

			// Taking the keys
			if (KeysHanging && usedObject == Name.Inner && target == null)
			{
				_cutscene = "TakeKeysFromHanger";
				KeysHanging = false;
				Usable = false;
			}
			else if (!KeysHanging && usedObject == "klíče v1" && target == Name.Inner)
			{
				KeysHanging = true;
				_cutscene = null;
				Usable = true;
			}

			base.OnUseObjects(message);
		}
	}
}