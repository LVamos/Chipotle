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
	/// Represents a door between two zones.
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
		protected string _openingSound;

		/// <summary>
		/// Sound of the door being closed
		/// </summary>
		protected string _closingSound;

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
		/// <param name="zones">Two zones connected by the door</param>
		public void Initialize(Name name, PassageState state, Rectangle area, IEnumerable<string> zones, DoorType type = DoorType.Door)
		{
			base.Initialize(name, area, zones);
			TypeDescription = Type == DoorType.Door ? "dveře" : "vrata";
			_closingSound = "snd24";
			_lockedSound = null;
			_manipulationTimer = 0;
			_openingSound = "snd23";
			_pinchTimer = 0;


			State = state;
			Type = type;
			_sounds["hit"] = "KitchenDoorCrash";
			_sounds["rattle"] = "DoorKnobRattle";
			_defaultVolume = .2f;
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
			Character character = message.Sender as Character;
			float characterHeight = character.transform.localScale.y;
			Vector3 slamPoint = message.Point.ToVector3(characterHeight);
			PlayDoorHit(slamPoint);
			LogDoorHit(message.Sender as Character, message.Point);
		}

		private void LogDoorHit(Character character, Vector2 point)
		{
			string title = "Náraz do dveří";
			string name = $"Název: {Name.Indexed}";
			string characterMessage = $"Postava {character.Name.Indexed} v lokaci {character.Zone.Name.Indexed}";
			string type = $"typ dveří: {TypeDescription}";
			string zones = $"Lokace: {_zones[0]}, {_zones[1]}";

			Logger.LogInfo(title, name, zones, type, characterMessage);
		}

		private void PlayDoorHit(Vector3 point) => Sounds.Play(_sounds["hit"], point, _defaultVolume);

		/// <summary>
		/// Closes the door if possible
		/// </summary>
		protected void Close(object sender, Vector2 point)
		{
			State = PassageState.Closed;

			AnnounceManipulation();
			Play(_closingSound, sender as Character, point);
			LogClosing();
		}

		private void LogClosing()
		{
			string title = "Dveře zavřeny";
			string name = $"Název: {Name.Indexed}";
			string type = $"typ dveří: {TypeDescription}";

			Logger.LogInfo(title, name, type);
		}

		private void AnnounceManipulation()
		{
			DoorManipulated message = new(this);
			IEnumerable<Zone> accessibles = Zones.First().GetAccessibleZones().Concat(Zones.Last().GetAccessibleZones()).Distinct();
			foreach (Zone l in accessibles)
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
			ObstacleType obstacle;
			if (character != World.Player)
				obstacle = World.DetectOcclusion(character);
			else
				obstacle = ObstacleType.None;

			// Set attenuation parameters
			float volume = _defaultVolume;

			switch (obstacle)
			{
				case ObstacleType.Wall:
					volume = Sounds.GetOverWallVolume(_defaultVolume);
					break;
				case ObstacleType.ClosedDoor:
					volume = Sounds.GetOverClosedDoorVolume(_defaultVolume);
					break;
				case ObstacleType.ItemOrCharacter:
					volume = Sounds.GetOverObjectVolume(_defaultVolume); break;
			}

			// Play the sound
			Vector3 position = new(point.x, 1.5f, point.y);
			Sounds.Play(sound, position, volume);
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
			LogUsage(message.Sender, message.ManipulationPoint);

			// Prevent rapidly repeated actions
			if (_manipulationTimer < _manipulationTimeLimit)
				return;
			_manipulationTimer = 0;

			if (State == PassageState.Locked)
			{
				Rattle(message.Sender as Character, message.ManipulationPoint);
				return;
			}

			if (State == PassageState.Closed)
			{
				Open(message.Sender, message.ManipulationPoint);
				return;
			}

			// The door is open. Prevent closing it if player is standing in it.
			if (_area.Value.Contains(World.Player.Area.Value.Center))
			{
				Play(_sounds["hit"], message.Sender as Character, message.ManipulationPoint);
				return;
			}

			// The door is open. If there are some objects or entities blocking the door then inform them that they were slammed by the door and let the door open.
			Entity[] obstacles = GetObstacles()
				.Where(o => o != World.Player)
				.ToArray();
			if (obstacles.IsNullOrEmpty()) // No obstacles, close the door.
			{
				Close(message.Sender, message.ManipulationPoint);
				return;
			}

			// The door is blocked by some objects or entities. Pinch them if the time limit has expired.
			if (_pinchTimer < _pinchTimeLimit)
				return;

			PinchEntities(message.Sender as Character, obstacles);
		}

		private void PinchEntities(Character character, Entity[] entities)
		{
			_pinchTimer = 0;
			float characterHeight = character.transform.localScale.y;

			foreach (Entity entity in entities)
			{
				Vector2 center = entity.Area.Value.Center;
				Vector3 slamPoint = center.ToVector3(characterHeight);
				Play(_sounds["hit"], character, slamPoint);

				PinchedInDoor pMessage = new(this, character);
				entity.TakeMessage(pMessage);
			}
		}

		protected string GetStateDescription()
		{
			return State switch
			{
				PassageState.Closed => "zavřené",
				PassageState.Open => "otevřené",
				PassageState.Locked => "zamčené",
				_ => "neznámý stav"
			};
		}

		protected void LogUsage(Character sender, Vector2 manipulationPoint)
		{
			string title = "Dveře reagují na použití";
			string doorName = $"Název: {Name.Indexed}";
			string characterName = $"Postava: {sender.Name.Indexed}";
			string doorType = $"typ dveří: {TypeDescription}";
			string doorState = $"Stav dveří: {GetStateDescription()}";
			string point = $"Bod: {manipulationPoint.GetString()}";

			Logger.LogInfo(title, doorName, characterName, doorType, doorState, point);
		}

		protected void Rattle(Character character, Vector2 manipulationPoint)
		{
			Play(_sounds["rattle"], character, manipulationPoint);
			LogRattling();
		}

		private void LogRattling()
		{
			string title = "Lomcování dveřmi";
			string name = $"Název: {Name.Indexed}";
			string type = $"typ dveří: {TypeDescription}";

			Logger.LogInfo(title, name, type);
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
			LogOpening();
		}

		private void LogOpening()
		{
			string title = "Dveře otevřeny";
			string name = $"Název: {Name.Indexed}";
			string type = $"typ dveří: {TypeDescription}";

			Logger.LogInfo(title, name, type);
		}
	}
}