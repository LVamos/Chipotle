using DavyKager;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Models;
using Game.Terrain;
using Game.UI;

using Luky;

using OpenTK;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
	/// <summary>
	/// Controls movement of the Detective Chipotle NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChipotlePhysicsComponent : PhysicsComponent
	{
		public float CalculateAngleBetweenPlayerAndItem(Item item)
		{
			// Bod na obvodu obdélníku hráče ve směru orientace
			Vector2 playerReferencePoint = _area.PointOnPerimeter(_orientation.UnitVector);

			// Nejbližší bod na objektu k referenčnímu bodu hráče
			Vector2 nearestPointOnItem = item.Area.GetClosestPoint(playerReferencePoint);

			// Vektor směru od hráče k objektu
			Vector2 directionToItem = nearestPointOnItem - playerReferencePoint;

			// Normalizace orientace hráče a vektoru směru
			Vector2 normalizedPlayerOrientation = _orientation.UnitVector.Normalized();
			Vector2 normalizedDirection = directionToItem.Normalized();

			// Výpočet úhlu
			float dotProduct = Vector2.Dot(normalizedPlayerOrientation, normalizedDirection);
			float angleInRadians = (float)Math.Acos(dotProduct);

			// Převod úhlu z radiánů na stupně
			float angleInDegrees = OpenTK.MathHelper.RadiansToDegrees(angleInRadians);

			// Převod na kompasové stupně (předpokládá, že hráč je orientován na sever)
			float compassAngle = (360 - angleInDegrees) % 360;

			// If the item is closer than length of player's step, return nearest multiplication of 90.
			float distance = item.Area.GetDistanceFrom(playerReferencePoint);
			if (Math.Abs(distance - _stepLength) < 1)
				return RoundToNearest90(compassAngle);

			return compassAngle; // Vrátí úhel v kompasových stupních
		}

		/// <summary>
		/// Rounds the given number to the nearest 90.
		/// </summary>
		/// <param name="n">The number to round.</param>
		/// <returns>The rounded number.</returns>
		protected int RoundToNearest90(float n)
		{
			if (n >= 315 || n < 45)
				return 0;

			if (n < 135)
				return 90;

			if (n < 225)
				return 180;

			return 270;
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected void OnResearchObject(ResearchObject m)
		{
			GameObject @object = SomethingBefore();
			@object?.TakeMessage(new ObjectResearched(Owner)); // Announce the object or character that it was researched.
			InnerMessage(new SayObjectDescription(this, @object)); // Let description of the object or character be spoken.
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
		protected HashSet<string> _inventory = new HashSet<string>();

		/// <summary>
		/// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
		/// </summary>
		/// <param name="step">The distance between the NPC and the required tile</param>
		/// <returns>A reference to an tile that lays in the specified distance and direction</returns>
		/// <see cref="PhysicsComponent.Orientation"/>
		protected override Vector2 GetNextTile()
		{
			Orientation2D finalOrientation = GetStepDirection();
			return GetNextTile(finalOrientation, 1).position;
		}

		private Vector2 GetStepDirection()
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
		public ChipotlePhysicsComponent() : base()
		{
			// set initial position.
			Width = 0.5f;
			Height = 0.5f;
			if (Settings.TestChipotleStartPosition.HasValue)
			{
				Vector2 position = Settings.TestChipotleStartPosition.Value;
				StartPosition = new Rectangle(
					position,
					new Vector2(position.X + Width, position.Y - Height)
					);
			}
			else
				StartPosition = new Rectangle(new Vector2(1028, 1034), new Vector2(1028.5f, 1033.5f));

		}

		/// <summary>
		/// Specifies if the NPC can walk.
		/// </summary>
		[ProtoIgnore]
		protected bool _blockWalk;

		/// <summary>
		/// Time interval for backward walk
		/// </summary>
		private const float _backwardSpeed = 2;

		/// <summary>
		/// Time interval for forward walk
		/// </summary>
		private const int _forwardWalkSpeed = 15;

		/// <summary>
		/// Time interval for walk to the side
		/// </summary>
		private const float _sideSpeed = 1.6f;

		/// <summary>
		/// Stores references to all the localities the NPC has visited.
		/// </summary>
		private readonly HashSet<Locality> _visitedLocalities = new HashSet<Locality>();

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
			=> World.GetObject("detektivovo auto") as ChipotlesCar;

		/// <summary>
		/// Returns reference to the Tuttle NPC.
		/// </summary>
		[ProtoIgnore]
		private Character Tuttle
			=> World.GetCharacter("tuttle");

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Start()
		{
			base.Start();
			JumpTo(StartPosition, true);
			_orientation = new Orientation2D(0, 1);
			InnerMessage(new OrientationChanged(this, _orientation, _orientation, TurnType.None, true));
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(GameMessage message)
		{
			switch (message)
			{
				case ResearchObject m: OnResearchObject(m); break;
				case PickUpObjectResult m: OnPickUpObjectResult(m); break;
				case RunInventoryMenu m: OnRunInventoryMenu(m); break;
				case PickUpObject m: OnPickUpObject(m); break;
				case CreatePredefinedSave c: OnCreatePredefinedSave(c); break;
				case LoadPredefinedSave l: OnLoadPredefinedSave(l); break;
				case ExitNavigationStopped ens: OnExitNavigationStopped(ens); break;
				case ListExits lex: OnListExits(lex); break;
				case ObjectNavigationStopped ons: OnObjectNavigationStopped(ons); break;
				case StopObjectNavigation son: OnStopObjectNavigation(son); break;
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
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be processed</param>
		protected void OnRunInventoryMenu(RunInventoryMenu m)
		{
			// Check if inventory isn't empty.
			if (Owner.Inventory.IsNullOrEmpty())
			{
				InnerMessage(new EmptyInventory(this));
				return;
			}

			// Run the menu
			(InventoryMenu.ActionType a, Item o) result = InventoryMenu.Run(Owner.Inventory.ToArray<Item>());

			if (result.a == InventoryMenu.ActionType.Use)
			{
				// If there's another item or character standing before the player use it through the item held by the player.
				GameObject another = SomethingBefore();

				// If there's no other item or character standing before this character use position of the NPC as a manipulation point. Otherwise use nearest point before this character.
				Vector2 manipulationPoint = another != null ? GetNextTile(1).position : _area.Center;

				var useMessage = new UseObjects(Owner, manipulationPoint, result.o, another);
				result.o.TakeMessage(useMessage);
				another?.TakeMessage(useMessage);
				return;
			}

			if (result.a == InventoryMenu.ActionType.Place)
			{
				Vector2 point = _area.UpperLeftCorner + _orientation.UnitVector * .2f;
				var placeMessage = new PlaceObject(Owner, result.o, point);
				result.o.TakeMessage(placeMessage);
			}
		}

		/// <summary>
		/// Handles the PickUpObject message.
		/// </summary>
		/// <param name="message">The message to be processed.
		protected void OnPickUpObject(PickUpObject message)
		{
			(List<object> obstacles, bool outOfMap) result = World.DetectCollisionsOnTrack(Owner, GetStepDirection(), _stepLength);
			Item item = null;
			if (result.obstacles != null)
			{
				item = result.obstacles
				.FirstOrDefault(o => o is Item) as Item;
			}

			if (item == null)
			{
				InnerMessage(new PickUpObjectResult(this));
				return;
			}

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
		/// Processes the ExitNavigationStopped message.
		/// </summary>
		/// <param name="message">source of the message</param>
		private void OnExitNavigationStopped(ExitNavigationStopped message)
		{
			if (message.Sender == _navigatedExit)
				_navigatedExit = null;
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

			StopNavigation(); // If there's any navigation in progress,  it'll be stopped and this command will be cancelled.
			if (NavigationInProgress)
				return;

			(List<List<string>> descriptions, Passage[] exits) result = GetNavigableExits();

			if (result.descriptions.IsNullOrEmpty()) // No objects near by
			{
				InnerMessage(new SayExitsResult(this));
				return;
			}

			// Run the menu
			int option = WindowHandler.Menu(result.descriptions, "Východy", " ", 2, false);
			if (option == -1)
				return;

			Passage target = result.exits[option];
			if (_navigatedExit != null && _navigatedExit != target)
				StopNavigation();

			_navigatedExit = target;
			_navigatedExit.TakeMessage(new StartExitNavigation(Owner));
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
		private void OnObjectNavigationStopped(ObjectNavigationStopped message)
		{
			if (message.Sender == _navigatedObject)
				_navigatedObject = null;
		}

		/// <summary>
		/// Processes the StopObjectNavigation message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnStopObjectNavigation(StopObjectNavigation message)
			=> StopNavigation();

		/// <summary>
		/// Specifies max radius for navigable objects enumeration.
		/// </summary>
		protected const int _navigableObjectsRadius = 50;

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

			int option = WindowHandler.Menu(descriptions, "Okolní objekty", " ", 0, false);

			if (option == -1)
				return;

			Item target = objects.Items[option];

			_navigatedObject = target;
			_navigatedObject.TakeMessage(new StartObjectNavigation(Owner));
		}

		/// <summary>
		/// Stops ongoing object navigation.
		/// </summary>
		private void StopNavigation()
		{
			if (_navigatedObject != null)
				_navigatedObject.TakeMessage(new StopObjectNavigation(Owner));

			if (_navigatedExit != null)
				_navigatedExit.TakeMessage(new StopExitNavigation(Owner));
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
		protected Item _navigatedObject;

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void Update()
		{
			base.Update();
			CountPhone();
			PerformWalk();
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
			Passage occupiedPassage = World.GetPassage(_area.Center);
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
			// Round the distance so that its value corresponds to a multiple of 0.5.
			int meters = (int)distance;
			float centimeters = (float)(distance - meters);
			if (centimeters <= .25f || centimeters >= .75f)
				centimeters = 0;
			else if ((centimeters > .25f && centimeters <= .5f) || (centimeters > .5f && centimeters < .75f))
				centimeters = .5f;
			float roundedDistance = meters + centimeters;

			// Compose output
			if (Math.Abs(distance - _stepLength) <= .5f)
				return string.Empty;
			if (roundedDistance == 1)
				return " metr ";
			if (roundedDistance >= 2 && roundedDistance <= 4)
				return $" {meters} metry ";
			if (roundedDistance == 1.5f)
				return " metr a půl ";
			else if (centimeters == .5f)
				return $" {meters} a půl metrů ";
			return $" {meters} metrů ";
		}

		/// <summary>
		/// Returns text descriptions of the specified exits including distance and position.
		/// </summary>
		/// <returns>A string array</returns>
		private (List<List<string>> descriptions, Passage[] exits) GetNavigableExits()
		{
			Passage[] exits = Locality.GetNearestExits(_area.Center, _exitRadius).ToArray<Passage>();

			Vector2 me = _area.Center;
			List<List<string>> descriptions =
	(
	from e in exits
	let point = e.Area.GetClosestPoint(_area.Center)
	let distance = GetDistanceDescription((int)World.GetDistance(me, point))
	let type = e.ToString()
	let to = e.AnotherLocality(Locality).To + " "
	let index = to.IndexOf(' ')
	let to1 = to.Substring(0, index)
	let to2 = to.Substring(index + 1)

	let angle = Angle.GetDescription(GetAngle(point))
	select new List<string> { type, to1, to2, distance, angle }
	).ToList();

			return (descriptions, exits);
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
			Random r = new Random();
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
		protected readonly Dictionary<Locality, HashSet<int>> _motionTrack = new Dictionary<Locality, HashSet<int>>();

		/// <summary>
		/// Determines the maximum distance from the pool at which stepping into a puddle will trigger a cutscene.
		/// </summary>
		protected const float _puddleRadius = 10;

		/// <summary>
		/// Computes amount of horizontal motion track regions for the current locality.
		/// </summary>
		protected int MotionTrackWidth => (int)(Locality.Area.Width / _motionTrackRadius + (Locality.Area.Width % _motionTrackRadius > 0 ? 1 : 0));

		/// <summary>
		/// Computes amount of vertical motion track regions for the current locality.
		/// </summary>
		protected int MotionTrackHeight => (int)(Locality.Area.Height / _motionTrackRadius + (Locality.Area.Height % _motionTrackRadius > 0 ? 1 : 0));

		protected int GetRegionIndex(Vector2 point)
		{
			Vector2 relative = Terrain.Rectangle.GetRelativeCoordinates(point);

			int rX = (int)(relative.X / _motionTrackRadius + (relative.X % _motionTrackRadius > 0 ? 1 : 0));
			int rY = (int)(relative.Y / _motionTrackRadius + (relative.Y % _motionTrackRadius > 0 ? 1 : 0));
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
				_motionTrack[Locality] = new HashSet<int>();

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

		private void AnnounceCollisions(List<object> elements)
		{
			foreach (object obj in elements)
			{
				Vector2 contactPoint = Vector2.Zero;
				if (obj is MapElement element)
					contactPoint = element.Area.GetClosestPoint(_area.Center);

				if (obj is ValueTuple<Vector2, Tile> terrainInfo)
					InnerMessage(new TerrainCollided(this, terrainInfo.Item1));

				else if (obj is GameObject g)
				{
					g.TakeMessage(new ObjectsCollided(Owner, g, contactPoint));
					InnerMessage(new ObjectsCollided(this, g, contactPoint));
				}

				else if (obj is Character c)
				{
					c.TakeMessage(new ObjectsCollided(Owner, c, contactPoint));
					InnerMessage(new ObjectsCollided(this, c, contactPoint));
				}

				else if (obj is Door d)
				{
					d.TakeMessage(new DoorHit(Owner, d, contactPoint));
					InnerMessage(new DoorHit(this, d, contactPoint));
				}
			}
		}

		/// <summary>
		/// Announces each object in specified radius.
		/// </summary>
		protected void ReportObjects()
		{
			HashSet<string> nearObjects =
				(from o in Locality.GetNearByObjects(_area.Center, _nearObjectRadius, false)
				 select o.Name.Indexed)
				 .ToHashSet();

			foreach (string o in nearObjects)
			{
				if (_navigatedObject != null && _navigatedObject.Name.Indexed == o)
					continue;

				// Announce new objects.
				if (!_nearObjects.Contains(o) && nearObjects.Contains(o))
				{
					ReportPosition message = new ReportPosition(Owner);
					Item theObject = World.GetObject(o);
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
		protected HashSet<string> _nearObjects = new HashSet<string>();

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
			Vector2 me = _area.PointOnPerimeter(_orientation.UnitVector);

			Item[] items = Locality.GetNearByObjects(me, _navigableObjectsRadius).ToArray<Item>();

			List<string> descriptions = new List<string>();
			foreach (Item item in items)
			{
				Vector2 point = item.Area.GetClosestPoint(me);
				string name = item.Name.Friendly;
				float distance = item.Area.GetDistanceFrom(me);
				string distanceDescription = GetDistanceDescription(distance);
				float compassDegrees = CalculateAngleBetweenPlayerAndItem(item);
				string angleDescription = Angle.GetDescription(compassDegrees);

				descriptions.Add($"{name}: {distanceDescription} {angleDescription}: ");
			}

			var result = new NavigableItemsModel(descriptions.ToArray(), items);
			return result;
		}

		/// <summary>
		/// Processes the SayNearestObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnSayObjects(SayObjects message)
			=> SayObjects();

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
			=> Tolk.Speak(World.Map[_area.UpperLeftCorner].Terrain.GetDescription(), true);

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
		protected void OnInteract(Interact message)
		/// <param name="message">The message to be processed</param>
		{
			void Use(MapElement e)
			{
				if (e == null)
					return;

				//Find the point at which the object will be manipulated.
				Vector2 point = default;
				Vector2? opposite = _area.FindOppositePoint(e.Area);
				point = opposite.HasValue ? opposite.Value : e.Area.GetClosestPoint(CurrentTile.position);

				if (e is Door)
					e.TakeMessage(new UseDoor(Owner, point));
				else e.TakeMessage(new UseObjects(Owner, point, e as Item));
			}

			// Find doors in radius of 2 meters that are in front or behind the player or door in which the player is standing.
			Vector2 me = _area.Center;
			IEnumerable<Passage> doors = World.GetNearestPassages(me, true, 2).Union(_area.GetPassages().Where(p => p is Door));

			IEnumerable<Passage> usable =
				from d in doors
				let obstacle = World.DetectObstacles(new Rectangle(d.Area.GetClosestPoint(me), me))
				where d.IsInFrontOrBehind(me)
				select d;

			// Use the doors.
			foreach (Passage d in usable)
				Use(d);

			Use(World.GetObject(GetNextTile(1).position)); // Detect object in front of Chipotle and use it.
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
			if (_carMovement == null)
				return;

			// Fade master volume in
			World.Sound.SetGroupVolume("master", 0);
			World.Sound.FadeMaster(FadingType.In, .00001f, World.Sound.DefaultMasterVolume);

			// Jump to target locality
			Vector2? target = World.GetRandomWalkablePoint(_carMovement.Target, 1, 2);
			Assert(target.HasValue, "No walkable tile found.");
			World.GetLocality((Vector2)target).TakeMessage(_carMovement);
			JumpTo((Vector2)target, true);//todo předělat na Rectangle
			_carMovement = null;
		}

		/// <summary>
		/// Plays an appropriate cutscene if the icecream machine (automat v1) has been used recently.
		/// </summary>
		private void WatchIcecreamMachine()
		{
			if (World.GetObject("automat v1").Used)
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
				World.GetObject("detektivovo auto").TakeMessage(new UnblockLocality(Owner, World.GetLocality("ulice s1")));
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
				&& World.GetObject("bazén w1").Area.GetDistanceFrom(CurrentTile.position) <= _puddleRadius)
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
				 World.GetObject("trezor s1").Used
				&& (World.GetObject("stůl s1").Used || World.GetObject("stůl s5").Used)
				&& World.GetObject("počítač s1").Used
				&& World.GetObject("mobil s1").Used
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
			(List<object> obstacles, bool outOfMap) collisions = World.DetectCollisionsOnTrack(Owner, direction, _stepLength);
			if (collisions == default)
				return false;

			// Stop walking.
			_walking = false;
			_startWalkMessage = null;

			// If the track doesn't lead off the map, announce collisions.
			if (collisions.obstacles != null)
				AnnounceCollisions(collisions.obstacles);
			return true;
		}
	}
}