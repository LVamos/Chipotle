using Game.Audio;
using Game.Entities.Characters;
using Game.Mapping.Saves;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Characters.Movement;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Serialization.Protobuf.Snapshots.Entities;
using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;
using Game.Terrain;



using System;
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









	public class Item : Entity
	{
		public bool Passable { get; protected set; }

		/// <summary>
		/// Fades out and stops the sound loop of this object.
		/// </summary>
		private void StopAmbientSounds()
		{
			if (LoopPlaying)
				Sounds.SlideVolume(_ambientSource, Settings.ItemDefaultOcclusionDuration, 0);
			_ambientSource = null;
		}

		protected void LogCollision(Character character, Vector2 point)
		{
			string title = Passable ? "Objekt zaznamenal průchod postavy" : "Objekt zaznamenal náraz postavy";
			string itemName = $"Objekt: {Name.Inner}";
			string characterName = $"Postava: {character.Name.Inner}";
			string pointOfCollision = $"Bod srážky: {point.GetString()}";

			Logger.LogInfo(title, itemName, characterName, pointOfCollision);
		}

		protected void LogUssage(Character character, MapElement usedItem, MapElement target, Vector2 point)
		{
			string title = "Objekt zaznamenal použití";
			string characterName = character.Name.Inner;
			string itemName = $"Objekt: {usedItem.Name.Inner}";
			string targetName = string.Empty;
			if (target != null)
				targetName = $"Cíl: {target.Name.Inner}";
			string pointMessage = $"Bod: {point.GetString()}";

			Logger.LogInfo(title, characterName, itemName, targetName, pointMessage);
		}


		protected AudioSource _ambientSource;


		protected AudioSource _actionSource;


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

		public List<Zone> Zones
		{
			get => _zones.Select(World.GetZone).ToList();
		}

		/// <summary>
		/// Finds all zones the object or the NPC intersects with and saves their names into _zones.
		/// </summary>
		protected void FindZones()
		{
			if (_area == null)
				return;

			_zones =
				(from l in World.GetZones(_area.Value)
				 select l.Name.Inner)
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
		protected bool _usableOnce;

		public virtual void Restore(ItemSave save)
		{
			base.Restore((EntitySave)save);
			Initialize(
				save.Name != null ? save.Name.ToName() : null,
				save.Area.ToRectangle(),
				save.Type,
				save.Decorative,
				save.Pickable,
				save.Usable,
				save.Passable,
				save.CollisionSound,
				save.ActionSound,
				save.LoopSound,
				save.Cutscene,
save.UsableOnce,
save.AudibleOverWalls,
_defaultVolume,
save.StopWhenPlayerMoves,
save.QuickActionsAllowed,
save.PickingSound,
save.PlacingSound,
save.UsableWith
				);

			HeldBy = save.HeldBy != null ? World.GetCharacter(save.HeldBy) : null;
			Used = save.Used;
			UsedOnce = save.UsedOnce;
		}

		public virtual ItemSave Export()
		{
			var save = base.Export().ToItemsave();

			save.HeldBy = HeldBy?.Name?.Inner;
			save.Decorative = Decorative;
			save.Pickable = _pickable;
			save.Passable = Passable;
			save.CollisionSound = _sounds["collision"];
			save.ActionSound = _sounds["action"];
			save.LoopSound = _sounds["loop"];
			save.Cutscene = _cutscene;
			save.UsableOnce = _usableOnce;
			save.AudibleOverWalls = _audibleOverWalls;
			save.StopWhenPlayerMoves = _stopWhenPlayerMoves;
			save.QuickActionsAllowed = _quickActionsAllowed;
			save.PickingSound = _sounds["picking"];
			save.PlacingSound = _sounds["placing"];
			save.Used = Used;
			save.UsedOnce = UsedOnce;

			return save;
		}

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
		public virtual void Initialize(
			Name name,
			Rectangle? area,
			string type,
			bool decorative,
			bool pickable,
			bool usable,
			bool passable = false,
			string collisionSound = null,
			string actionSound = null,
			string loopSound = null,
			string cutscene = null,
			bool usableOnce = false,
			bool audibleOverWalls = true,
			float volume = 1,
			bool stopWhenPlayerMoves = false,
			bool quickActionsAllowed = false,
			string pickingSound = null,
			string placingSound = null,
			List<string> usableWith = null,
			bool acousticObstacle = false
			)
		{
			base.Initialize(name, type, area, acousticObstacle);
			HeldBy = null;
			_actionSource = null;
			_ambientSource = null;
			_cutscene = null;
			_loopStopped = false;
			_passByAudio = null;
			_placingAudio = null;
			_zones = new();
			_audibleOverWalls = false;

			Area = area;

			Decorative = decorative;
			Usable = usable;
			_pickable = pickable;
			Passable = passable;
			_usableOnce = usableOnce;
			_cutscene = cutscene;
			_audibleOverWalls = audibleOverWalls;
			_defaultVolume = volume;
			_stopWhenPlayerMoves = stopWhenPlayerMoves;
			_quickActionsAllowed = quickActionsAllowed;

			// Set up sound names
			if (passable)
				_sounds["collision"] = collisionSound ?? null;
			else _sounds["collision"] = collisionSound ?? Settings.DefaultCollisionSound;
			_sounds["action"] = actionSound;
			_sounds["picking"] = pickingSound;
			_sounds["placing"] = placingSound;
			_sounds["loop"] = loopSound;
			_sounds["passBy"] = "ObjectPassBy";
			// Preparing a long loop takes a while; while loading it does not stop the game.
			Sounds.PrepareLoop(loopSound);

			UsableWith = usableWith;
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
				l.TakeMessage(new ItemAppearedInZone(this, this, l));

			// Play loop sound if any and if the player can hear it.
			UpdateAmbientSounds();
		}

		/// <summary>
		/// Processes the Collision message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnOrientationChanged(OrientationChanged message) => StopActionWhenPlayerMoves();

		/// <summary>
		/// Stops sound loop of this object for good, if any.
		/// </summary>
		protected void StopLoop()
		{
			_loopStopped = true;
			StopAmbientSounds();
		}

		private bool _loopStopped;

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

			UpdateAmbientSounds();
			StopActionWhenPlayerMoves();
		}

		/// <summary>
		/// Stops the action sound.
		/// </summary>
		protected void StopActionWhenPlayerMoves()
		{
			if (_stopWhenPlayerMoves && _actionSource != null && _actionSource.isPlaying)
				Sounds.SlideVolume(_actionSource, Settings.ItemActionFadingDuration, 0);
		}

		protected bool _quickActionsAllowed;

		/// <summary>
		/// Specifies if the object can be carried.
		/// </summary>
		protected bool _pickable;


		protected float _lastUse;


		private AudioSource _passByAudio;

		/// <summary>
		/// Destroys the object.
		/// </summary>
		public override void Destroy()
		{
			base.Destroy();
			StopAmbientSounds();

			// Inform zones that the object disappeared.
			foreach (Zone l in Zones)
				l.TakeMessage(new ItemLeftZone(this, this, l));
		}

		/// <summary>
		/// Processes the Collision message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnObjectsCollided(ObjectsCollided message)
		{
			LogCollision(message.Sender as Character, message.ContactPoint);

			if (string.IsNullOrEmpty(_sounds["collision"]))
				return;

			Vector3 position = OutOfObstacle(message.ContactPoint).ToVector3(GetSoundHeight());
			string soundName = _sounds["collision"];
			Sounds.Play(soundName, position, _defaultVolume);
		}

		/// <summary>
		/// Returns the height of the object's sounds, at most the height of the player's ears.
		/// </summary>
		/// <remarks>The camera rises to the ears only after the objects start their loops.</remarks>
		private float GetSoundHeight()
			=> Mathf.Min(transform.localScale.y, World.Player.transform.localScale.y);

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnUseObjects(UseObjects message)
		{
			if (string.IsNullOrEmpty(_sounds["action"]) && string.IsNullOrEmpty(_cutscene))
				return;

			if (!string.IsNullOrEmpty(_cutscene))
				Cutscene.Play(this, _cutscene);
			else if (!_quickActionsAllowed)
			{
				if (_actionSource == null || !_actionSource.isPlaying)
					PlayActionSound(message.ManipulationPoint);
			}

			// Play the sound if predefined amount of time has passed since the last use.
			else if (_quickActionsAllowed && Time.time - _lastUse > Settings.ItemActionRepetetionInterval)
			{
				_lastUse = Time.time;
				Sounds.Play(_sounds["action"], OutOfObstacle(message.ManipulationPoint).ToVector3(GetSoundHeight()), _defaultVolume);
			}

			Used = true;
			if (_usableOnce && Used)
				Usable = false;

			if (message.UsedObject != this)
				return;

			ObjectsUsed newMessage = new(this, message.Sender, message.ManipulationPoint, message.UsedObject, message.Target);
			message.Sender.TakeMessage(newMessage);
			LogUssage(message.Sender, message.UsedObject, message.Target, message.ManipulationPoint);
		}

		protected void PlayActionSound(Vector2 manipulationPoint, string soundName = null)
		{
			Vector3 position = OutOfObstacle(manipulationPoint).ToVector3(GetSoundHeight());
			string finalSoundName = soundName ?? _sounds["action"];
			_actionSource = Sounds.Play(finalSoundName, position, _defaultVolume);
		}

		/// <summary>
		/// Handles the game reloaded message.
		/// </summary>
		private void OnGameReloaded() => UpdateAmbientSounds();

		/// <summary>
		/// Distance at which the direct sound of a loop is 40 dB weaker than 1 m from it; a farther loop is inaudible and stops.
		/// </summary>
		private const float _maxLoopDistance = 100;

		/// <summary>
		/// Keeps the sound loop of this object playing at the object while the player can hear it. Steam Audio attenuates it physically:
		/// with distance, with walls and closed doors on the direct path, and lets its reflections through open passages.
		/// </summary>
		protected void UpdateAmbientSounds()
		{
			if (string.IsNullOrEmpty(_sounds["loop"]) || _loopStopped)
				return;

			if (_area == null || GetDistanceToPlayer() > _maxLoopDistance)
			{
				StopAmbientSounds();
				return;
			}

			if (!LoopPlaying)
				PlayAmbient();
			else if (AcousticObstacle)
				_ambientSource.transform.position = GetAmbientPosition();
		}

		/// <summary>
		/// Generation of the pooled source of the loop; the pool gives the source to other sounds once the loop stops.
		/// </summary>
		private int _ambientGeneration;

		private bool LoopPlaying
			=> _ambientSource != null && Sounds.GetGeneration(_ambientSource) == _ambientGeneration;

		/// <summary>
		/// Plays sound loop of the object.
		/// </summary>
		protected void PlayAmbient()
		{
			_ambientSource = Sounds.Play(_sounds["loop"], GetAmbientPosition(), _defaultVolume, true, true, description: GetAmbientDescription());
			_ambientGeneration = Sounds.GetGeneration(_ambientSource);
		}

		private Vector3 GetAmbientPosition()
			=> (AcousticObstacle ? GetSurfaceTowardsPlayer() : _area.Value.Center).ToVector3(GetSoundHeight());

		/// <summary>
		/// Returns the point of the object nearest to the player, moved slightly out of it. An acoustic obstacle is solid geometry
		/// for Steam Audio, and a sound starting inside it would be occluded and would have no reflections.
		/// </summary>
		private Vector2 GetSurfaceTowardsPlayer()
			=> OutOfObstacle(_area.Value.GetClosestPoint(World.Player.Center));

		/// <summary>
		/// Moves a point of the object towards the player, out of its geometry.
		/// </summary>
		private Vector2 OutOfObstacle(Vector2 point)
			=> AcousticObstacle ? Vector2.MoveTowards(point, World.Player.Center, .15f) : point;

		private string GetAmbientDescription()
		{
			return $"loop for {Name.Inner} item";
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
				case PickUpItem m: OnPickUpItem(m); break;
				case ReportPosition m: OnReportPosition(m); break;
				case OrientationChanged oc: OnOrientationChanged(oc); break;
				case CharacterMoved em: OnCharacterMoved(em); break;
				case Reloaded gr: OnGameReloaded(); break;
				case ObjectsCollided oc: OnObjectsCollided(oc); break;
				case UseObjects uo: OnUseObjects(uo); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">Source of the message</param>
		private void OnPlaceItem(PlaceItem message)
		{
			if (message.Target == null)
				throw new ArgumentNullException(nameof(message.Target));

			HeldBy = null;
			Area = message.Target;

			//Register the item in target zones.
			_zones = new();
			List<Zone> zones = World.GetZones(_area.Value).ToList();
			foreach (Zone zone in zones)
			{
				ItemAppearedInZone appearedMessage = new(this, this, zone);
				zone.TakeMessage(appearedMessage);
				_zones.Add(zone.Name.Inner);
			}

			Placed();
			LogPlacement(message.Sender as Character, message.Target.Value);
		}

		protected void LogPlacement(Character character, Rectangle position)
		{
			string title = "Objekt zaznamenal pokus o položení";
			string itemName = $"Objekt: {Name.Inner}";
			string pointOfPlacement = string.Empty;
			pointOfPlacement = $"Bod umístění: {position.ToString()}";

			Logger.LogInfo(title, itemName);
		}

		/// <summary>
		/// Handles the PickUpObject message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected void OnPickUpItem(PickUpItem message)
		{
			PickUpItemResult.ResultType result = CanBePicked() ? PickUpItemResult.ResultType.Success : PickUpItemResult.ResultType.Unpickable;

			if (result == PickUpItemResult.ResultType.Success)
			{
				Picked();
				HeldBy = (Character)message.Sender;
				Area = null;
			}

			// Report the result.
			PickUpItemResult newMessage = new(this, this, result, message.Silently);
			message.Sender.TakeMessage(newMessage);
			LogPickup(message.Sender as Character, result);
		}

		protected void LogPickup(Character character, PickUpItemResult.ResultType result)
		{
			string title = "Objekt zaznamenal pokus o sebrání";
			string itemName = $"Objekt: {Name.Inner}";
			string characterName = $"Postava: {character.Name.Inner}";
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
		/// <param name="message">The message to be handled</param>
		private void OnReportPosition(ReportPosition message) => ReportPosition(false);
	}
}