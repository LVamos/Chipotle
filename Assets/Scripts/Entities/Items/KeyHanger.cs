using Game.Messaging.Commands.Physics;

using ProtoBuf;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the key hanger object in the garage in Vanilla crunch company (garáž v1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class KeyHanger : Item
	{

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
			string usedObject = message.UsedObject?.Name.Indexed;
			string target = message.Target?.Name.Indexed;

			if (KeysHanging && usedObject == Name.Indexed && target == null)
			{
				_cutscene = "TakeKeysFromHanger";
				KeysHanging = false;
				Usable = false;
			}
			else if (!KeysHanging && usedObject == "klíče v1" && target == Name.Indexed)
			{
				KeysHanging = true;
				_cutscene = null;
				Usable = true;
			}

			base.OnUseObjects(message);
		}
	}
}