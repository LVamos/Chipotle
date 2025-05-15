using Game.Audio;
using Game.Entities.Characters;
using Game.Messaging;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Models;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Base class for all simple game objects
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(CarsonsBench))]
	[ProtoInclude(101, typeof(CarsonsGrill))]
	[ProtoInclude(102, typeof(Corpse))]
	[ProtoInclude(103, typeof(ChipotlesCar))]
	[ProtoInclude(104, typeof(ChristinesBell))]
	[ProtoInclude(105, typeof(IcecreamMachine))]
	[ProtoInclude(106, typeof(KeyHanger))]
	[ProtoInclude(107, typeof(KillersCar))]
	[ProtoInclude(108, typeof(PoolsideBin))]
	[ProtoInclude(109, typeof(PubBench))]
	[ProtoInclude(110, typeof(SweeneysBell))]
	[ProtoInclude(111, typeof(VanillaCrunchCar))]
	public class Item : Entity
	{
		private const float _doorOpeningOcclusionDuration = .5f;
		private const float _doorClosingOcclusionDuration = .5f;

		protected void LogCollision(Character character, Vector2 point)
		{
			string title = "Objekt zaznamenal náraz postavy";
			string characterName = $"Postava: {character.Name.Indexed}";
			string pointOfCollision = $"Bod srážky: {point.GetString()}";

			Logger.LogInfo(title, characterName, pointOfCollision);
		}

		protected void LogUssage(Character character, Item usedItem, Entity target, Vector2 point)
		{
			string title = "Objekt zaznamenal použití";
			string characterName = character.Name.Indexed;
			string itemName = $"Objekt: {usedItem.Name.Indexed}";
			string targetName = string.Empty;
			if (target != null)
				targetName = $"Cíl: {target.Name.Indexed}";
			string pointMessage = $"Bod: {point.GetString()}";

			Logger.LogInfo(title, characterName, itemName, targetName, pointMessage);
		}

		[ProtoIgnore]
		protected AudioSource _loopAudio;

		[ProtoIgnore]
		protected AudioSource _actionAudio;

		[ProtoIgnore]
		protected AudioSource _placingAudio;

		/// <summary>
		/// React on placing on the ground.
		/// </summary>
		protected void Placed()
		{
			string soundName = _sounds["placing"];
			if (!string.IsNullOrEmpty(soundName))
				Sounds.Play(soundName, transform.position, _defaultVolume);
		}

		/// <summary>
		/// Backing field for Zones property.
		/// </summary>
		protected HashSet<string> _zones = new();

		/// <summary>
		/// Zones intersecting with this object.
		/// </summary>
		[ProtoIgnore]
		public IEnumerable<Zone> Zones => from name in _zones
										  select World.GetZone(name);

		/// <summary>
		/// Finds all zones the object or the NPC intersects with and saves their names into _zones.
		/// </summary>
		protected void FindZones()
		{
			if (_area == null)
				return;

			_zones =
				(from l in World.GetZones(_area.Value)
				 select l.Name.Indexed)
				 .ToHashSet();
		}

		/// <summary>
		/// Sets value of the Area property.
		/// </summary>
		/// <param name="value">A value assigned to the property</param>
		protected override void SetArea(Rectangle? value)
		{
			base.SetArea(value);
			if (value != null)
				transform.position = value.Value.Center.ToVector3(2);
		}

		/// <summary>
		/// Specifies if the object is held by an entity.
		/// </summary>
		public Character HeldBy { get; protected set; }

		/// <summary>
		/// Checks if the object can be picked up off the ground in the moment.
		/// </summary>
		/// <returns>True if the object can be picked up off the ground.</returns>
		public virtual bool CanBePicked() => _pickable && HeldBy == null;

		/// <summary>
		/// Indicates if the object stops playing its action sound when the player moves or turns.
		/// </summary>
		protected bool _stopWhenPlayerMoves;

		/// <summary>
		/// Specifies if the object works as a decorator.
		/// </summary>
		/// <remarks>When it's true the object isn't reported in SaySurroundingObjects command.</remarks>
		public bool Decorative;

		/// <summary>
		/// Cutscene that should be played when the object is used by an entity
		/// </summary>
		protected string _cutscene;

		/// <summary>
		/// Determines if the object shall be used just once
		/// </summary>
		private bool _usableOnce;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <param name="type">Type of the object</param>
		/// <param name="collisionSound">
		/// Sound that should be played when an entity bumps to the object
		/// </param>
		/// <param name="actionSound">Sound that should be played when an entity uses the object</param>
		/// <param name="loopSound">Sound that should be played in loop on background</param>
		/// <param name="cutscene">
		/// Cutscene that should be played when the object is used by an entity
		/// </param>
		/// <param name="usableOnce">Determines if the object shall be used just once</param>
		/// <param name="audibleOverWalls"></param>
		/// <param name="decorative">Determines if the object is just a decoration</param>
		/// <param name="pickable">Determines if the object can be picked by a character</param>
		/// <param name="pickingSound">A sound played when the object is picked by a character</param>
		/// <param name="placingSound">A sound played when the object is placed by a character</param>
		/// <param name="quickActionsAllowed">Specifies if the item can be used in rapid succession.</param>
		/// <param name="stopWhenPlayerMoves">Specifies if an ongoing action sound stops when the player moves.</param>
		/// <param name="volume">Specifies individual volume for sounds made by the object.</param>
		/// <remarks>
		/// The type parameter allows assigning objects with some special behavior to proper classes.
		/// </remarks>
		public virtual void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
		{
			base.Initialize(name, type, area);
			Area = area;

			Decorative = decorative;
			Usable = usable;
			_pickable = pickable;
			_usableOnce = usableOnce;
			_cutscene = cutscene;
			_audibleOverWalls = audibleOverWalls;
			_defaultVolume = volume;
			_stopWhenPlayerMoves = stopWhenPlayerMoves;
			_quickActionsAllowed = quickActionsAllowed;

			// Set up sound names
			_sounds["collision"] = collisionSound ?? "MovCrashDefault";
			_sounds["action"] = actionSound;
			_sounds["picking"] = pickingSound;
			_sounds["placing"] = placingSound;
			_sounds["loop"] = loopSound;
			_sounds["passBy"] = "ObjectPassBy";
		}

		/// <summary>
		/// Indicates if the object was used by an entity.
		/// </summary>
		public bool Used { get; protected set; }

		/// <summary>
		/// Determines if the object shall be used just once
		/// </summary>
		public bool UsedOnce { get; protected set; }

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();

			// Add the object to intersecting zones
			FindZones();
			foreach (Zone l in Zones)
				l.TakeMessage(new ObjectAppearedInZone(this, this, l));

			// Play loop sound if any and if the player can hear it.
			PlayLoop();
			UpdateLoop();
		}

		/// <summary>
		/// Processes the Collision message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnOrientationChanged(OrientationChanged message) => WatchPlayersMovement();

		/// <summary>
		/// Handles the DoorManipulated message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnDoorManipulated(DoorManipulated message)
		{
			bool playerHere = World.Player.SameZone(this);
			if (playerHere)
				return;

			bool doorClosed = message.Sender.State is PassageState.Closed or PassageState.Locked;
			float duration;
			ObstacleType obstacle;
			if (doorClosed)
			{
				duration = _doorClosingOcclusionDuration;
				obstacle = ObstacleType.Door;
			}
			else
			{
				duration = _doorOpeningOcclusionDuration;
				obstacle = ObstacleType.None;
			}

			UpdateOcclusion(obstacle, duration);
		}

		/// <summary>
		/// Stops sound loop of this object, if any.
		/// </summary>
		protected void StopLoop()
		{
			if (_loopAudio != null)
				Sounds.SlideVolume(_loopAudio, .5f, 0);
		}

		/// <summary>
		/// Determines if the object should be heart over walls and closed doors in other zones.
		/// </summary>
		protected bool _audibleOverWalls;

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnCharacterMoved(CharacterMoved message)
		{
			if (message.Sender != World.Player)
				return;

			UpdateNavigatingSoundPosition();
			UpdateLoop();
			WatchPlayersMovement();
		}

		/// <summary>
		/// Stops the action sound.
		/// </summary>
		protected void WatchPlayersMovement()
		{
			if (_stopWhenPlayerMoves && _actionAudio != null && _actionAudio.isPlaying)
				Sounds.SlideVolume(_actionAudio, .5f, 0);
		}

		private bool _quickActionsAllowed;

		/// <summary>
		/// Specifies if the object can be carried.
		/// </summary>
		protected bool _pickable;

		[ProtoIgnore]
		protected float _lastUse;

		[ProtoIgnore]
		private AudioSource _passByAudio;
		private ObstacleType _occludingObstacle;
		protected const float _intervalBetweenActions = .5f;
		private const int _loopMaxDistance = 9;
		private const float _loopMinDistance = .5f;
		private const int _loopInitialFadeDuration = 2;
		private const float _defaultOcclusionDuration = .5f;

		/// <summary>
		/// Destroys the object.
		/// </summary>
		protected override void DestroyObject()
		{
			base.DestroyObject();

			if (_loopAudio.isPlaying)
				_loopAudio.Stop();

			// Inform zones that the object disappeared.
			foreach (Zone l in Zones)
				l.TakeMessage(new ObjectDisappearedFromZone(this, this, l));
		}

		/// <summary>
		/// Processes the Collision message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnObjectsCollided(ObjectsCollided message)
		{
			Vector3 position = message.Position.ToVector3(GetSoundHeight());
			string soundName = _sounds["collision"];
			Sounds.Play(soundName, position, _defaultVolume);
			LogCollision(message.Sender as Character, message.Position);
		}

		private float GetSoundHeight()
		{
			float itemHeight = transform.localScale.y;
			float cameraHeight = Camera.main.transform.position.y;
			float height = itemHeight < cameraHeight ? itemHeight : cameraHeight;
			return height;
		}

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnObjectsUsed(ObjectsUsed message)
		{
			if (_usableOnce && Used)
			{
				Usable = false;
				return;
			}

			if (string.IsNullOrEmpty(_sounds["action"]) && string.IsNullOrEmpty(_cutscene))
				return;

			if (!string.IsNullOrEmpty(_cutscene))
				World.PlayCutscene(this, _cutscene);
			else if (!_quickActionsAllowed)
			{
				if (_actionAudio == null || !_actionAudio.isPlaying)
				{
					Vector3 position = message.ManipulationPoint.ToVector3(GetSoundHeight());
					_actionAudio = Sounds.Play(_sounds["action"], position, _defaultVolume);
				}
			}

			// Play the sound if predefined amount of time has passed since the last use.
			else if (_quickActionsAllowed && Time.time - _lastUse > _intervalBetweenActions)
			{
				_lastUse = Time.time;
				Sounds.Play(_sounds["action"], message.ManipulationPoint, _defaultVolume);
			}

			UsedOnce = !Used;
			Used = true;
			LogUssage(message.Sender, message.UsedObject, message.Target, message.ManipulationPoint);
		}

		/// <summary>
		/// Handles the game reloaded message.
		/// </summary>
		private void OnGameReloaded() => UpdateLoop();

		/// <summary>
		/// Plays the sound loop of this object if there's any.
		/// </summary>
		/// <param name="attenuated">Determines if the sound of the object should be played over a wall or other obstacles.</param>
		protected void UpdateLoop()
		{
			if (string.IsNullOrEmpty(_sounds["loop"]))
				return;

			ObstacleType obstacle = World.DetectOcclusion(this);
			//test
			//if (Type == "akvárko")
			//Tolk.Output(obstacle.ToString());

			UpdateOcclusion(obstacle);
		}

		protected void UpdateOcclusion(ObstacleType obstacle = ObstacleType.None, float duration = _defaultOcclusionDuration)
		{
			if (_sounds["loop"] == null)
				return;

			bool attenuate = obstacle is not ObstacleType.None and not ObstacleType.IndirectPath;
			;
			int cutoffFrequency = 0;
			float volume = _defaultVolume;

			if (_occludingObstacle == obstacle)
				return;
			_occludingObstacle = obstacle;

			switch (obstacle)
			{
				case ObstacleType.Wall: cutoffFrequency = Game.Audio.Sounds.OverWallLowpass; volume = _overWallVolume; break;
				case ObstacleType.Door: cutoffFrequency = Sounds.OverDoorLowpass; volume = OverDoorVolume; break;
				case ObstacleType.ItemOrCharacter: cutoffFrequency = Game.Audio.Sounds.OverObjectLowpass; volume = OverObjectVolume; break;
			}

			if (attenuate)
			{
				Sounds.SlideVolume(_loopAudio, duration, volume);
				Sounds.SlideLowPass(_loopAudio, duration, cutoffFrequency);
			}
			else
			{
				// Turn of attenuation
				if (_muffled && !attenuate)
				{
					Sounds.SlideVolume(_loopAudio, duration, _defaultVolume);
					Sounds.SlideLowPass(_loopAudio, duration, 22000);
				}
			}

			_muffled = attenuate;
		}

		/// <summary>
		/// Plays sound loop of the object with sound attenuation.
		/// </summary>
		/// <param name="obstacle">Type of obstacle between player and this object</param>
		protected void PlayLoop()
		{
			string name = _sounds["loop"];
			if (string.IsNullOrEmpty(name))
				return;

			Vector3 position = _area.Value.Center.ToVector3(GetSoundHeight());
			string description = $"loop for {Name.Indexed} item";
			_loopAudio = Sounds.Play(name, position, 0, true, true, description: description);
			_loopAudio.maxDistance = _loopMaxDistance;
			_loopAudio.minDistance = _loopMinDistance;
			_loopAudio.rolloffMode = AudioRolloffMode.Linear;

			// Slide volume
			float targetVolume = Sounds.GetLinearRolloffAttenuation(_loopAudio, _defaultVolume);
			Sounds.SlideVolume(_loopAudio, _loopInitialFadeDuration, targetVolume);
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case PlaceItem m: OnPlaceItem(m); break;
				case PickUpItem m: OnPickUpObject(m); break;
				case ReportPosition m: OnReportPosition(m); break;
				case OrientationChanged oc: OnOrientationChanged(oc); break;
				case CharacterMoved em: OnCharacterMoved(em); break;
				case DoorManipulated dm: OnDoorManipulated(dm); break;
				case Reloaded gr: OnGameReloaded(); break;
				case ObjectsCollided oc: OnObjectsCollided(oc); break;
				case ObjectsUsed uo: OnObjectsUsed(uo); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">Source of the message</param>
		private void OnPlaceItem(PlaceItem message)
		{
			Rectangle? vacancy = FindVacancy(
				message.Character.Area.Value,
		message.DirectionFromCharacter.Value,
		message.MaxDistanceFromCharacter
		);
			bool success = vacancy != null;

			if (success)
			{
				HeldBy = null;
				Area = vacancy;
				Placed();
			}

			MessagingObject sender = (message.Sender as MessagingObject);
			sender.TakeMessage(new PlaceItemResult(this, success));
			LogPlacement(message.Sender as Character, message.DirectionFromCharacter, success);
		}

		protected void LogPlacement(Character character, Vector2? position, bool success)
		{
			string title = "Objekt zaznamenal pokus o položení";
			string itemName = $"Objekt: {Name.Indexed}";
			string resultDescription = $"Výsledek: {(success ? "úspěch" : "neúspěch")}";
			string pointOfPlacement = string.Empty;
			if (position != null)
				pointOfPlacement = $"Bod umístění: {position.Value.GetString()}";

			Logger.LogInfo(title, itemName, resultDescription);
		}

		/// <summary>
		/// Tries to find a free spot on the ground to place the object on.
		/// </summary>
		/// <param name="lowerLeftCorner">A point that should intersect with the placed object</param>
		/// <returns>An area the object could be placed on</returns>
		protected Rectangle? FindVacancy(Rectangle characterArea, Vector2 directionFromCharacter, float maxDistanceFromCharacter)
		{
			float offset = characterArea.Height / 2;
			List<Rectangle> positions = new()
			{
Rectangle.FromCenter(characterArea.Center, Dimensions.height, Dimensions.width),
Rectangle.FromCenter(characterArea.Center, Dimensions.width, Dimensions.height)
			};

			return TryFindPosition(positions[0]) ?? TryFindPosition(positions[1]);

			Rectangle? TryFindPosition(Rectangle rectangle)
			{
				rectangle.Move(directionFromCharacter, offset); // Move the rectangle to the edge of the character
				float step = 0.1f;
				do
				{
					CollisionsModel result = World.DetectCollisions(null, rectangle);
					if ((!result.OutOfMap && result.Obstacles == null))
						return rectangle;
					rectangle.Move(directionFromCharacter, step);
				}
				while (rectangle.GetDistanceFrom(characterArea) <= maxDistanceFromCharacter);

				return null;
			}
		}

		/// <summary>
		/// Handles the PickUpObject message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected void OnPickUpObject(PickUpItem message)
		{
			PickUpObjectResult.ResultType result = CanBePicked() ? PickUpObjectResult.ResultType.Success : PickUpObjectResult.ResultType.Unpickable;

			if (result == PickUpObjectResult.ResultType.Success)
			{
				Picked();
				HeldBy = (Character)message.Sender;
				Area = null;
			}

			// Report the result.
			message.Sender.TakeMessage(new PickUpObjectResult(this, this, result));
			LogPickup(message.Sender as Character, result);
		}

		protected void LogPickup(Character character, PickUpObjectResult.ResultType result)
		{
			string title = "Objekt zaznamenal pokus o sebrání";
			string itemName = $"Objekt: {Name.Indexed}";
			string characterName = $"Postava: {character.Name.Indexed}";
			string resultDescription = $"Výsledek: {result}";

			Logger.LogInfo(title, itemName, characterName, resultDescription);
		}

		/// <summary>
		/// Reacts on picking off the ground.
		/// </summary>
		protected virtual void Picked()
		{
			string soundName = _sounds["picking"];
			if (!string.IsNullOrEmpty(soundName))
				Sounds.Play(soundName, transform.position, _defaultVolume);
		}

		/// <summary>
		/// Handles the ReportPosition message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		private void OnReportPosition(ReportPosition m) => ReportPosition();
	}
}