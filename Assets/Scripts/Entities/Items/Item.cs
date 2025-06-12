using Assets.Scripts.Models;

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
		private void UpdatePortals()
		{
			if (_portals == null)
				PlayAmbientFromExits();

			foreach (Passage exit in _portals.Keys)
			{
				PortalModel portal = _portals[exit];
				UpdatePortalOcclusion(portal, exit, _enteringZoneOcclusionDuration);
				UpdatePortalPosition(portal, exit);
			}
		}

		private void UpdatePortalPosition(PortalModel portal, Passage exit)
		{
			Vector3 position = GetPointForPortal(exit);
			portal.AudioSource.transform.position = position;
		}

		protected string GetPortalDescription(Passage passage)
		{
			Zone[] zones = passage.Zones.ToArray();
			string description = $"portal for {Name.Indexed}; passage between {zones[0].Name.Indexed} and {zones[1].Name.Indexed}";
			return description;
		}


		private void MoveAmbientToPortal(ReadyPortalModel portal, float volume)
		{
			_ambientSource.transform.position = portal.Position;
			PortalModel newPortal = new()
			{
				AudioSource = _ambientSource
			};
			_ambientSource = null;
			newPortal.AudioSource.maxDistance = GetMaxDistance();
			newPortal.AudioSource.name = GetPortalDescription(portal.Passage);
			_portals[portal.Passage] = newPortal;
		}

		protected ReadyPortalModel GetReadyPortalByPassage(List<ReadyPortalModel> portals, Passage passage)
		{
			ReadyPortalModel result = portals
				.First(loop => loop.Passage == passage);
			return result;
		}

		protected ReadyPortalModel RemoveReadyPortalNearPlayer(List<ReadyPortalModel> portals)
		{
			List<Passage> passages = ReadyPortalsToPassages(portals);
			Passage closestPassage = World.GetClosestElement(passages, World.Player) as Passage;
			ReadyPortalModel closestPortal = GetReadyPortalByPassage(portals, closestPassage);
			portals.Remove(closestPortal);
			return closestPortal;
		}

		private const int _enteringZoneOcclusionDuration = 1;

		protected bool ReplaceAmbientLoopWithNearestPortal()
		{
			PortalModel portal = TryRemovePortalNearPlayer();
			if (portal == null)
				return false;

			DisableOcclusion(portal.AudioSource, _enteringZoneOcclusionDuration);
			_ambientSource = portal.AudioSource;
			_ambientSource.transform.position = GetAmbientPosition();
			return true;
		}

		protected PortalModel TryRemovePortalNearPlayer()
		{
			if (_portals == null)
				return null;

			Passage closestPassage = World.GetClosestElement(_portals.Keys, World.Player) as Passage;
			PortalModel closestPortal = _portals[closestPassage];
			_portals.Remove(closestPassage);
			return closestPortal;
		}

		private void PlayAmbientFromExits()
		{
			// Portal ambients already playing
			if (_portals != null)
				return;

			_portals = new();
			Zone playersZone = World.Player.Zone;
			List<ReadyPortalModel> readyPortals = PreparePortals();

			/* 
			 * Enuse original ambient sound if already playing.
			 */
			if (_ambientSource != null && _ambientSource.isPlaying)
			{
				// place it into  the nearest exit.
				ReadyPortalModel closestPortal = RemoveReadyPortalNearPlayer(readyPortals);
				MoveAmbientToPortal(closestPortal, GetPortalVolume(closestPortal.Passage));
			}

			// Start playback in The remaining exits.
			foreach (ReadyPortalModel portal in readyPortals)
				PlayPortal(portal);
		}

		private const int _passageDistanceAttenuationThreshold = 5;

		private void UpdatePortalOcclusion(PortalModel portal, Passage exit, float? duration = null)
		{
			float finalDuration = duration != null ? duration.Value : GetPortalOcclusionDuration(exit);
			PassageState state = exit.State;
			bool farFromPlayer = exit.GetDistanceToPlayer() > _passageDistanceAttenuationThreshold;
			float lowPass = GetPortalOcclusionLowPass(exit, farFromPlayer);

			Sounds.SlideLowPass(portal.AudioSource, finalDuration, lowPass);
			bool playerBehindWall = !GetZoneNearPlayer().IsAccessible(World.Player.Zone);
			UpdatePortalVolume(portal, exit, finalDuration, playerBehindWall);
		}

		protected float GetPortalOcclusionLowPass(Passage exit, bool farFromPlayer = false)
		{
			if (exit.State is PassageState.Closed or PassageState.Locked)
				return farFromPlayer ? Sounds.OverWallLowpass : Sounds.OverClosedDoorLowpass;
			return farFromPlayer ? Sounds.OverClosedDoorLowpass : Sounds.OverOpenDoorLowpass;
		}

		protected float GetPortalOcclusionDuration(Passage exit)
		{
			if (exit.State is PassageState.Closed or PassageState.Locked)
				return _doorClosingOcclusionDuration;
			return _doorOpeningOcclusionDuration;
		}

		protected void UpdatePortalVolume(PortalModel portal, Passage exit, float duration, bool playerBehindWall = false)
		{
			float volume = Sounds.GetLinearRolloffAttenuation(
				transform.position,
				_ambientMinDistance,
				GetMaxDistance(),
				_defaultVolume
				);
			float finalVolume = volume * GetPortalVolumeCoefficient(exit);
			if (playerBehindWall)
				finalVolume *= _behindWallVolumeCoefficient;
			Sounds.SlideVolume(portal.AudioSource, duration, finalVolume);
		}

		protected float GetPortalVolumeCoefficient(Passage exit)
		{
			if (exit is Passage and not Door)
				return _portalVolumeCoefficient;

			return exit.State switch
			{
				PassageState.Closed => _portalClosedVolumeCoefficient,
				PassageState.Locked => _portalClosedVolumeCoefficient,
				_ => _portalOpenVolumeCoefficient
			};
		}

		private List<Passage> ReadyPortalsToPassages(List<ReadyPortalModel> portals)
		{
			List<Passage> result = portals
				.Select(loop => loop.Passage)
				.ToList();
			return result;
		}

		private void PlayPortal(ReadyPortalModel readyPortal)
		{
			string description = GetPortalDescription(readyPortal.Passage);
			string name = _sounds["loop"];
			PortalModel newPortal = new()
			{
				AudioSource = Sounds.Play(name, readyPortal.Position, 0, true, false, description: description)
			};
			SetAttenuation(newPortal.AudioSource);

			_portals[readyPortal.Passage] = newPortal;
		}

		public List<Passage> GetExitsFromZones()
		{
			List<Zone> zones = Zones;
			List<Passage> exits =
				zones.SelectMany(z => z.Exits)
				.ToList();
			return exits;
		}

		private List<ReadyPortalModel> PreparePortals()
		{
			Zone playersZone = World.Player.Zone;
			List<ReadyPortalModel> portals = new();
			List<Passage> exits = GetExitsFromZones();

			foreach (Passage exit in exits)
			{
				Vector3 position = GetPointForPortal(exit);
				portals.Add(new(exit, position, false));
			}

			return portals;
		}

		private float GetPortalVolume(Passage exit)
		{
			float volume = _defaultVolume;
			if (exit.State is PassageState.Closed or PassageState.Locked)
				volume = Sounds.GetOverClosedDoorVolume(volume);
			return volume;
		}

		private PortalModel TakeClosestPassageLoop()
		{
			Passage closest = _portals.Keys
				.OrderBy(p => p.Area.Value.GetDistanceFrom(World.Player.Area.Value))
				.FirstOrDefault();
			if (closest == null)
				return null;

			PortalModel loop = _portals[closest];
			_portals.Remove(closest);
			return loop;
		}

		private void StopPortals()
		{
			if (_portals == null)
				return;

			foreach (PortalModel loop in _portals.Values)
				Sounds.SlideVolume(loop.AudioSource, .5f, 0);

			_portals = null;
		}

		private void StopLoops()
		{
			Stop(ref _ambientSource);
			foreach (PortalModel loop in _portals.Values)
				Stop(ref loop.AudioSource);
			_portals = new();

			void Stop(ref AudioSource source)
			{
				if (source != null && source.isPlaying)
				{
					source.Stop();
					source = null;
				}
			}
		}

		private void StopAmbientSounds()
		{
			if (_ambientSource != null && _ambientSource.isPlaying)
			{
				Sounds.SlideVolume(_ambientSource, _defaultOcclusionDuration, 0);
				_ambientSource = null;
			}

			StopPortals();
		}

		[ProtoIgnore]
		private Dictionary<Passage, PortalModel> _portals;

		protected Vector3? _loopPositionBackup;
		protected const float _portalVolumeCoefficient = 0.8f;
		protected const float _portalClosedVolumeCoefficient = 0.3f;
		protected const float _portalOpenVolumeCoefficient = 0.5f;

		protected Vector3 GetPointForPortal(Passage exit)
		{
			Vector2 exitPoint = exit.GetClosestPointToPlayer();
			float distance = _area.Value.GetDistanceFrom(exitPoint);
			Zone myZone = GetZoneNearPlayer();

			Vector2 projectionPoint = myZone.Area.Value.GetAlignedPoint(exitPoint, distance, true).Value;
			float height = GetSoundHeight();
			Vector3 result = projectionPoint.ToVector3(height);
			return result;
		}

		protected void RestoreLoopPosition()
		{
			_ambientSource.transform.position = _loopPositionBackup.Value;
			_loopPositionBackup = null;
		}

		public ObstacleType DetectOcclusion()
		{
			Zone playersZone = World.Player.Zone;
			Vector2 closestPoint = GetClosestPointToPlayer();
			Zone myZone = World.GetZone(closestPoint);

			// If the item is in the same zone as the player, use raycasting.
			if (playersZone == myZone)
				return World.DetectOcclusion(this);

			// Player in a different zone. Simplify occlusion detection.
			bool neighbourZone = playersZone.IsNeighbour(myZone);
			bool accessibleZone = myZone.IsAccessible(playersZone);

			// Adjecting zones
			if (neighbourZone && accessibleZone)
				return ObstacleType.InDifferentZone;

			// Player in inadjecting inaccesible zone.
			return ObstacleType.Far;
		}

		private Passage GetPassageInFrontOfPlayer()
		{
			Zone myZone = GetZoneNearPlayer();
			return myZone.GetPassageInFront(World.Player.Area.Value.Center);
		}

		private const float _doorOpeningOcclusionDuration = 1;
		private const float _doorClosingOcclusionDuration = 1;

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
		protected AudioSource _ambientSource;

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
		public List<Zone> Zones => (from name in _zones
									select World.GetZone(name))
										  .ToList();

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
			PlayAmbient();
			UpdateAmbientSounds();
		}

		/// <summary>
		/// Processes the Collision message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnOrientationChanged(OrientationChanged message) => StopActionWhenPlayerMoves();

		/// <summary>
		/// Handles the DoorManipulated message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnDoorManipulated(DoorManipulated message)
		{
			if (_sounds["loop"] == null)
				return;

			Door door = message.Sender as Door;
			Zone myZone = GetZoneNearPlayer();
			if (!door.LeadsTo(myZone))
				return;
			if (IsPlayerHere())
				return;

			UpdatePortalOcclusion(_portals[door], door);
		}

		private bool IsPlayerHere()
		{
			return World.Player.Zone == GetZoneNearPlayer();
		}

		/// <summary>
		/// Stops sound loop of this object, if any.
		/// </summary>
		protected void StopLoop()
		{
			if (_ambientSource != null)
				Sounds.SlideVolume(_ambientSource, .5f, 0);
		}

		/// <summary>
		/// Determines if the object should be heart over walls and closed doors in other zones.
		/// </summary>
		protected bool _audibleOverWalls;

		private bool PlayerInSoundRadius
		{ get => GetDistanceToPlayer() <= GetMaxDistance(); }

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnCharacterMoved(CharacterMoved message)
		{
			if (message.Sender != World.Player)
				return;

			UpdateNavigatingSoundPosition();
			UpdateAmbientSounds();
			StopActionWhenPlayerMoves();
		}

		private const float _actionFadingDuration = .5f;

		/// <summary>
		/// Stops the action sound.
		/// </summary>
		protected void StopActionWhenPlayerMoves()
		{
			if (_stopWhenPlayerMoves && _actionAudio != null && _actionAudio.isPlaying)
				Sounds.SlideVolume(_actionAudio, _actionFadingDuration, 0);
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
		protected ObstacleType _lastOccludingObstacle;
		private const float _behindWallVolumeCoefficient = .5f;
		protected const float _intervalBetweenActions = .5f;
		protected const float _maxDistanceMargin = 10;
		protected const float _ambientMinDistance = .5f;
		protected const int _loopInitialFadeDuration = 2;
		protected const float _defaultOcclusionDuration = 1;
		private const AudioRolloffMode _ambientRollofMode = AudioRolloffMode.Linear;

		protected float GetMaxDistance()
		{
			Rectangle myZoneArea = GetZoneNearPlayer().Area.Value;
			float longerSide = Mathf.Max(myZoneArea.Height, myZoneArea.Width);
			return longerSide + _maxDistanceMargin;
		}

		/// <summary>
		/// Destroys the object.
		/// </summary>
		protected override void DestroyObject()
		{
			base.DestroyObject();

			if (_ambientSource.isPlaying)
				_ambientSource.Stop();

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
		private void OnGameReloaded() => UpdateAmbientSounds();

		/// <summary>
		/// Plays the sound loop of this object if there's any.
		/// </summary>
		/// <param name="attenuated">Determines if the sound of the object should be played over a wall or other obstacles.</param>
		protected void UpdateAmbientSounds()
		{
			//test
			return;

			if (string.IsNullOrEmpty(_sounds["loop"]))
				return;
			if (!PlayerInSoundRadius)
				return;

			ObstacleType obstacle = DetectOcclusion();
			UpdateOcclusion(obstacle);
		}

		protected bool IsInAudibleDistance()
			=> GetDistanceToPlayer() <= _ambientSource.maxDistance;

		protected void UpdateOcclusion(ObstacleType obstacle, float duration = _defaultOcclusionDuration, Door door = null)
		{
			if (_sounds["loop"] == null)
				return;

			if (_lastOccludingObstacle == obstacle && obstacle != default && obstacle != ObstacleType.InDifferentZone)
				return;
			ObstacleType lastObstacleBackup = _lastOccludingObstacle;
			_lastOccludingObstacle = obstacle;
			_muffled = obstacle != ObstacleType.None;

			if (obstacle == ObstacleType.InDifferentZone)
			{
				UpdatePortals();
				return;
			}
			if (obstacle == ObstacleType.Far)
			{
				StopAmbientSounds();
				return;
			}

			if (lastObstacleBackup == ObstacleType.InDifferentZone && obstacle is not ObstacleType.Far and not ObstacleType.InDifferentZone)
			{
				PlayAmbient();
			}

			if (obstacle is ObstacleType.Far or ObstacleType.InDifferentZone)
				return;

			if (_muffled)
			{
				AttenuationModel attenuation = GetAttenuationSettings(obstacle);
				SetAttenuation(_ambientSource, attenuation, duration);
			}
			else DisableOcclusion(_ambientSource, duration);
		}

		private Passage GetPassageInFrontPlayer()
		{
			Zone myZone = GetZoneNearPlayer();
			return myZone.GetPassageInFront(World.Player.Area.Value.Center);
		}

		protected AttenuationModel GetAttenuationSettings(ObstacleType obstacle)
		{
			return obstacle switch
			{
				ObstacleType.Wall => new(
					Sounds.OverWallLowpass,
					Sounds.GetOverWallVolume(_defaultVolume)
				),
				ObstacleType.ClosedDoor => new(
					Sounds.OverClosedDoorLowpass,
					Sounds.GetOverClosedDoorVolume(_defaultVolume)
				),
				ObstacleType.OpenDoor => new(
					null,
					Sounds.GetOverOpenDoorVolume(_defaultVolume)
				),
				ObstacleType.ItemOrCharacter => new(
					Sounds.OverObjectLowpass,
					Sounds.GetOverObjectVolume(_defaultVolume)
				),
				_ => new(
					null,
					0
				)
			};
		}

		private void DisableOcclusion(AudioSource source, float duration = _defaultOcclusionDuration)
		{
			SetAttenuation(source, new(22000, _defaultVolume), duration, true);
		}

		private void SetAttenuation(AudioSource source, AttenuationModel attenuationSetting, float duration, bool disableLowPassAfterwards = false)
		{
			float updatedVolume = Sounds.GetLinearRolloffAttenuation(source.transform.position, _ambientMinDistance, GetMaxDistance(), attenuationSetting.Volume);
			Sounds.SlideVolume(source, duration, updatedVolume, false);
			source.spatialBlend = attenuationSetting.SpatialBlend;

			if (attenuationSetting.LowPassFrequency != null)
			{
				Sounds.SlideLowPass(source, duration, attenuationSetting.LowPassFrequency.Value, disableLowPassAfterwards);
				return;
			}

			if (_muffled)
				Sounds.SlideLowPass(source, duration, 22000);
		}

		/// <summary>
		/// Plays sound loop of the object with sound attenuation.
		/// </summary>
		/// <param name="obstacle">Type of obstacle between player and this object</param>
		protected void PlayAmbient()
		{
			if (string.IsNullOrEmpty(_sounds["loop"]))
				return;

			Vector3 position = GetAmbientPosition();
			string description = GetAmbientDescription();

			if (ReplaceAmbientLoopWithNearestPortal())
			{
				StopPortals();
				return;
			}

			string name = _sounds["loop"];
			_ambientSource = Sounds.Play(name, position, 0, true, false, description: description);
			SetAttenuation(_ambientSource);
		}

		private Vector3 GetAmbientPosition()
		{
			return _area.Value.Center.ToVector3(GetSoundHeight());
		}

		private void SetAttenuation(AudioSource source)
		{
			source.maxDistance = GetMaxDistance();
			source.minDistance = _ambientMinDistance;
			source.rolloffMode = _ambientRollofMode;
		}

		private string GetAmbientDescription()
		{
			return $"loop for {Name.Indexed} item";
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