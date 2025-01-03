using Game.Entities.Characters.Chipotle;
using Game.Entities.Characters.Tuttle;
using Game.Entities.Items;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Models;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Controls movement of an NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(ChipotlePhysics))]
	[ProtoInclude(101, typeof(TuttlePhysics))]
	public class Physics : CharacterComponent
	{
		protected virtual Vector2 GetStepDirection()
		{
			return _orientation.UnitVector;
		}

		/// <summary>
		/// Represents the maximum distance in meters from which the NPC can manipulate items or characters.
		/// </summary>
		protected float _objectManipulationRadius = .5f;

		/// <summary>
		/// Represents the maximum distance in meters from which the NPC can manipulate the door.
		/// </summary>
		protected float _doorManipulationRadius = 1;

		/// <summary>
		/// Checks if there's an character or item standing before the character and returns it.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>The character or item standing before the this character or null</returns>
		protected Entity SomethingBefore(float radius)
		{
			Entity i = GetItemsAndCharactersBefore(radius).First();
			return i ?? CharacterBefore();
		}

		/// <summary>
		/// Checks if there's an character standing before the character and returns it.
		/// </summary>
		/// <returns>The character standing before the character or null</returns>
		protected Character CharacterBefore()
		{
			return World.GetCharacter(GetNextTile(1).position);
		}

		/// <summary>
		/// Finds items and characters standing before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of items and characters standing before the character.</returns>
		protected virtual IEnumerable<Entity> GetItemsAndCharactersBefore(float radius)
		{
			Vector2 direction = GetStepDirection();
			CollisionsModel collisions = World.DetectCollisionsOnTrack(Owner, direction, radius);
			return collisions == null || collisions.Obstacles == null
				? null
				: collisions.Obstacles
				.OfType<Entity>()
				.Select(o => o as Entity);
		}

		/// <summary>
		/// Retrieves characters before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of characters standing before the character.</returns>
		protected virtual IEnumerable<Character> GetCharactersBefore(float radius)
		{
			return
				GetItemsAndCharactersBefore(radius)
					?.OfType<Character>();
		}

		/// <summary>
		/// Radius at which the game instructs the player to walk closer to the item or character when picking it up, using it or exploring it.
		/// </summary>
		protected float _objectManipulationHelpRadius = 50;

		/// <summary>
		/// Retrieves usable items or characters before the NPC.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of items and characters standing before the character.</returns>
		protected virtual UsableObjectsModel GetUsableItemsAndCharactersBefore(float radius)
		{
			List<Entity> objectsBefore = GetItemsAndCharactersBefore(_objectManipulationHelpRadius)
				.ToList();

			// No objects in range
			if (objectsBefore.IsNullOrEmpty())
				return new();

			// Help variable
			List<Entity> reachable = objectsBefore
				.Where(o => World.IsInRange(o, Owner, _objectManipulationRadius))
				.ToList();

			// Return usable reachable objects.
			IEnumerable<Entity> usableReachable = reachable
				.Where(o => o.Usable);
			if (usableReachable.Any())
				return new(usableReachable.ToList(), UsableObjectsModel.ResultType.Success);

			// Report that there are only unusable items in front of the NPC.
			bool unusableOnly = reachable.Any() &&
								reachable
									.All(o => !o.Usable);

			if (unusableOnly)
				return new(null, UsableObjectsModel.ResultType.Unusable);

			// Usable but too far away
			IEnumerable<Entity> usableButFar = objectsBefore
				.Where(o => !reachable.Contains(o))
				.Where(o => o.Usable);

			return usableButFar.Any() ? new(null, UsableObjectsModel.ResultType.Far) : new();
		}

		/// <summary>
		/// Retrieves pickable items before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of items standing before the character.</returns>
		protected virtual PickableItemsModel GetPickableItemsBefore(float radius)
		{
			List<Item> items = GetItemsBefore(_objectManipulationHelpRadius).ToList();
			if (items.IsNullOrEmpty())
				return new();

			// Some items are before the NPC but they can't be picked.
			List<Item> pickableItems = items
				.Where(i => i.CanBePicked())
				.ToList();
			if (pickableItems.IsNullOrEmpty())
			{
				// If there are only decorative items in front of the character, we return the Unpickable result.
				return items.Any(i => i.Decorative)
					? new(null, PickableItemsModel.ResultType.Unpickable)
					: new(null, PickableItemsModel.ResultType.Unpickable);
			}

			// Check if there are any items that are reachable from distance of _objectManipulationRadius.
			return !pickableItems.Any(i => World.IsInRange(i, Owner, _objectManipulationRadius))
				? new(null, PickableItemsModel.ResultType.Unreachable)
				: new(pickableItems, PickableItemsModel.ResultType.Success);
		}

		/// <summary>
		/// Retrieves items before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of items standing before the character.</returns>
		protected virtual IEnumerable<Item> GetItemsBefore(float radius)
		{
			return
				GetItemsAndCharactersBefore(radius)
					?.OfType<Item>();
		}

		/// <summary>
		/// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
		/// </summary>
		/// <param name="step">The distance between the NPC and the required tile</param>
		/// <returns>A reference to an tile that lays in the specified distance and direction</returns>
		/// <see cref="Orientation"/>
		protected virtual Vector2 GetNextTile()
		{
			return _path.Dequeue();
		}

		/// <summary>
		/// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
		/// </summary>
		/// <param name="step">The distance between the NPC and the required tile</param>
		/// <returns>A reference to an tile that lays in the specified distance and direction</returns>
		/// <see cref="Orientation"/>
		protected (Vector2 position, Tile tile) GetNextTile(int step)
		{
			return GetNextTile(Orientation, step);
		}

		/// <summary>
		/// Returns a tile at the specified distance and direction.
		/// </summary>
		/// <param name="direction">The direction of the tile to be found</param>
		/// <param name="step">The distance between the NPC and the tile to be found</param>
		/// <returns>A reference to an tile that lays in the specified distance and direction</returns>
		protected (Vector2 position, Tile tile) GetNextTile(Orientation2D direction, int step)
		{
			Rectangle? target = new(_area.Value);
			target.Value.Move(direction, step);
			Vector2 result = target.Value.Center;
			result = new((float)Math.Round(result.x), (float)Math.Round(result.y));
			return (result, World.Map[result]);
		}

		/// <summary>
		/// Stores the current shortest track towards the Detective Chipotle NPC.
		/// </summary>
		protected Queue<Vector2> _path;

		/// <summary>
		/// Reference to a path finder instance
		/// </summary>

		/// <summary>
		/// Solves a situation when the NPC encounters an obstacle.
		/// </summary>
		/// <param name="obstacle">Nearest point of the obstacle</param>
		protected virtual bool DetectCollisions(Vector2 obstacle)
		{
			return true;
		}

		/// <summary>
		/// Performs one step in direction specified in the _startWalkMessage field.
		/// </summary>
		protected virtual void MakeStep()
		{
			_speed = GetSpeed();
			_walkTimer = 0;
		}

		/// <summary>
		/// Specifies the length of one step in milliseconds.
		/// </summary>
		[ProtoIgnore]
		protected int _walkTimer;

		/// <summary>
		/// Performs walk on a preplanned route.
		/// </summary>
		protected virtual void PerformWalk()
		{
			if (_walkTimer < _speed)
				_walkTimer += World.DeltaTime;
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();
			PerformWalk();
		}

		/// <summary>
		/// The goal that Tuttle is just going to.
		/// </summary>
		protected Vector2 _goal;

		/// <summary>
		/// Returns reference to the tile the NPC currently stands on.
		/// </summary>
		[ProtoIgnore]
		protected (Vector2 position, Tile tile) CurrentTile
			=> (_area.Value.Center, World.Map[_area.Value.Center]);

		protected float GetDistanceCoefficient(float distance)
		{
			if (distance < 5)
				return 2;

			if (distance < 10)
				return 1;

			return distance < 30 ? .5f : .2f;
		}

		/// <summary>
		/// Computes length of the next step of the NPC.
		/// </summary>
		protected virtual int GetSpeed()
		{
			return (int)(GetTerrainSpeed() * GetDistanceCoefficient(_path.Count));
		}

		/// <summary>
		/// Returns speed of the terrain on which the NPC stands.
		/// </summary>
		/// <returns></returns>
		protected int GetTerrainSpeed()
		{
			return _speeds[CurrentTile.tile.Terrain];
		}

		/// <summary>
		/// Contains walk speed settings for particullar terrain types.
		/// </summary>
		protected readonly Dictionary<TerrainType, int> _speeds = new()
		{
			[TerrainType.Grass] = 600,
			[TerrainType.Linoleum] = 540,
			[TerrainType.Carpet] = 570,
			[TerrainType.Gravel] = 523,
			[TerrainType.Asphalt] = 440,
			[TerrainType.Cobblestones] = 510,
			[TerrainType.Tiles] = 400,
			[TerrainType.Wood] = 570,
			[TerrainType.Mud] = 940,
			[TerrainType.Puddle] = 650,
			[TerrainType.Concrete] = 468,
			[TerrainType.Clay] = 460,
			[TerrainType.Bush] = 970
		};

		/// <summary>
		/// Defines start position of the NPC.
		/// </summary>
		public Rectangle? StartPosition { get; protected set; }

		/// <summary>
		/// Coordinates of the area currently occupied by the NPC
		/// </summary>
		protected Rectangle? _area;

		/// <summary>
		/// Stores reference to a locality in which the NPC is currently located.
		/// </summary>
		public Locality Locality => _area == null ? null : Owner.Locality;

		/// <summary>
		/// Current orientation of the NPC
		/// </summary>
		protected Orientation2D _orientation;

		/// <summary>
		/// Specifies the length of one step.
		/// </summary>
		[ProtoIgnore]
		protected int _speed;

		/// <summary>
		/// Length of Character's steps.
		/// </summary>
		protected float _stepLength = .5f;

		/// <summary>
		/// Specifies max radius for navigable objects enumeration.
		/// </summary>
		protected int _navigableObjectsRadius = 50;

		/// <summary>
		/// Current orientation of the NPC
		/// </summary>
		public Orientation2D Orientation => new(_orientation);

		/// <summary>
		/// Gets or sets the width of the character.
		/// </summary>
		public float Width { get; protected set; }

		/// <summary>
		/// Gets or sets the height of the character.
		/// </summary>
		public float Height { get; protected set; }

		/// <summary>
		/// Handles the Reloaded message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected virtual void OnGameReloaded(Reloaded message)
		{
			InnerMessage(new OrientationChanged(this, _orientation, _orientation, TurnType.None, true));
			InnerMessage(new PositionChanged(this, _area.Value, _area.Value, Locality, Locality, ObstacleType.None, true));
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case Reloaded gr: OnGameReloaded(gr); break;
				case SetPosition sp: OnSetPosition(sp); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Calculates an angle between the NPC and a specified point.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="b"></param>
		/// <param name="orientation"></param>
		/// <returns></returns>
		protected Angle GetAngle(Vector2 point)
		{
			return World.GetAngle(point, _area.Value.Center, _orientation);
		}

		/// <summary>
		/// Immediately changes position of the NPC.
		/// </summary>
		/// <param name="x">X coordinate of the target position</param>
		/// <param name="y">Y coordinate of the target position</param>
		/// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
		protected void JumpTo(float x, float y, bool silently = false)
		{
			JumpTo(new Vector2(x, y), silently);
		}

		/// <summary>
		/// Moves the character in the specified direction by distance defined in <see cref="_stepLength"/>.
		/// </summary>
		/// <param name="direction">The direction in which to move the object.</param>
		/// <param name="silently">Optional. Determines whether to move silently or not. Defaults to false.</param>
		public void Move(Vector2 direction, bool silently = false)
		{
			Rectangle target = new(_area.Value);
			target.Move(direction * _stepLength);
			JumpTo(target);
		}

		/// <summary>
		/// Represents the maximum distance from the wall that the NPC will evaluate as standing close to the wall.
		/// </summary>
		protected float _wallDistanceThreshold = 2;

		/// <summary>
		/// Stores directional vectors to walls that are close to the NPC.
		/// </summary>
		protected List<Vector2> _nearbyWalls;

		/// <summary>
		/// Immediately changes position of the NPC.
		/// </summary>
		/// <param name="target">Coordinates of the target position</param>
		/// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
		protected virtual void JumpTo(Vector2 target, bool silently = false)
		{
			JumpTo(new Rectangle(target, Width, Height), silently);
		}

		/// <summary>
		/// Immediately changes position of the NPC.
		/// </summary>
		/// <param name="target">coordinates of the target position</param>
		/// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
		protected void JumpTo(Rectangle target, bool silently = false)
		{
			Locality sourceLocality = Locality;
			Locality targetLocality = target.GetLocalities().First();
			Rectangle? sourcePosition = _area == null ? null : new Rectangle(_area.Value);

			_area = new(target);

			// If it isn't the player detect obstacles between him and this NPC.
			Character player = World.Player;
			ObstacleType obstacle = Owner != player ? World.DetectAcousticObstacles(_area.Value) : ObstacleType.None;

			// Announce changes
			PositionChanged changed = new(this, sourcePosition, target, sourceLocality, targetLocality, obstacle, silently);
			InnerMessage(changed);

			if (targetLocality != sourceLocality)
				InnerMessage(new LocalityChanged(this, sourceLocality, targetLocality));
		}

		/// <summary>
		/// Processes the SetPosition message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnSetPosition(SetPosition message)
		{
			JumpTo(message.Target, message.Silently);
		}

		/// <summary>
		/// Calculates the angle between the character and the target MapElement.
		/// </summary>
		/// <param name="target">Another map element</param>
		/// <returns>An angle in compass degrees</returns>
		public float GetAngle(MapElement target)
		{
			Vector2 targetPoint = target.Area.Value.GetClosestPoint(_area.Value.Center);
			Vector2 characterPosition = _area.Value.GetClosestPoint(targetPoint);
			Vector2 characterOrientation = _orientation.UnitVector;

			Vector2 targetDirection = targetPoint - characterPosition;

			// Normalizujeme směrový vektor k cíli
			targetDirection.Normalize();

			// Vypočítáme úhel mezi severem (0, -1) a směrem k cíli
			float angleToNorth = (float)Math.Atan2(-targetDirection.x, -targetDirection.y);

			// Převedeme úhel na stupně a upravíme pro kompasový formát
			float angleDegrees = angleToNorth * Mathf.Rad2Deg;
			angleDegrees = (angleDegrees + 360) % 360;

			// Vypočítáme úhel orientace postavy vzhledem k severu
			float characterAngle = (float)Math.Atan2(-characterOrientation.x, -characterOrientation.y);
			float characterAngleDegrees = characterAngle * Mathf.Rad2Deg;
			characterAngleDegrees = (characterAngleDegrees + 360) % 360;

			// Vypočítáme relativní úhel mezi orientací postavy a cílem
			float relativeAngle = (angleDegrees - characterAngleDegrees + 360) % 360;

			return relativeAngle;
		}
		/// <summary>
		/// Rounds the given number to the nearest 90.
		/// </summary>
		/// <param name="n">The number to round.</param>
		/// <returns>The rounded number.</returns>
		protected int RoundToNearest90(float n)
		{
			if (n is >= 315 or < 45)
				return 0;

			if (n < 135)
				return 90;

			return n < 225 ? 180 : 270;
		}

		/// <summary>
		/// Returns the nearest door in front of the NPC.
		/// </summary>
		/// <returns>A door</returns>
		protected Door GetDoorBefore()
		{
			float radius = _doorManipulationRadius + _area.Value.DistanceFromCenterToCorner;
			Vector2 direction = GetStepDirection();

			return
				World.GetNearestDoors(_area.Value.Center, radius)
					.FirstOrDefault(d => GetAngle(d) == 0);
		}

		/// <summary>
		/// Finds the manipulation point for a given map element.
		/// </summary>
		protected Vector2 FindManipulationPoint(MapElement element)
		{
			Vector2? point = element.Area.Value.FindAlignedPoint(_area.Value.Center);
			if (point == null)
				point = element.Area.Value.GetClosestPoint(_area.Value.Center);
			return point.Value;
		}
	}
}