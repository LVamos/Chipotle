using Game.Audio;
using Game.Entities;
using Game.Entities.Characters;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.Physics;

using ProtoBuf;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a door between two localities.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(HallDoor))]
	[ProtoInclude(101, typeof(MariottisDoor))]
	[ProtoInclude(102, typeof(SlidingDoor))]
	[ProtoInclude(103, typeof(VanillaCrunchGarageDoor))]
	public class Door : Passage
	{
		/// <summary>
		/// Describes type of a door.
		/// </summary>
		public enum DoorType
		{
			Door,
			Gate
		}

		/// <summary>
		/// Describes type of the door.
		/// </summary>
		public DoorType Type { get; protected set; }

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();
			WatchTimers();
		}

		/// <summary>
		/// Watches and manages timers for door manipulation.
		/// </summary>
		private void WatchTimers()
		{
			if (_manipulationTimer < _manipulationTimeLimit)
				_manipulationTimer += World.DeltaTime;
			if (_pinchTimer < _pinchTimeLimit)
				_pinchTimer += World.DeltaTime;
		}

		/// <summary>
		/// Conts time from last opening / closing.
		/// </summary>
		protected int _manipulationTimer = _manipulationTimeLimit;

		/// <summary>
		/// Conts time from last attempt to pinch an object or entity in the door.
		/// </summary>
		protected int _pinchTimer = _pinchTimeLimit;

		/// <summary>
		/// Sound of the door being opened
		/// </summary>
		protected string _openingSound = "snd23";

		/// <summary>
		/// Sound of the door being closed
		/// </summary>
		protected string _closingSound = "snd24";

		/// <summary>
		/// Sound of the door being opened
		/// </summary>
		protected string _lockedSound;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner name for the door</param>
		/// <param name="closed">Specifies whether the door should be implicitly closed or open</param>
		/// <param name="area">Location of the door</param>
		/// <param name="localities">Two localities connected by the door</param>
		public void Initialize(Name name, PassageState state, Rectangle area, IEnumerable<string> localities, DoorType type = DoorType.Door)
		{
			base.Initialize(name, area, localities);

			State = state;
			Type = type;
			_sounds["hit"] = "KitchenDoorCrash";
			_sounds["rattle"] = "DoorKnobRattle";
			_defaultVolume = .5f;
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case DoorHit dh: OnDoorHit(dh); break;
				case UseDoor m: OnUseDoor(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		///  Handles the DoorHit message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected virtual void OnDoorHit(DoorHit message)
		{
			float cameraHeight = Camera.main.transform.localScale.y;
			Vector3 position = new(message.Point.x, cameraHeight, message.Point.y);
			Sounds.Play(_sounds["hit"], position, _defaultVolume);
		}

		/// <summary>
		/// Closes the door if possible
		/// </summary>
		protected void Close(object sender, Vector2 point)
		{
			if (Area.Value.Walkable())
			{
				State = PassageState.Closed;

				AnnounceManipulation();
				Play(_closingSound, sender as Character, point);
			}
		}

		private void AnnounceManipulation()
		{
			DoorManipulated message = new(this);
			IEnumerable<Locality> accessibles = Localities.First().GetAccessibleLocalities().Concat(Localities.Last().GetAccessibleLocalities()).Distinct();
			foreach (Locality l in accessibles)
				l.TakeMessage(message);
		}

		/// <summary>
		/// Plays the specified sound.
		/// </summary>
		/// <param name="sound">Name of the sound to be played</param>
		/// <param name="position"></param>
		/// <param name="obstacle">Describes type of obstacle between the entity and the player if any.</param>
		protected void Play(string sound, Character character, Vector2 point)
		{
			// Set attenuation parameters
			ObstacleType obstacle = character != World.Player ? World.DetectAcousticObstacles(character.Area.Value) : ObstacleType.None;

			if (obstacle == ObstacleType.Far)
			{
				Locality l = World.Player.Locality;
				if (Localities.First().IsBehindDoor(l) || Localities.Last().IsBehindDoor(l)) // Can be heart from the adjecting locality
					obstacle = ObstacleType.Wall;
				else
					return; // Too far and inaudible
			}

			// Set attenuation parameters
			float volume = _defaultVolume;

			switch (obstacle)
			{
				case ObstacleType.Wall:
					volume = Sounds.GetOverWallVolume(_defaultVolume);
					break;
				case ObstacleType.Door:
					volume = Sounds.GetOverDoorVolume(_defaultVolume);
					break;
				case ObstacleType.Object:
					volume = Sounds.GetOverObjectVolume(_defaultVolume); break;
			}

			// Play the sound
			Vector3 position3d = new(point.x, 1.5f, point.y);
			Sounds.Play(sound, position3d, volume);
		}

		/// <summary>
		/// Enumerates objects and entities stand ing near the door.
		/// </summary>
		/// <returns>Enumeration of objects and entities</returns>
		protected IEnumerable<Entity> GetObstacles()
		{
			Rectangle surroundings = Area.Value; // Just copied
			surroundings.Extend();
			IEnumerable<Entity> obstacles = surroundings.GetEntities()
				.Union(_area.Value.GetObjects());
			return obstacles;
		}

		/// <summary>
		/// A time interval in milliseconds between opening and closing.
		/// </summary>
		protected const int _manipulationTimeLimit = 800;

		/// <summary>
		/// A time interval in milliseconds between pinching an object or entity in the door.
		/// </summary>
		protected const int _pinchTimeLimit = 2500;

		/// <summary>
		/// Processes the UseObject message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnUseDoor(UseDoor message)
		{
			if (State == PassageState.Locked)
			{
				Rattle(message.Sender as Character, message.ManipulationPoint);
				return;
			}

			if (State == PassageState.Closed && _manipulationTimer >= _manipulationTimeLimit)
			{
				_manipulationTimer = 0;
				Open(message.Sender, message.ManipulationPoint);
				return;
			}

			// The door is open. Prevent closing it if player is standing in it.
			if (_area.Value.Intersects(World.Player.Area.Value.Center))
			{
				if (_manipulationTimer >= _manipulationTimeLimit)
				{
					_manipulationTimer = 0;
					Play(_sounds["hit"], message.Sender as Character, message.ManipulationPoint);
				}
				return;
			}

			// The door is open. If there are some objects or entities blocking the door then inform them that they were slammed by the door and let the door open.
			IEnumerable<Entity> obstacles = GetObstacles().Where(o => o != World.Player);
			if (obstacles.IsNullOrEmpty()) // No obstacles, close the door.
			{
				if (_manipulationTimer >= _manipulationTimeLimit)
				{
					_manipulationTimer = 0;
					Close(message.Sender, message.ManipulationPoint);
				}
				return;
			}

			// The door is blocked by some objects or entities. Pinch them if the time limit has expired.
			if (_pinchTimer < _pinchTimeLimit)
				return;

			// Play a slamming sound at the nearest entity or object
			_pinchTimer = 0;

			Vector3 slamPoint = obstacles.OrderBy(o => o.Area.Value.GetDistanceFrom(_area.Value.Center)).First().Area.Value.Center;
			Play(_sounds["hit"], message.Sender as Character, slamPoint);

			// Inform the obstacles without closing the door.

			foreach (Entity o in obstacles)
			{
				PinchedInDoor pMessage = new(this, (Character)message.Sender);
				o.TakeMessage(pMessage);
			}
		}

		protected void Rattle(Character character, Vector2 manipulationPoint)
		{
			Play(_sounds["rattle"], character, manipulationPoint);
		}

		/// <summary>
		/// Opens the door if possible.
		/// </summary>
		/// <param name="position">
		/// The coordinates of the place on the door that an NPC is pushing on
		/// </param>
		protected virtual void Open(object sender, Vector2 point)
		{
			State = PassageState.Open;

			AnnounceManipulation();
			Play(_openingSound, sender as Character, point);
		}

		/// <summary>
		/// Returns text description of the door.
		/// </summary>
		/// <returns>text description of the door</returns>
		public override string ToString()
		{
			return Type == DoorType.Door ? "dveře" : "vrata";
		}
	}
}