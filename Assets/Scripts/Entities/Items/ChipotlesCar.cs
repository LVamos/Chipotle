using Game.Audio;
using Game.Entities.Characters;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Terrain;
using Game.UI;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the car of the Detective Chipotle NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChipotlesCar : Item
	{
		/// <summary>
		/// Indicates if the object has moved at least once.
		/// </summary>
		public bool Moved;

		/// <summary>
		/// Returns enumeration of localities where the car can ride.
		/// </summary>
		public IEnumerable<Locality> AllowedDestinations
		{
			get
			{
				_allowedDestinations ??= new();

				return _allowedDestinations.Select(World.GetLocality);
			}
		}

		/// <summary>
		/// Backing field for AllowedDestinations.
		/// </summary>
		protected HashSet<string> _allowedDestinations;

		/// <summary>
		/// List of all localities visited by the object.
		/// </summary>
		protected HashSet<Locality> _visitedLocalities = new();

		/// <summary>
		/// Maps all possible movements among localities.
		/// </summary>
		private readonly Dictionary<string, string> _destinations = new() // locality inner name/rectangle coordinates
		{
			["ulice p1"] = "1810, 1123, 1812, 1119", // at Christine's
			["ulice h1"] = "1539, 1000, 1543, 998", // At the pub
			["ulice s1"] = "1324, 1036, 1328, 1034", // In safe distance from Sweeney's house
			["příjezdová cesta w1"] = "1025, 1036, 1027, 1032", // At a fence
			["ulice v1"] = "2006, 1087, 2008, 1083", // On park place next to Vanilla crunch
			["asfaltka c1"] = "1207, 984, 1209, 980" // At the edge of "cesta c1"
		};

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable)
			=> base.Initialize(name: name, area: area, type: type, decorative: decorative, pickable: pickable,
				usable: usable, stopWhenPlayerMoves: true);

		/// <summary>
		/// Reference to the Detective Chipotle NPC
		/// </summary>
		[ProtoIgnore]
		private Character Player => World.Player;

		/// <summary>
		/// List of all localities visited by the object
		/// </summary>
		public IReadOnlyCollection<Locality> VisitedLocalities => _visitedLocalities;

		/// <summary>
		/// Reference to the Tuttle NPC
		/// </summary>
		[ProtoIgnore]
		private Character _tuttle
			=> World.GetCharacter("tuttle");

		/// <summary>
		/// List of all crutial object in the Walsch's area
		/// </summary>
		private IEnumerable<Item> WalshAreaObjects =>
			(new string[]
				{"tělo w1", "hadice w1", "popelnice w1", "prkno w1", "lavička w1"})
			.Select(o => World.GetItem(o) as Item);

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case MoveChipotlesCar mc: OnMoveChipotlesCar(mc); break;
				case UnblockLocality ul: OnUnblockLocality(ul); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Moves the object to the specified position.
		/// </summary>
		/// <param name="target">Coordinates of the target location</param>
		protected override void Move(Rectangle target)
		{
			base.Move(target);
			ChipotlesCarMoved message = new(this, target);
			Player.TakeMessage(message);
			_tuttle.TakeMessage(message);
		}

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnObjectsUsed(ObjectsUsed message)
		{
			base.OnObjectsUsed(message);

			// When it's not allowed to use the car, play a knocking souund.
			if (
				_localities.Contains("příjezdová cesta w1") && !Moved && !(WalshAreaObjectsUsed() && WalshAreaExplored())
				|| Localities.Any(l => l.Name.Indexed == "asfaltka c1") && !CarsonsBenchesUsed())
			{
				_actionAudio.clip = Sounds.GetClip("snd14");
				_actionAudio.Play();
			}

			// If player didn't leave Walsh area but used required objects and went through all area
			else if (!Moved && WalshAreaObjectsUsed() && WalshAreaExplored())
			{
				AllowDestination(World.GetLocality("ulice p1"));
				DestinationMenu("cs20");
				AllowDestination(World.GetLocality("příjezdová cesta w1"));
			}
			else DestinationMenu(); // Let player seldct destination.
		}

		/// <summary>
		/// Makes the specified locality accessible.
		/// </summary>
		/// <param name="destination">The locality to be allowed</param>
		private void AllowDestination(Locality destination)
		{
			if (!AllowedDestinations.Any(d => d == destination))
				_allowedDestinations.Add(destination.Name.Indexed);
		}

		/// <summary>
		/// Checks if some of the Carsons's bench (lavička c1 or lavička c2) objects was used.
		/// </summary>
		/// <returns>
		/// True if some of the Carsons's bench (lavička c1 or lavička c2) objects was used
		/// </returns>
		private bool CarsonsBenchesUsed()
			=> World.GetItemsByType("lavice u carsona")
				.Any(o => o.Used);

		/// <summary>
		/// Launches a menu from which the player selects the location he wants to go to. Then a
		/// cutscene is played.
		/// </summary>
		/// <param name="preferredCutscene">Name of a cutscene to be played.</param>
		/// <remarks>If the preferredCutscene is'n.t defined a default one is played.</remarks>
		private void DestinationMenu(string preferredCutscene = null)
		{
			if (_allowedDestinations.IsNullOrEmpty())
				throw new InvalidOperationException("No allowed destinations.");

			string cutscene;
			if (string.IsNullOrEmpty(preferredCutscene))
				cutscene = IsChipotleAlone() ? "cs37" : "cs36";
			else cutscene = preferredCutscene;

			Dictionary<string, Locality> destinations = new();
			foreach (string indexedName in _allowedDestinations.Where(d => !_localities.Contains(d)))
			{
				Locality l = World.GetLocality(indexedName);
				destinations[l.Name.Friendly] = l;
			}

			if (destinations.Count == 1)
			{
				Move(destinations.First().Value, cutscene);
				return;
			}

			List<List<string>> items =
				destinations.Keys.ToArray()
					.Select(k => new List<string>() { k })
					.ToList();
			//_allowedDestinations.Select(d => d.Name.Friendly).ToList();
			int item = WindowHandler.Menu(new(items, "Kam chceš jet?"));

			if (item >= 0)
				Move(destinations[items[item][0]], cutscene);
		}

		/// <summary>
		/// Checks if the Detective Chipotle and Tuttle NPC are in the same locality.
		/// </summary>
		/// <returns>True if the Detective Chipotle and Tuttle NPC are in the same locality</returns>
		private bool IsChipotleAlone()
			=> !SameLocality(_tuttle);

		/// <summary>
		/// Moves the object to the specified position and plays a cutscene.
		/// </summary>
		/// <param name="target">Coordinate sof the target location</param>
		/// <param name="cutscene">Name of the cutscene to be played</param>
		private void Move(Rectangle target, string cutscene)
		{
			World.PlayCutscene(this, cutscene);
			Move(target);
			Moved = true;
		}

		/// <summary>
		/// Moves the object to the specified locality.
		/// </summary>
		/// <param name="locality">Inner name of the target locality</param>
		/// <remarks>
		/// The exact target location is taken from the _destinations dictionary. The movement is
		/// allowed only if the specified locality is in the _allowedDestinations hash set.
		/// </remarks>
		/// <completionlist cref="_destinations"/>
		private void Move(Locality locality)
			=> Move(new Rectangle(_destinations[locality.Name.Indexed]));

		/// <summary>
		/// Moves the object to the specified locality.
		/// </summary>
		/// <param name="locality">Inner name of the target locality</param>
		/// <param name="cutscene">Name of a cutscene to be played</param>
		/// <remarks>
		/// The exact target location is taken from the _destinations dictionary. The movement is
		/// allowed only if the specified locality is in the _allowedDestinations hash set. If no
		/// cutscene is defined then a predefined alternative is played.
		/// </remarks>
		private void Move(Locality locality, string cutscene)
			=> Move(new Rectangle(_destinations[locality.Name.Indexed]), cutscene);

		/// <summary>
		/// Processes the MoveChipotlesCar message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnMoveChipotlesCar(MoveChipotlesCar message)
			=> Move(message.Destination);

		/// <summary>
		/// Processes the UnblockLocality message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnUnblockLocality(UnblockLocality message)
		{
			if (!_allowedDestinations.Contains(message.Locality.Name.Indexed))
				_allowedDestinations.Add(message.Locality.Name.Indexed);
		}

		/// <summary>
		/// Checks if all the Walsch's area was explored.
		/// </summary>
		/// <returns>True if all the Walsch's area was explored</returns>
		private bool WalshAreaExplored()
			=> Player.VisitedLocalities.Count() == 14
			   && Player.VisitedLocalities.All(l => l.Name.Indexed.ToLower().Contains("w1"));

		/// <summary>
		/// Checks if all crutial objects in Walsch's area were used.
		/// </summary>
		/// <returns>True if all crutial objects in Walsch's area were used</returns>
		private bool WalshAreaObjectsUsed()
			=> WalshAreaObjects.All(o => o.Used);
	}
}