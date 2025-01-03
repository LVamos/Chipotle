﻿using Assets.Scripts.Messaging.Events.Characters;

using DavyKager;

using Game.Entities.Items;
using Game.Messaging.Commands;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.GameManagement;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Commands.UI;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.GameInfo;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Messaging.Events.Sound;
using Game.Models;
using Game.Terrain;
using Game.UI;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;
using Physics = Game.Entities.Characters.Components.Physics;
using Random = System.Random;

namespace Game.Entities.Characters.Chipotle
{
	/// <summary>
	/// Controls movement of the Detective Chipotle NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChipotlePhysics : Physics
	{
		public void OnSayItemSize(SayItemSize message)
		{
			Item item = World.GetNearestObjects(_area.Value.Center).FirstOrDefault();
			if (item != null)
				InnerMessage(new SaySize(this, item.Area.Value));
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected void OnExploreObject(ExploreObject m)
		{
			if (m.Object != null)
			{
				InnerMessage(new SayObjectDescription(this, m.Object));
				return; // This should be handled by the sound component.
			}

			// Check if any items or characters are standing before the player.
			IEnumerable<Entity> objects = GetItemsAndCharactersBefore(_objectManipulationRadius);
			if (objects.IsNullOrEmpty())
			{
				InnerMessage(new SayObjectDescription(this, null)); // The sound component will announce that nothing is standing before the player.
				return;
			}

			if (objects.Count() == 1)
			{
				Entity obj = objects.First();
				obj.TakeMessage(new ObjectResearched(Owner)); // Announce the object or character that it was researched.
				InnerMessage(new SayObjectDescription(this, obj)); // A command for sound component.be spoken.
				return;
			}

			// More objects awailable. Run the menu to select an object to be explored.
			WindowHandler.ActiveWindow.TakeMessage(new SelectObjectToExplore(Owner, objects.ToList()));
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">Source of the message</param>
		protected void OnPickUpObjectResult(PickUpObjectResult m)
		{
			if (m.Result == PickUpObjectResult.ResultType.Success)
				_inventory.Add(m.Object.Name.Indexed);

		}

		/// <summary>
		/// Determines how many items this entity can carry.
		/// </summary>
		protected const int _inventoryLimit = 5;

		/// <summary>
		/// Represents the inventory. The objects are stored as indexed names.
		/// </summary>
		protected HashSet<string> _inventory = new();

		protected override Vector2 GetStepDirection()
		{
			Orientation2D finalOrientation = _orientation;
			if (_startWalkMessage != null && _startWalkMessage.Direction != TurnType.None)
				finalOrientation.Rotate(_startWalkMessage.Direction);
			return finalOrientation.UnitVector;
		}

		/// <summary>
		/// Immediately changes position of the NPC.
		/// </summary>
		/// <param name="target">Coordinates of the target position</param>
		/// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
		protected override void JumpTo(Vector2 target, bool silently = false)
		{
			base.JumpTo(target, silently);
			RecordRegion(target); // Record visited region

			// Watch important events
			WatchPuddle(target); // Check if player walked in a puddle
			WatchPhone();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// set initial position.
			Width = .4f;
			Height = .4f;

			if (Settings.TestChipotleStartPosition.HasValue)
			{
				Vector2 position = Settings.TestChipotleStartPosition.Value;
				StartPosition = new(
					position,
					new(position.x + Width, position.y - Height)
				);
			}
			else
				StartPosition = new(new(1028, 1034), new(1028.5f, 1033.5f));
		}

		/// <summary>
		/// Specifies if the NPC can walk.
		/// </summary>
		[ProtoIgnore]
		protected bool _blockWalk;

		/// <summary>
		/// Time interval for backward walk
		/// </summary>
		private const float _backwardSpeed = 1.3f;

		/// <summary>
		/// Time interval for forward walk
		/// </summary>
		private const int _forwardWalkSpeed = 15;

		/// <summary>
		/// Time interval for walk to the side
		/// </summary>
		private const float _sideSpeed = 1.2f;

		/// <summary>
		/// Stores references to all the localities the NPC has visited.
		/// </summary>
		private readonly HashSet<Locality> _visitedLocalities = new();

		/// <summary>
		/// A delayed message of ChipotlesCarMoved type
		/// </summary>
		private ChipotlesCarMoved _carMovement;

		/// <summary>
		/// Indicates the ongoing countdown of the phone call cutscene that should be played after
		/// the Detective Chipotle and Tuttle NPCs leave the Christine's bed room (ložnice p1) locality.
		/// </summary>
		private bool _phoneCountdown;

		/// <summary>
		/// The time that has elapsed since the phone call cutscene countdown started.
		/// </summary>
		private int _phoneDeltaTime;

		/// <summary>
		/// Specifies how long the phone call cutscene countdown should last.
		/// </summary>
		private int _phoneInterval;

		/// <summary>
		/// Indicates if the NPC is sitting at a table in the pub (výčep h1) locality.
		/// </summary>
		private bool _sittingAtPubTable;

		/// <summary>
		/// Indicates if the NPC is sitting on a chair.
		/// </summary>
		private bool _sittingOnChair;

		/// <summary>
		/// stores information about walk.
		/// </summary>
		[ProtoIgnore]
		private StartWalk _startWalkMessage;

		/// <summary>
		/// Indicates if the NPC stepped into the puddle in the pool (bazén w1) locality.
		/// </summary>
		private bool _steppedIntoPuddle;

		/// <summary>
		/// Indicates if the Chipotle NPC is currently walking.
		/// </summary>
		[ProtoIgnore]
		private bool _walking;

		/// <summary>
		/// Returns reference to the asphalt road (asfaltka c1) locality.
		/// </summary>
		private Locality AsphaltRoad
			=> World.GetLocality("asfaltka c1");

		/// <summary>
		/// Returns a reference to the Chipotle's car object.
		/// </summary>
		private ChipotlesCar Car
			=> World.GetItem("detektivovo auto") as ChipotlesCar;

		/// <summary>
		/// Returns reference to the Tuttle NPC.
		/// </summary>
		[ProtoIgnore]
		private Character Tuttle
			=> World.GetCharacter("tuttle");

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			_orientation = new(0, 1);
			_orientation.Chipotle = true;
			OrientationChanged message = new(this, _orientation, _orientation, TurnType.None, false, true);
			InnerMessage(message);
			JumpTo(StartPosition.Value, true);
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			base.HandleMessage(message);

			switch (message)
			{
				case ApplyItemToTarget m:
					OnApplyItemToTarget(m); break;
				case SayCharacters m: OnSayCharacters(m); break;
				case ListCharacters m: OnListCharacters(m); break;
				case SayItemSize m: OnSayItemSize(m); break;
				case ExploreObject m: OnExploreObject(m); break;
				case PickUpObjectResult m: OnPickUpObjectResult(m); break;
				case RunInventoryMenu m: OnRunInventoryMenu(m); break;
				case PickUpObject m: OnPickUpObject(m); break;
				case CreatePredefinedSave c: OnCreatePredefinedSave(c); break;
				case LoadPredefinedSave l: OnLoadPredefinedSave(l); break;
				case ListExits lex: OnListExits(lex); break;
				case NavigationStopped m: OnObjectNavigationStopped(m); break;
				case StopNavigation m: OnStopNavigation(m); break;
				case ListObjects lob: OnListObjects(lob); break;
				case SayExits sex: OnSayExits(sex); break;
				case StopWalk sw: OnStopWalk(sw); break;
				case SayTerrain ste: OnSayTerrain(ste); break;
				case SayVisitedRegion sv: OnSayVisitedLocality(sv); break;
				case ChipotlesCarMoved ccm: OnChipotlesCarMoved(ccm); break;
				case CutsceneEnded ce: OnCutsceneEnded(ce); break;
				case CutsceneBegan cb: OnCutsceneBegan(cb); break;
				case SayObjects sob: OnSayObjects(sob); break;
				case StartWalk staw: OnStartWalk(staw); break;
				case ChangeOrientation cor: OnChangeOrientation(cor); break;
				case Interact m: OnInteract(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles a message
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnApplyItemToTarget(ApplyItemToTarget message)
		{
			if (message.Target != null)
			{
				ApplyItemToTarget(message.ItemToUse, message.Target);
				return;
			}

			UsableObjectsModel objects = GetUsableItemsAndCharactersBefore(_objectManipulationRadius);
			if (objects.Result == UsableObjectsModel.ResultType.NothingFound)
				InnerMessage(new InteractResult(this, InteractResult.ResultType.NoObjects));
			else if (objects.Result == UsableObjectsModel.ResultType.Unusable)
				InnerMessage(new InteractResult(this, InteractResult.ResultType.NoUsableObjects));
			else if (objects.Result == UsableObjectsModel.ResultType.Far)
				InnerMessage(new InteractResult(this, InteractResult.ResultType.Far));
			else // Success
			{
				if (objects.Objects.Count() == 1)
					ApplyItemToTarget(message.ItemToUse, objects.Objects[0]);
				else if (objects.Objects.Any())
					WindowHandler.ActiveWindow.TakeMessage(new SelectObjectToApply(Owner, message.ItemToUse, objects.Objects));
			}
		}

		/// <summary>
		/// Applies the given item to the target item or character.
		/// </summary>
		/// <param name="itemToUse">The item to be applied to the target</param>
		/// <param name="target">The target item or character</param>
		private void ApplyItemToTarget(Item itemToUse, Entity target)
		{
			Vector2 manipulationPoint = FindManipulationPoint(target);
			var message = new ObjectsUsed(Owner, manipulationPoint, itemToUse, target);
			target.TakeMessage(message);
		}

		/// <summary>
		/// Handles a message
		/// </summary>
		/// <param name="message">The incoming message</param>
		private void OnListCharacters(ListCharacters message)
		{
			if (_walking)
			{
				SayCharacters();
				return;
			}

			StopNavigation(); // If there's any navigation in progress, it'll be stopped and this command will be cancelled.
			if (NavigationInProgress)
				return;

			NavigableCharactersModel characters = GetNavigableCharacters();

			if (characters.Characters.IsNullOrEmpty())
			{
				InnerMessage(new SayCharactersResult(this, null));
				return;
			}

			List<List<string>> descriptions =
				characters.Descriptions
					.Select(d => new List<string> { d }).ToList();

			MenuParametersDTO parameters = new(descriptions, "Okolní postavy", " ", 0, false);
			int option = WindowHandler.Menu(parameters);


			if (option == -1)
				return;

			Character target = characters.Characters[option];

			_navigatedObject = target;
			_navigatedObject.TakeMessage(new StartNavigation(Owner));
		}

		private NavigableCharactersModel GetNavigableCharacters()
		{
			IEnumerable<Character> characters = Locality.GetNearByCharacters(
				_area.Value.Center,
				_navigableObjectsRadius,
				Owner
			);
			List<string> descriptions = new();

			foreach (Character character in characters)
			{
				string name = character.Name.Friendly;
				float distance = World.GetDistance(Owner, character);
				string distanceDescription = GetDistanceDescription(distance);
				float compassDegrees = GetAngle(character);
				string angleDescription = Angle.GetDescription(compassDegrees);

				descriptions.Add($"{name} {distanceDescription} {angleDescription} ");
			}

			var result = new NavigableCharactersModel(descriptions.ToArray(), characters.ToArray());
			return result;
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnRunInventoryMenu(RunInventoryMenu message)
		{
			// Check if inventory isn't empty.
			if (Owner.Inventory.IsNullOrEmpty())
			{
				InnerMessage(new EmptyInventory(this));
				return;
			}

			// Run the menu

			List<Item> items =
				(from itemName in _inventory
				 let item = World.GetItem(itemName)
				 select item)
				.ToList();

			var newMessage = new selectInventoryAction(Owner, items);
			WindowHandler.ActiveWindow.TakeMessage(newMessage);
		}

		/// <summary>
		/// Handles the PickUpObject message.
		/// </summary>
		/// <param name="message">The message to be processed.
		protected void OnPickUpObject(PickUpObject message)
		{
			if (message.Object != null)
			{
				TryPickItem(message.Object);
				return;
			}

			PickableItemsModel items = GetPickableItemsBefore(_objectManipulationRadius);
			if (items.Result == PickableItemsModel.ResultType.NothingFound)
				InnerMessage(new PickUpObjectResult(this));
			else if (items.Result == PickableItemsModel.ResultType.Unpickable)
			{
				var newMessage = new PickUpObjectResult(this, null, PickUpObjectResult.ResultType.Unpickable);
				InnerMessage(newMessage);
			}
			else if (items.Result == PickableItemsModel.ResultType.Unreachable)
			{
				var newMessage = new PickUpObjectResult(this, null, PickUpObjectResult.ResultType.Unreachable);
				InnerMessage(newMessage);
			}

			else if (items.Items.Count() == 1)
			{
				TryPickItem(items.Items[0]);
			}
			else // More items awailable.
				WindowHandler.ActiveWindow.TakeMessage(new SelectItemToPick(Owner, items.Items));
		}

		/// <summary>
		/// Tries to pick an item.
		/// </summary>
		private void TryPickItem(Item item)
		{
			if (!item.CanBePicked())
				InnerMessage(new PickUpObjectResult(this, null, PickUpObjectResult.ResultType.Unpickable));
			else if (_inventory.Count >= _inventoryLimit)
				InnerMessage(new PickUpObjectResult(this, null, PickUpObjectResult.ResultType.FullInventory));
			else item.TakeMessage(new PickUpObject(Owner, item));
		}

		/// <summary>
		/// Handles the CreatePredefinedSave message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnCreatePredefinedSave(CreatePredefinedSave message)
			=> World.CreatePredefinedSave();

		/// <summary>
		/// Handles the LoadPredefinedSave message.
		/// </summary>
		/// <param name="message"></param>
		private void OnLoadPredefinedSave(LoadPredefinedSave message)
		{
			StopNavigation();
			World.LoadPredefinedSave();
		}

		/// <summary>
		/// Processes the ListExits message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnListExits(ListExits message)
		{
			if (_walking)
			{
				SayExits();
				return;
			}

			if (NavigationInProgress)
			{
				StopNavigation(); // If there's any navigation in progress,  it'll be stopped and this command will be cancelled.
				return;
			}

			(List<List<string>> descriptions, Passage[] exits) result = GetNavigableExits();

			if (result.descriptions.IsNullOrEmpty()) // No objects near by
			{
				InnerMessage(new SayExitsResult(this));
				return;
			}

			// Run the menu
			MenuParametersDTO parameters = new(
	items: result.descriptions,
	introText: "Východy",
	divider: " ",
	searchIndex: 2,
	wrappingAllowed: false,
	menuClosed: (option) =>
	{
		if (option == -1)
			return;

		_navigatedExit = result.exits[option];
		_navigatedExit.TakeMessage(new StartNavigation(Owner));
	}
);
			WindowHandler.Menu(parameters);
		}

		/// <summary>
		/// An exit to which the NPC is navigated.
		/// </summary>
		[ProtoIgnore]
		protected Passage _navigatedExit;
		/// <summary>
		/// Processes the ObjectNavigationStopped message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnObjectNavigationStopped(NavigationStopped message)
		{
			if (message.Sender == _navigatedObject)
				_navigatedObject = null;
			else if (message.Sender == _navigatedExit)
				_navigatedExit = null;
		}

		/// <summary>
		/// Processes the StopObjectNavigation message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnStopNavigation(StopNavigation message)
			=> StopNavigation();

		/// <summary>
		/// Processes the ListNavigableObjects message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnListObjects(ListObjects message)
		{
			if (_walking)
			{
				SayObjects();
				return;
			}

			StopNavigation(); // If there's any navigation in progress, it'll be stopped and this command will be cancelled.
			if (NavigationInProgress)
				return;

			NavigableItemsModel objects = GetNavigableItems();

			if (objects.Items.IsNullOrEmpty())
			{
				InnerMessage(new SayObjectsResult(this, null));
				return;
			}

			List<List<string>> descriptions =
				objects.Descriptions
					.Select(d => new List<string> { d }).ToList();

			MenuParametersDTO parameters = new(
				items: descriptions,
				introText: "Okolní objekty",
				wrappingAllowed: false,
				menuClosed: (index) =>
				{
					if (index == -1)
						return;

					Item target = objects.Items[index];

					_navigatedObject = target;
					_navigatedObject.TakeMessage(new StartNavigation(Owner));
				});
			int option = WindowHandler.Menu(parameters);
		}

		/// <summary>
		/// Stops ongoing object navigation.
		/// </summary>
		private void StopNavigation()
		{
			_navigatedObject?.TakeMessage(new StopNavigation(Owner));
			_navigatedExit?.TakeMessage(new StopNavigation(Owner));
		}

		/// <summary>
		/// Checks if there's any navigation in progress.
		/// </summary>
		protected bool NavigationInProgress
			=> _navigatedExit != null || _navigatedObject != null;

		/// <summary>
		/// Objectt to which tthe NPC is currently navigated.
		/// </summary>
		[ProtoIgnore]
		protected Entity _navigatedObject;

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();
			CountPhone();
		}

		/// <summary>
		/// Processes the CutsceneBegan message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnCutsceneBegan(CutsceneBegan message)
		{
			StopWalk();
			StopNavigation();
			CatchSitting(message);
		}

		protected void SaveGame()
		{
			int temp = _currentRegion;
			bool temp2 = _inVisitedRegion;
			_currentRegion = -1;
			_inVisitedRegion = true;

			World.SaveGame();

			_currentRegion = temp;
			_inVisitedRegion = temp2;
		}

		/// <summary>
		/// Processes the CutsceneEnded message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnCutsceneEnded(CutsceneEnded message)
		{
			base.OnCutsceneEnded(message);
			WatchCar();

			switch (message.CutsceneName)
			{
				case "cs7": case "cs10": PlayFinalScene(); break;
				case "cs11": WatchIcecreamMachine(); JumpToMariottisOffice(); break;
				case "cs12": JumpToVanillaCrunchGarage(); break;
				case "cs14": JumpToBelvedereStreet2(); break;
				case "cs15":
				case "cs16":
				case "cs17":
				case "cs18": WatchSweeneysRoom(); break;
				case "cs21": JumpToChristinesHall(); break;
				case "cs23": JumpToSweeneysHall(); break;
				case "cs35": QuitGame(); break;
			}
		}

		/// <summary>
		/// Processes the SayExits message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnSayExits(SayExits message)
			=> SayExits();

		/// <summary>
		/// Announces the nearest exits from the locality in which the Chipotle NPC is located.
		/// </summary>
		private void SayExits()
		{
			// If there's any navigation in progress; break; it'll be stopped and this command will be cancelled.
			StopNavigation();
			if (NavigationInProgress)
				return;

			// If the player stands in a passage inform him.
			Passage occupiedPassage = World.GetPassage(_area.Value.Center);
			if (occupiedPassage != null)
				InnerMessage(new SayExitsResult(this, occupiedPassage));
			else InnerMessage(new SayExitsResult(this, GetNavigableExits().descriptions));
		}

		/// <summary>
		/// Generates a text representation of the specified distance in Czech.
		/// </summary>
		/// <param name="distance">The distance in meters to be described</param>
		/// <returns>The text representation of the specified distance</returns>
		private string GetDistanceDescription(float distance)
		{
			if (distance <= _stepLength)
				return string.Empty;

			// Round the distance so that its value corresponds to a multiple of 0.5.
			float roundedDistance = (float)(Math.Round(distance * 2, MidpointRounding.AwayFromZero) / 2);
			int meters = (int)roundedDistance;
			float centimeters = roundedDistance - meters;

			// Compose output
			if (roundedDistance == .5f)
				return " půl metru ";
			if (roundedDistance == 1)
				return " metr ";
			if (centimeters == 0 && roundedDistance >= 2 && roundedDistance <= 4)
				return $" {meters} metry ";
			if (roundedDistance == 1.5f)
				return " metr a půl ";
			if (centimeters == .5f)
				return $" {meters} a půl metrů ";
			return $" {meters} metrů ";
		}

		/// <summary>
		/// Returns text descriptions of the specified exits including distance and position.
		/// </summary>
		/// <returns>A string array</returns>
		private (List<List<string>> descriptions, Passage[] exits) GetNavigableExits()
		{
			IEnumerable<Passage> exits =
				Locality.GetNearestExits(
					_area.Value.Center,
					_exitRadius);
			List<List<string>> descriptions =
			(
				from e in exits
				let distance = World.GetDistance(Owner, e)
				let distanceDescription = GetDistanceDescription(distance)
				let type = e.ToString()
				let to = e.AnotherLocality(Locality).To + " "
				let index = to.IndexOf(' ')
				let to1 = to.Substring(0, index)
				let to2 = to.Substring(index + 1)
				let angle = GetAngle(e)
				let angleDescription = Angle.GetDescription(angle)
				select new List<string> { type, to1, to2, distanceDescription, angleDescription }
			).ToList();

			return (descriptions, exits.ToArray());
		}

		/// <summary>
		/// Checks if the NPC sits and sets appropriate fields.
		/// </summary>
		/// <param name="message"></param>
		private void CatchSitting(CutsceneBegan message)
		{
			switch (message.CutsceneName)
			{
				case "cs24": case "cs25": _sittingAtPubTable = true; break;
				case "snd12": _sittingOnChair = true; break;
			}
		}

		/// <summary>
		/// Measures time for the phone call countdown.
		/// </summary>
		private void CountPhone()
		{
			if (_phoneCountdown)
				_phoneDeltaTime += World.DeltaTime;
		}

		/// <summary>
		/// Checks if Tuttle and Chipotle NPCs are in the same locality.
		/// </summary>
		/// <returns>True if Tuttle and Chipotle NPCs are in the same locality</returns>
		private bool IsTuttleNearBy()
			=> Owner.SameLocality(Tuttle);

		/// <summary>
		/// Chipotle and Tuttle NPCs relocate from the Walsch's drive way (příjezdoivá cesta w1)
		/// locality right outside the Christine's door.Christine's front door.
		/// </summary>
		private void JumpToBelvedereStreet2()
		{
			_phoneCountdown = true;
			Random r = new();
			_phoneInterval = r.Next(30000, 120000);
			_phoneDeltaTime = 0;
			JumpTo(1813, 1126, true);
		}

		/// <summary>
		/// The Detective Chipotle and Tuttle NPCs relocate from the Belvedere street (ulice p1)
		/// locality to the Christine's hall (hala p1) locality.
		/// </summary>
		private void JumpToChristinesHall()
		{
			JumpTo(1797, 1125, true);
			World.PlayCutscene(Owner, "cs38");
		}

		/// <summary>
		/// Relocates the NPC from the hall in Vanilla crunch company (hala v1) into the Mariotti's
		/// office (kancelář v1) locality.
		/// </summary>
		private void JumpToMariottisOffice()
			=> JumpTo(2018, 1123, true);

		/// <summary>
		/// Chipotle and Tuttle NPCs relocate from the Easterby street (ulice p1) locality to the
		/// Sweeney's hall (hala s1) locality.
		/// </summary>
		private void JumpToSweeneysHall()
		{
			JumpTo(1405, 965, true);
			World.PlayCutscene(Owner, "cs41");
		}

		/// <summary>
		/// Relocates the NPC from the Mariotti's office (kancelář v1) into the garage of the
		/// vanilla crunch company (garáž v1) locality.
		/// </summary>
		private void JumpToVanillaCrunchGarage()
			=> JumpTo(2006, 1166, true);

		/// <summary>
		/// Stores indexes of regions visited by the NPC.
		/// </summary>
		/// <remarks>Each locality is divided into regions of same size specified by the _motionTrackRadius field. Index of a region define distance from the top left region of the current locality (MotionTrackWidth *row +column). Last row and last column can be smaller.</remarks>
		protected readonly Dictionary<Locality, HashSet<int>> _motionTrack = new();

		/// <summary>
		/// Determines the maximum distance from the pool at which stepping into a puddle will trigger a cutscene.
		/// </summary>
		protected const float _puddleRadius = 10;

		/// <summary>
		/// Computes amount of horizontal motion track regions for the current locality.
		/// </summary>
		protected int MotionTrackWidth => (int)(Locality.Area.Value.Width / _motionTrackRadius + (Locality.Area.Value.Width % _motionTrackRadius > 0 ? 1 : 0));

		/// <summary>
		/// Computes amount of vertical motion track regions for the current locality.
		/// </summary>
		protected int MotionTrackHeight => (int)(Locality.Area.Value.Height / _motionTrackRadius + (Locality.Area.Value.Height % _motionTrackRadius > 0 ? 1 : 0));

		protected int GetRegionIndex(Vector2 point)
		{
			Vector2 relative = Terrain.Rectangle.GetRelativeCoordinates(point);

			int rX = (int)(relative.x / _motionTrackRadius + (relative.x % _motionTrackRadius > 0 ? 1 : 0));
			int rY = (int)(relative.y / _motionTrackRadius + (relative.y % _motionTrackRadius > 0 ? 1 : 0));
			return rX + rY;
		}

		/// <summary>
		/// Indicates if the NPC stands in a previously visited region.
		/// </summary>
		protected bool _inVisitedRegion;

		/// <summary>
		/// Specifies radius of surroundings around the NPC in steps used in motion tracking.
		/// </summary>
		protected const int _motionTrackRadius = 10;

		/// <summary>
		/// Stores index of current motion track region.
		/// </summary>
		protected int _currentRegion;

		/// <summary>
		/// Specifies distance in which exits from current locality are searched.
		/// </summary>
		protected const int _exitRadius = 50;

		/// <summary>
		/// Marks current nearest surroundings of the NPC as visited. If it's already been visited then the _inVisitedRegion is set to true.
		/// </summary>
		/// <param name="point">Current position of the NPC</param>
		protected void RecordRegion(Vector2 point)
		{
			if (!_motionTrack.ContainsKey(Locality))
				_motionTrack[Locality] = new();

			int regionIndex = GetRegionIndex(point);

			if (_currentRegion != regionIndex)
			{
				_currentRegion = regionIndex;
				_inVisitedRegion = _motionTrack[Locality].Contains(regionIndex);

				if (!_inVisitedRegion)
					_motionTrack[Locality].Add(regionIndex);
			}
		}

		/// <summary>
		/// Performs one step in direction specified in the _startWalkMessage field.
		/// </summary>
		protected override void MakeStep()
		{
			// Speed limitation
			if (_walkTimer < _speed)
				return;

			base.MakeStep();

			// Move if the terrain is occupable.
			Vector2 direction = GetStepDirection();

			if (!DetectCollisions(direction))
				Move(direction);
			//ReportObjects();
		}

		/// <summary>
		/// Handles collisions between the NPC and other elements.
		/// </summary>
		private void HandleCollisions(List<object> elements)
		{
			// Check for door collisions first
			if (HandleDoorCollision(elements))
				return;

			// Handle collisions with items and characters
			HandleItemsAndCharactersCollisions(elements);

			// Handle collisions with inaccessible terrain
			HandleTerrainCollisions(elements);
		}

		private bool HandleDoorCollision(List<object> elements)
		{
			Door door = elements.OfType<Door>().FirstOrDefault();
			if (door != null)
			{
				Vector2 contactPoint = door.Area.Value.GetClosestPoint(_area.Value.Center);
				var doorHitMessage = new DoorHit(Owner, door, contactPoint);
				door.TakeMessage(doorHitMessage);
				InnerMessage(doorHitMessage);
				return true;
			}
			return false;
		}

		private void HandleItemsAndCharactersCollisions(List<object> elements)
		{
			IEnumerable<IGrouping<string, Entity>> groupedElements = elements
				.OfType<Entity>()
				.GroupBy(o => o.Name.Friendly);

			foreach (IGrouping<string, Entity> group in groupedElements)
			{
				Entity closestElement = group.OrderBy(GetDistance)
					.First();

				Vector2 contactPoint = GetContactPoint(closestElement);
				var collisionMessage = new ObjectsCollided(Owner, closestElement, contactPoint);
				closestElement.TakeMessage(collisionMessage);
				InnerMessage(collisionMessage);
			}

			float GetDistance(MapElement element)
				=> World.GetDistance(GetContactPoint(element), _area.Value.Center);
		}

		private void HandleTerrainCollisions(List<object> elements)
		{
			var terrainPoints = elements
				.OfType<ValueTuple<Vector2, Tile>>()
				.Select(t => t.Item1);

			foreach (Vector2 point in terrainPoints)
				InnerMessage(new TerrainCollided(this, point));
		}

		private Vector2 GetContactPoint(MapElement element)
			=> element.Area.Value.GetClosestPoint(_area.Value.Center);

		/// <summary>
		/// Announces each object in specified radius.
		/// </summary>
		protected void ReportObjects()
		{
			HashSet<string> nearObjects =
				(from o in Locality.GetNearByObjects(_area.Value.Center, _nearObjectRadius, false)
				 select o.Name.Indexed)
				.ToHashSet();

			foreach (string o in nearObjects)
			{
				if (_navigatedObject != null && _navigatedObject.Name.Indexed == o)
					continue;

				// Announce new objects.
				if (!_nearObjects.Contains(o) && nearObjects.Contains(o))
				{
					ReportPosition message = new(Owner);
					Item theObject = World.GetItem(o);
					theObject.TakeMessage(message);
					Tolk.Speak(theObject.Name.Friendly);
				}
			}

			_nearObjects = nearObjects;
		}

		/// <summary>
		/// Specifies a radius for near object announcements.
		/// </summary>
		protected const int _nearObjectRadius = 6;

		/// <summary>
		/// Lists near objects that have been already announced.
		/// </summary>
		protected HashSet<string> _nearObjects = new();

		/// <summary>
		/// Processes the ChipotlesCarMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnChipotlesCarMoved(ChipotlesCarMoved message)
		{
			_carMovement = message;
			Locality.TakeMessage(message, true);
		}



		/// <summary>
		/// Returns information about all navigable objects from current locality in the specified radius around the NPC.
		/// </summary>
		/// <returns>Tuple with an object list and text descriptions including distance and position of each object</returns>
		protected NavigableItemsModel GetNavigableItems()
		{
			List<string> descriptions = new();
			IEnumerable<Item> items = Locality.GetNearByObjects(_area.Value.Center, _navigableObjectsRadius);

			foreach (Item item in items)
			{
				string name = item.Name.Friendly;
				float distance = World.GetDistance(Owner, item);
				string distanceDescription = GetDistanceDescription(distance);
				float compassDegrees = GetAngle(item);
				string angleDescription = Angle.GetDescription(compassDegrees);
				descriptions.Add($"{name} {distanceDescription} {angleDescription}");
			}

			var result = new NavigableItemsModel(descriptions.ToArray(), items.ToArray());
			return result;
		}

		/// <summary>
		/// Processes the SayCharacters message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnSayCharacters(SayCharacters message)
			=> SayCharacters();


		/// <summary>
		/// Processes the SayNearestObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnSayObjects(SayObjects message)
			=> SayObjects();

		/// <summary>
		/// Announces the nearest characters around the Chipotle 
		/// </summary>
		private void SayCharacters()
		{
			// If there's any navigation in progress, it'll be stopped and this command will be cancelled.
			if (NavigationInProgress)
			{
				StopNavigation();
				return;
			}

			NavigableCharactersModel characters = GetNavigableCharacters();
			InnerMessage(new SayCharactersResult(this, characters.Descriptions));
		}


		/// <summary>
		/// Announces the nearest objects around the Chipotle 
		/// </summary>
		private void SayObjects()
		{
			// If there's any navigation in progress, it'll be stopped and this command will be cancelled.
			StopNavigation();
			if (!NavigationInProgress)
				InnerMessage(new SayObjectsResult(this, GetNavigableItems().Descriptions));
		}

		/// <summary>
		/// Processes the SayTerrain message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnSayTerrain(SayTerrain message)
			=> Tolk.Speak(World.Map[_area.Value.UpperLeftCorner].Terrain.GetDescription(), true);

		/// <summary>
		/// Processes the SayVisitedLocality message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnSayVisitedLocality(SayVisitedRegion message)
			=> InnerMessage(new SayVisitedLocalityResult(this, _inVisitedRegion));

		/// <summary>
		/// Processes the MakeStep message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnStartWalk(StartWalk message)
		{
			if (_blockWalk || StandUp())
				return;

			_blockWalk = true;
			_startWalkMessage = message;
			_walking = true;
			MakeStep();
		}

		/// <summary>
		/// Processes the StopWalk message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnStopWalk(StopWalk message)
			=> StopWalk();

		/// <summary>
		/// Interrupts walk of the Chipotle NPC.
		/// </summary>
		private void StopWalk()
		{
			_blockWalk = false;
			_walking = false;
			_startWalkMessage = null;
		}

		/// <summary>
		/// Processes the TurnEntity message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnChangeOrientation(ChangeOrientation message)
		{
			Orientation2D source = _orientation;
			_orientation.Rotate(message.Degrees);
			InnerMessage(new OrientationChanged(this, source, _orientation, message.Direction));
			Locality.TakeMessage(new OrientationChanged(Owner, source, _orientation, message.Direction));
			_speed += Math.Abs(message.Degrees) * World.DeltaTime;
		}

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnInteract(Interact message)
		{
			if (message.Object != null)
			{
				UseElement(message.Object);
				return;
			}

			Door door = GetDoorBefore();
			UsableObjectsModel objects = GetUsableItemsAndCharactersBefore(_objectManipulationRadius);

			if (door != null)
			{
				HandleDoorAndObjectsInteractions(door, objects);
				return;
			}

			if (objects.Result == UsableObjectsModel.ResultType.NothingFound)
				InnerMessage(new InteractResult(this, InteractResult.ResultType.NoObjects));
			else if (objects.Result == UsableObjectsModel.ResultType.Unusable)
				InnerMessage(new InteractResult(this, InteractResult.ResultType.NoUsableObjects));
			else if (objects.Result == UsableObjectsModel.ResultType.Far)
				InnerMessage(new InteractResult(this, InteractResult.ResultType.Far));
			else // Success
			{
				if (objects.Objects.Count() == 1)
					UseElement(objects.Objects[0]);
				else if (objects.Objects.Any())
					WindowHandler.ActiveWindow.TakeMessage(new SelectObjectToUse(Owner, objects.Objects));
			}
		}

		/// <summary>
		/// Handles simultaneous collisions with a door and items or characters.
		/// </summary>
		private void HandleDoorAndObjectsInteractions(Door door, UsableObjectsModel itemsOrCharacters)
		{
			if (itemsOrCharacters.Objects.IsNullOrEmpty())
				UseElement(door);
			else if (itemsOrCharacters.Objects.Count() == 1)
				UseCloserElement(door, itemsOrCharacters.Objects[0]);
			else
			{ // More awailable objects 
				MapElement closest = World.GetClosestElement(itemsOrCharacters.Objects, Owner);
				if (World.IsCloser(Owner, door, closest))
					UseElement(door);
				else UseElement(closest);
			}
		}
		/// <summary>
		/// Takes two map elements and uses the closer one.
		/// </summary>
		/// <param name="a">First Entity</param>
		/// <param name="b">Second Entity</param>
		/// <remarks>When both elements are at the same distance, the first oen is used.</remarks>
		private void UseCloserElement(MapElement a, MapElement b)
		{
			if (World.IsCloser(Owner, a, b))
				UseElement(a);
			else if (World.IsCloser(Owner, b, a))
				UseElement(b);
			else UseElement(a);
		}

		/// <summary>
		/// Interacts with the specified map element.
		/// </summary>
		/// <param name="element">The element to be used</param>
		private void UseElement(MapElement element)
		{
			Vector2? point = FindManipulationPoint(element);

			if (element is Door)
				element.TakeMessage(new UseDoor(Owner, point.Value));
			else element.TakeMessage(new ObjectsUsed(Owner, point.Value, element as Item));
		}

		/// <summary>
		/// Controlls walk of the Chipotle NPC.
		/// </summary>
		protected override void PerformWalk()
		{
			base.PerformWalk();

			if (_walking && _walkTimer >= _speed)
				MakeStep();
		}

		/// <summary>
		/// Plays the game finals.
		/// </summary>
		private void PlayFinalScene() => World.PlayCutscene(Owner, "cs35");

		/// <summary>
		/// Terminates the game and runs the main menu.
		/// </summary>
		private void QuitGame()
			=> World.QuitGame();

		/// <summary>
		/// He puts the NPC on its feet if it is sitting and plays the appropriate sound.
		/// </summary>
		/// <returns>True on success</returns>
		private bool StandUp()
		{
			if (_sittingAtPubTable)
			{
				_sittingAtPubTable = false;
				World.PlayCutscene(Owner, IsTuttleNearBy() ? "cs27" : "cs26");
				return true;
			}

			if (_sittingOnChair)
			{
				_sittingOnChair = false;
				World.PlayCutscene(Owner, "snd13");
				return true;
			}

			return false;
		}

		/// <summary>
		/// Relocates the NPC near the car of the Detective Chipotle NPC if the car has moved recently.
		/// </summary>
		private void WatchCar()
		{
			if (_carMovement == null) return;

			// Jump to target locality
			Vector2? target = World.GetRandomWalkablePoint(_carMovement.Target, 1, 2);

			if (target == null)
				throw new InvalidOperationException("No walkable tile found.");

			World.GetLocality((Vector2)target).TakeMessage(_carMovement);
			InnerMessage(new LeftBycar(this));
			JumpTo((Vector2)target, true);//todo předělat na Rectangle
			_carMovement = null;
		}

		/// <summary>
		/// Plays an appropriate cutscene if the icecream machine (automat v1) has been used recently.
		/// </summary>
		private void WatchIcecreamMachine()
		{
			if (World.GetItem("automat v1").Used)
				World.PlayCutscene(this, "cs13");
		}

		/// <summary>
		/// Makes the Easterby street (ulice s1) accessible if the Detective Chipotle has answered a
		/// phone call recently. and plays an appropriate cutscene
		/// </summary>
		private void WatchPhone()
		{
			if (_phoneCountdown && _phoneDeltaTime >= _phoneInterval)
			{
				_phoneCountdown = false;
				World.GetItem("detektivovo auto").TakeMessage(new UnblockLocality(Owner, World.GetLocality("ulice s1")));
				World.PlayCutscene(Owner, "cs22");
			}
		}

		/// <summary>
		/// Plays an appropriate cutscene if the Detective Chipotle NPC stepped into the puddle next
		/// to the Walsch's pool (bazén w1) object.
		/// </summary>
		/// <param name="point">
		/// Coordintes of a tile the Detective Chipotle NPC is gonna step on to.
		/// </param>
		private void WatchPuddle(Vector2 point)
		{
			if (!_steppedIntoPuddle
				&& CurrentTile.tile.Terrain == TerrainType.Puddle
				&& World.GetItem("bazén w1").Area.Value.GetDistanceFrom(CurrentTile.position) <= _puddleRadius)
			{
				_steppedIntoPuddle = true;
				World.PlayCutscene(Owner, "cs2");
			}
		}

		/// <summary>
		/// Relocates the Chipotle's car (detektivovo auto) object to the afphalt road (asfaltka c1)
		/// locality and plays an appropriate cutscene if crutial objects in the sweeney's room
		/// (pokoj s1) has been used.
		/// </summary>
		/// <remarks>The Detective Chiptole and Tuttle NPCs should move with the car afterwards.</remarks>
		private void WatchSweeneysRoom()
		{
			if (
				World.GetItem("trezor s1").Used
				&& (World.GetItem("stůl s1").Used || World.GetItem("stůl s5").Used)
				&& World.GetItem("počítač s1").Used
				&& World.GetItem("mobil s1").Used
			)
			{
				Car.TakeMessage(new MoveChipotlesCar(Owner, AsphaltRoad));
				World.PlayCutscene(Owner, "cs19");
			}
		}

		/// <summary>
		/// Computes length of the next step of the NPC.
		/// </summary>
		/// <param name="goal">Coordinates of the goal of an ongoing movement of the NPC</param>
		protected override int GetSpeed()
		{
			float coefficient = 1;
			if (_startWalkMessage.Direction == TurnType.SharplyLeft || _startWalkMessage.Direction == TurnType.SharplyRight)
				coefficient = _sideSpeed;
			else if (_startWalkMessage.Direction == TurnType.Around)
				coefficient = _backwardSpeed;

			return (int)(GetTerrainSpeed() * coefficient);
		}

		/// <summary>
		/// Solves a situation when the NPC encounters an obstacle.
		/// </summary>
		/// <param name="direction">Direction of the trajectory</param>
		/// <returns>True if collisions were detected</returns>
		protected bool DetectCollisions(Vector2 direction)
		{
			CollisionsModel collisions = World.DetectCollisionsOnTrack(Owner, direction, _stepLength);
			if (collisions.Obstacles == null && !collisions.OutOfMap)
				return false;

			// Stop walking.
			_walking = false;
			_startWalkMessage = null;

			// If the track doesn't lead off the map, announce collisions.
			if (collisions.Obstacles != null)
				HandleCollisions(collisions.Obstacles);
			return true;
		}
	}
}