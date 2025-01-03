﻿using Game.Audio;
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

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.VisualScripting;

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

		[ProtoIgnore]
		protected AudioSource _loopAudio;

		[ProtoIgnore]
		protected AudioSource _actionAudio;

		[ProtoIgnore]
		protected AudioSource _pickingAudio;

		[ProtoIgnore]
		protected AudioSource _placingAudio;

		[ProtoIgnore]
		protected AudioLowPassFilter _loopLowpass;

		/// <summary>
		/// React on placing on the ground.
		/// </summary>
		protected void Placed() => _placingAudio.Play();

		/// <summary>
		/// Backing field for Localities property.
		/// </summary>
		protected HashSet<string> _localities = new();

		/// <summary>
		/// Localities intersecting with this object.
		/// </summary>
		[ProtoIgnore]
		public IEnumerable<Locality> Localities
		{
			get
			{
				return
					from name in _localities
					select World.GetLocality(name);
			}
		}

		/// <summary>
		/// Finds all localities the object or the NPC intersects with and saves their names into _localities.
		/// </summary>
		protected void FindLocalities()
		{
			if (_area == null)
				return;

			IEnumerable<string> query =
				(from l in World.GetLocalities(_area.Value)
				 select l.Name.Indexed);
			_localities = new HashSet<string>(query);
		}


		/// <summary>
		/// Sets value of the Area property.
		/// </summary>
		/// <param name="value">A value assigned to the property</param>
		protected override void SetArea(Rectangle? value)
		{
			base.SetArea(value);

			// Find out which localities the object now intersects, then sends the appropriate messages to the concerning localities.
			Locality[] backup = Localities.ToArray();
			FindLocalities();

			// Inform concerning localities that the object disappeared.
			foreach (Locality l in backup)
			{
				if (!Localities.Contains(l))
					l.TakeMessage(new ObjectDisappearedFromLocality(this, this, l));
			}

			// Inform concerning localities that the object appeared.
			foreach (Locality l in Localities)
			{
				if (!backup.Contains(l))
					l.TakeMessage(new ObjectAppearedInLocality(this, this, l));
			}
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
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
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
			_sounds = new(StringComparer.OrdinalIgnoreCase)
			{
				{"collision", collisionSound ?? "MovCrashDefault" },
				{"action",actionSound },
				{"picking", pickingSound },
				{"placing", placingSound },
				{"loop", loopSound },
				{"passBy", "ObjectPassBy" }
			};
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

			// Add the object to intersecting localities
			foreach (Locality l in Localities)
				l.TakeMessage(new ObjectAppearedInLocality(this, this, l));

			// Play loop sound if any and if the player can hear it.
			UpdateLoop();
		}

		/// <summary>
		/// Processes the Collision message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnOrientationChanged(OrientationChanged message)
			=> WatchPlayersMovement();
		protected virtual void OnLocalityEntered(CharacterCameToLocality message)
			=> UpdateLoop();

		/// <summary>
		/// Checks if there's a direct path from this object to the player.
		/// </summary>
		/// <returns>True if there's a direct path from this object to the player</returns>
		protected ObstacleType GetObstacles()
		{
			Vector2 player = World.Player.Area.Value.Center;
			Vector2 me = _area.Value.GetClosestPoint(player);
			Rectangle path = new(me, player);

			return World.DetectObstacles(path);
		}

		/// <summary>
		/// Handles the DoorManipulated message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnDoorManipulated(DoorManipulated message)
			=> UpdateLoop();

		/// <summary>
		/// Stops sound loop of this object, if any.
		/// </summary>
		protected void StopLoop()
		{
			if (_loopAudio != null)
				Sounds.SlideVolume(_loopAudio, .5f, 0);
		}


		/// <summary>
		/// Determines if the object should be heart over walls and closed doors in other localities.
		/// </summary>
		protected bool _audibleOverWalls;

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnEntityMoved(CharacterMoved message)
		{
			if (message.Sender != World.Player)
				return;

			UpdateNavigatingSound();
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

		protected const float _intervalBetweenActions = .5f;

		/// <summary>
		/// Destroys the object.
		/// </summary>
		protected override void DestroyObject()
		{
			base.DestroyObject();

			if (_loopAudio.isPlaying)
				_loopAudio.Stop();

			// Inform localities that the object disappeared.
			foreach (Locality l in Localities)
				l.TakeMessage(new ObjectDisappearedFromLocality(this, this, l));
		}

		/// <summary>
		/// Processes the Collision message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnObjectsCollided(ObjectsCollided message)
		{
			float objectHeight = message.Object.gameObject.transform.localScale.y;
			float cameraHeight = Camera.main.transform.position.y;
			float height = objectHeight < cameraHeight ? objectHeight : cameraHeight;
			Vector3 position = new(message.Position.x, height, message.Position.y);
			Sounds.Play(_sounds["collision"], position);
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
				if (!_actionAudio.isPlaying)
					_actionAudio.Play();
			}
			// Play the sound if predefined amount of time has passed since the last use.
			else if (_quickActionsAllowed && Time.time - _lastUse > _intervalBetweenActions)
			{
				_lastUse = Time.time;
				Sounds.Play(_sounds["action"], message.ManipulationPoint, _defaultVolume);
			}

			UsedOnce = !Used;
			Used = true;
		}

		/// <summary>
		/// Handles the game reloaded message.
		/// </summary>
		private void OnGameReloaded()
			=> UpdateLoop();

		/// <summary>
		/// Plays the sound loop of this object if there's any.
		/// </summary>
		/// <param name="attenuated">Determines if the sound of the object should be played over a wall or other obstacles.</param>
		protected void UpdateLoop()
		{
			if (string.IsNullOrEmpty(_sounds["loop"]))
				return;

			ObstacleType obstacle = World.DetectAcousticObstacles(_area.Value);
			if (obstacle == ObstacleType.Far)
				StopLoop();
			else PlayLoop(obstacle);// != ObstacleType.Wall ? obstacle: ObstacleType.Object);
		}

		/// <summary>
		/// Plays sound loop of the object with sound attenuation.
		/// </summary>
		/// <param name="obstacle">Type of obstacle between player and this object</param>
		protected void PlayLoop(ObstacleType obstacle = ObstacleType.None)
		{
			return;
			// Start the loop if not playing.
			if (!_loopAudio.isPlaying)
			{
				_loopAudio.Play();
				_loopAudio.loop = true;
			}

			// Start the sound attenuation if needed.
			bool attenuate = obstacle != ObstacleType.None && obstacle != ObstacleType.IndirectPath; ;
			int cutoffFrequency = 0;
			float volume = _defaultVolume;

			switch (obstacle)
			{
				case ObstacleType.Wall: cutoffFrequency = Game.Audio.Sounds.OverWallLowpass; volume = _overWallVolume; break;
				case ObstacleType.Door: cutoffFrequency = Game.Audio.Sounds.OverDoorLowpass; volume = OverDoorVolume; break;
				case ObstacleType.Object: cutoffFrequency = Game.Audio.Sounds.OverObjectLowpass; volume = OverObjectVolume; break;
			}

			if (attenuate)
			{
				_loopLowpass.cutoffFrequency = cutoffFrequency;
				Sounds.SlideVolume(_loopAudio, .5f, volume);
			}
			else
			{
				// Turn of attenuation
				if (_muffled && !attenuate)
				{
					_loopLowpass.cutoffFrequency = 22000;
					Sounds.SlideVolume(_loopAudio, .5f, _defaultVolume);
				}
			}

			_muffled = attenuate;
		}

		/// <summary>
		/// Returns distance from this object to the player.
		/// </summary>
		/// <returns>Distance in meters</returns>
		protected float GetDistanceFromPlayer()
			=> World.GetDistance(GetClosestPointToPlayer(), World.Player.Area.Value.Center);

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case PlaceItem m: OnPlaceObject(m); break;
				case PickUpObject m: OnPickUpObject(m); break;
				case ReportPosition m: OnReportPosition(m); break;
				case OrientationChanged oc: OnOrientationChanged(oc); break;
				case CharacterCameToLocality le: OnLocalityEntered(le); break;
				case CharacterMoved em: OnEntityMoved(em); break;
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
		private void OnPlaceObject(PlaceItem message)
		{
			Rectangle? vacancy = FindVacancy(message.Position.Value);
			bool success = vacancy != null;

			if (success)
			{
				HeldBy = null;
				Area = vacancy;
				Placed();
			}

			(message.Sender as MessagingObject).TakeMessage(new PlaceObjectResult(this, success));
		}

		/// <summary>
		/// Tries to find a free spot on the ground to place the object on.
		/// </summary>
		/// <param name="lowerLeftCorner">A point that should intersect with the placed object</param>
		/// <returns>An area the object could be placed on</returns>
		protected Rectangle? FindVacancy(Vector2 lowerLeftCorner)
		{
			// Try horizontal and vertical position.
			List<Rectangle> positions = new();
			positions.Add(new(
				new(lowerLeftCorner.x, lowerLeftCorner.y + Dimensions.height),
				Dimensions.height,
				Dimensions.width
			));

			if (Dimensions.height != Dimensions.width)
			{
				positions.Add(new(
					new(lowerLeftCorner.x, lowerLeftCorner.y + Dimensions.width),
					Dimensions.height,
					Dimensions.width
				));
			}

			// Detect collisions
			foreach (Rectangle position in positions)
			{
				CollisionsModel result = World.DetectCollisions(World.Player, position);
				if (result.OutOfMap || result.Obstacles != null)
					return position;
			}

			return null;
		}

		/// <summary>
		/// Handles the PickUpObject message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		protected void OnPickUpObject(PickUpObject m)
		{
			PickUpObjectResult.ResultType result = CanBePicked() ? PickUpObjectResult.ResultType.Success : PickUpObjectResult.ResultType.Unpickable;

			if (result == PickUpObjectResult.ResultType.Success)
			{
				Picked();
				HeldBy = (Character)m.Sender;
				Area = null;
			}

			// Report the result.
			m.Sender.TakeMessage(new PickUpObjectResult(this, this, result));
		}

		/// <summary>
		/// Reacts on picking off the ground.
		/// </summary>
		protected virtual void Picked() => _pickingAudio.Play();

		/// <summary>
		/// Handles the ReportPosition message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		private void OnReportPosition(ReportPosition m)
			=> ReportPosition();
	}
}