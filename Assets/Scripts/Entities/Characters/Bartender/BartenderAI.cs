using Game.Entities.Characters.Components;
using Game.Entities.Items;
using Game.Messaging.Events.Movement;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;
using System.Linq;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Bartender
{
	/// <summary>
	/// Controls the behavior of Bartender NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class BartenderAI : AI
	{
		private readonly Locality BonitaStreet = World.GetLocality("ulice h1");

		/// <summary>
		/// Determines whether the bartender NPC should say goodbye to the detective Chipotle NPC
		/// when the detective leaves.
		/// </summary>
		private bool _sayGoodbyeToChipotle;

		/// <summary>
		/// Determines whether the bartender NPC should say hello to the Detective Chipotle NPC when
		/// the detective comes to the pub.
		/// </summary>
		private bool _velcomeChipotle = true;

		/// <summary>
		/// Indicates whether the Detective Chipotle NPC has entered the pub more than once since
		/// arriving by car.
		/// </summary>
		private bool _wasChipotleHere;

		/// <summary>
		/// Returns a reference to the Chipotle's car object.
		/// </summary>
		private ChipotlesCar ChipotlesCar
			=> World.GetItem("detektivovo auto") as ChipotlesCar;

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			JumpTo(World.GetItem("pult h2").Area.Value); // At the bar in the pub
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CharacterCameToLocality le: OnLocalityEntered(le); break;
				case CharacterMoved em: OnEntityMoved(em); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();

			WatchChipotlesCar();
		}

		/// <summary>
		/// Checks if Tuttle NPC is in the same locality as Detective Chipotle NPC.
		/// </summary>
		/// <returns>True if Tuttle NPC is in the same locality as Detective Chipotle NPC</returns>
		private bool IsChipotleAlone()
		{
			Character tuttle = World.GetCharacter("tuttle");
			IEnumerable<Locality> nearPub = World.GetLocalitiesInArea("h");

			return !nearPub.Any(l => l.IsItHere(tuttle));
		}

		/// <summary>
		/// Checks if the Detective's car object is in Bonita street (ulice h1) locality.
		/// </summary>
		/// <returns>True if the Detective's car object is in Bonita street (ulice h1) locality</returns>
		private bool IsChipotlesCarNearBy()
		{
			return BonitaStreet.IsItHere(ChipotlesCar);
		}

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnEntityMoved(CharacterMoved message)
		{
			if (message.Sender != World.Player)
				return;

			Character player = World.Player;
			Passage door = World.GetPassage("duvh1");
			bool tableUsed = World.GetItemsByType("hospodský stůl").Any(o => o.Used);

			if (
				_sayGoodbyeToChipotle
				&& tableUsed
				&& player.SameLocality(Owner)
				&& door.Area.Value.GetDistanceFrom(player.Area.Value.Center) == 1
			)
			{
				World.PlayCutscene(Owner, IsChipotleAlone() ? "cs28" : "cs29");
				_sayGoodbyeToChipotle = false;
			}
		}

		/// <summary>
		/// Processes the LocalityEntered message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnLocalityEntered(CharacterCameToLocality message)
		{
			if (message.Character != World.Player || message.CurrentLocality != Owner.Locality)
				return;

			if (_velcomeChipotle)
			{
				World.PlayCutscene(Owner, IsChipotleAlone() ? "cs30" : "cs31");
				_velcomeChipotle = false;
				_sayGoodbyeToChipotle = true;
			}
		}

		/// <summary>
		/// checks if the Detective's car object is in Bonita street (ulice h1) locality.
		/// </summary>
		/// <remarks>The method is called from the GameUpdate method.</remarks>
		private void WatchChipotlesCar()
		{
			bool isCarNearBy = IsChipotlesCarNearBy();
			if (isCarNearBy != _wasChipotleHere)
			{
				_velcomeChipotle = isCarNearBy;
				_wasChipotleHere = isCarNearBy;
			}
		}
	}
}