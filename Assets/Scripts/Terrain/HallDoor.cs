using DavyKager;

using Game.Audio;
using Game.Entities.Items;
using Game.Messaging;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Commands.UI;
using Game.Messaging.Events.Physics;

using ProtoBuf;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a door in the hall of the Vanilla crunch company (hala v1) zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class HallDoor : Door
	{
		protected override void Open(object sender, Vector2 point)
		{
			base.Open(sender, point);
			Usable = false;
		}

		protected override void Close(object sender, Vector2 point)
		{
			base.Close(sender,point);
			Usable = true;
		}

		private const string _walshesKeysId = "klíče w1";

		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case ObjectsUsed objectsUsed:
					OnObjectsUsed(objectsUsed);
					break;
				default: base.HandleMessage(message); break;
			}
		}

		private void OnObjectsUsed(ObjectsUsed message)
		{
			if (message.UsedObject.Name.Indexed != _walshesKeysId)
				return;

			LockOrUnlock(message.ManipulationPoint);
			World.PlayCutscene(this,"HalldoorUnlock");
		}

		private void LockOrUnlock(Vector2 manipulationPoint)
		{
			if (Locked)
				Unlock();
			else Lock();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the door</param>
		/// <param name="area">Coordinates of the area the door occupies</param>
		/// <param name="zones">The zones connected by the door</param>
		public override void Initialize(Name name, Rectangle area, IEnumerable<string> zones)
		{
			base.Initialize(name, PassageState.Locked, area, zones, usable: true);

			UsableWith = new()
			{
"klíče w1"
			};
			_openingSound = "HallDoorOpening";
			_closingSound = "HallDoorClosing";
		}
	}
}