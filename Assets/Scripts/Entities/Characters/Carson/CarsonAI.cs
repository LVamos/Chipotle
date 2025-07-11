﻿using Game.Entities.Characters.Components;
using Game.Entities.Items;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Movement;
using Game.Terrain;

using ProtoBuf;

using System.Linq;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Carson
{
	/// <summary>
	/// Controls the behavior of the Carson NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CarsonAI : AI
	{
		/// <summary>
		/// Indicates if the Carson NPC said goodbye to the Detective Chipotle NPC when Chipotle
		/// left the zahrada c1 zone.
		/// </summary>
		private bool _saidGoodbyeToChipotle;

		/// <summary>
		/// Indicates if the Carson NPC scolded the Detective Chipotle NPC the first time Chipotle
		/// came to the zahrada c1 zone.
		/// </summary>
		private bool _yelledAtChipotle;

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();

			JumpNear(World.GetItem("stůl c1").Area.Value); // At a table in his garden
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CharacterLeftZone ll: OnZoneLeft(ll); break;
				case CharacterCameToZone le: OnZoneEntered(le); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();

			if (!_messagingEnabled)
				return;

			WatchChipotlesCar();
		}

		/// <summary>
		/// Processes the ZoneEntered message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnZoneEntered(CharacterCameToZone message)
		{
			if (message.Character != World.Player || message.CurrentZone != Owner.Zone)
				return;

			// This happens just once.
			if (!_yelledAtChipotle)
			{
				_yelledAtChipotle = true;
				World.PlayCutscene(Owner, "cs34");
			}
		}

		/// <summary>
		/// Processes the ZoneLeft message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnZoneLeft(CharacterLeftZone message)
		{
			if (message.LeftZone != Owner.Zone || message.Character != World.Player)
				return;

			bool benchUsed = World.GetItemsByType("lavice u carsona").Any(o => o.Used);
			if (benchUsed && !_saidGoodbyeToChipotle)
			{
				_saidGoodbyeToChipotle = true;
				World.PlayCutscene(Owner, "cs33");
			}
		}

		/// <summary>
		/// checks if the Detective's car object went away from the asfaltka c1 zone after
		/// saying goodbye.
		/// </summary>
		/// <remarks>The method is called from the GameUpdate method.</remarks>
		private void WatchChipotlesCar()
		{
			Zone road = World.GetZone("asfaltka c1");
			Item car = World.GetItem("detektivovo auto");
			if (_saidGoodbyeToChipotle && !road.IsItHere(car)) // Chipotle left the area
			{
				_messagingEnabled = false;
				InnerMessage(new DestroyObject(this));
			}
		}
	}
}