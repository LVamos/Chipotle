using Game.Entities.Characters.Components.PhysicsComponent.Results;
using Game.Mapping.Saves;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Models;
using Game.PathFinding;
using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Item = Game.Entities.Items.Item;
using Message = Game.Messaging.Message;
using Random = System.Random;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Characters.Components.PhysicsComponent
{
	/// <summary>
	/// Controls movement of an NPC.
	/// </summary>

	public class Physics : CharacterComponent
	{
		public void Restore(ComponentSave save)
		{
			if (save is not PhysicsSave data)
				return;

			base.Restore(data);
			_state = data.State;
			_area = data.Area.ToRectangle();
			_goal = data.Goal.ToVector2();
			_inventory = new(data.Inventory);
			_maxObjectDistance = data.MaxObjectDistance;
			_minObjectDistance = data.MinObjectDistance;
			_navigableObjectsRadius = data.NavigableObjectsRadius;
			_nearbyWalls = data.NearbyWalls;
			_objectManipulationHelpRadius = data.ObjectManipulationHelpRadius;
			_orientation = new(data.Orientation.ToVector2());
			_path = data.Path.ToVector2Queue();
			_restartApproaching = data.RestartApproaching;
			_speed = data.Speed;
			_stepLength = data.StepLength;
			_targetPlayerDistance = data.TargetPlayerDistance;
			_wallDistanceThreshold = data.WallDistanceThreshold;
			Height = data.Height;
			Width = data.Width;
		}

		public PhysicsSave Export()
		{
			var save = base.Export().ToPhysicsSave();

			save.State = _state;
			save.Area = _area.Value.ToRectangleSave();
			save.Goal = _goal.ToVector2Save();
			save.Inventory = new(_inventory);
			save.MaxObjectDistance = _maxObjectDistance;
			save.MinObjectDistance = _minObjectDistance;
			save.NavigableObjectsRadius = _navigableObjectsRadius;
			save.NearbyWalls = _nearbyWalls;
			save.ObjectManipulationHelpRadius = _objectManipulationHelpRadius;
			save.Orientation = _orientation.UnitVector.ToVector2Save();
			save.Path = _path.ToVector2SaveQueue();
			save.RestartApproaching = _restartApproaching;
			save.Speed = _speed;
			save.StepLength = _stepLength;
			save.TargetPlayerDistance = _targetPlayerDistance;
			save.WallDistanceThreshold = _wallDistanceThreshold;
			save.Height = Height;
			save.Width = Width;

			return save;
		}

		public Vector2 Center { get => _area.Value.Center; }

		private void FixedUpdate()
		{
			if (GameManager.State == GameState.Playing)
				PerformWalk();
		}

		protected void LogOutOfMapAttempt(Vector2 position)
		{
			string title = "Mimo mapu";
			string name = Owner.Name.Inner;
			string positionMessage = $"Poslední vadlidní pozice: {position.GetString()}";

			Logger.LogError(title, name, positionMessage);
		}

		protected float _minObjectDistance = 2;
		protected float _maxObjectDistance = 4;

		protected void JumpNear(Rectangle area)
		{
			Vector2? target = FindFreePlacementsAroundArea(area, _minObjectDistance, _maxObjectDistance)
				.FirstOrDefault();
			if (target == null)
				throw new InvalidOperationException("No free placement found.");
			JumpTo(target.Value);
		}

		protected Vector2[] FindFreePlacementsAroundArea(Rectangle area, float minDistance, float maxDistance, bool sameZone = true)
		{
			float height = transform.localScale.z;
			float width = transform.localScale.x;

			Vector2[] points = World.Placements.GetFreePlacementsNear(new() { Owner }, area, height, width, minDistance, maxDistance, sameZone)
				.ToArray();

			return points;
		}

		/// <summary>
		/// Gets or sets the height of the character.
		/// </summary>
		public float Height { get; private set; } = .4f;

		/// <summary>
		/// Gets or sets the width of the character.
		/// </summary>
		public float Width { get; private set; } = .4f; //todo předělat

		/// <summary>
		/// Checks if the NPC is walking in the moment.
		/// </summary>
		protected bool _walking => _state is CharacterState.GoingToTarget or CharacterState.GoingToTargetAndWatchingPlayer or CharacterState.GoingToPlayer;

		/// <summary>
		/// Specifies the minimum allowed distance from the Detective Chipotle NPC.
		/// </summary>
		/// <remarks>Used when following the Detective Chipotle NPC</remarks>
		protected const int _minPlayerDistance = 4;

		/// <summary>
		/// Specifies the maximum allowed distance from the Detective Chipotle NPC.
		/// </summary>
		/// <remarks>Used when following the Detective Chipotle NPC</remarks>
		protected const int _maxPlayerDistance = 5;

		/// <summary>
		/// Specifies the maximum distance allowed for path finding.
		/// </summary>
		protected const int _maxPathFindingDistance = 60;
		private const int _tightZoneThreshold = 2;

		protected virtual Vector2 GetStepDirection() => _orientation.UnitVector;

		/// <summary>
		/// Checks if there's an character or item standing before the character and returns it.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>The character or item standing before the this character or null</returns>
		protected Entity SomethingBefore(float radius)
		{
			Entity i = GetEntitiesBefore(radius).First();
			return i ?? CharacterBefore();
		}

		/// <summary>
		/// Checks if there's an character standing before the character and returns it.
		/// </summary>
		/// <returns>The character standing before the character or null</returns>
		protected Character CharacterBefore() => World.GetCharacter(GetNextTile(1).position);

		/// <summary>
		/// Finds items and characters standing before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of items and characters standing before the character.</returns>
		protected virtual IEnumerable<Entity> GetEntitiesBefore(float radius)
		{
			Rectangle area = _area.Value;
			area.Extend(.5f);

			TrackCollisionParams parameters = new(
				direction: GetStepDirection(),
				length: radius,
				ignored: new() { Owner },
				area: area,
				mode: TrackStopMode.WholeTrack
				);
			Collisions collisions = World.Collisions.DetectOnTrack(parameters);

			if (collisions == null || collisions.Obstacles == null)
				return null;

			IEnumerable<Entity> obstacles = collisions.Obstacles
				.OfType<Entity>()
				.Cast<Entity>()
				.ToList();

			obstacles = obstacles
				.Where(o => GetAngle(o.Area.Value.GetClosestPoint(Center)) <= 45)
				.ToList();
			return obstacles.ToList();
		}

		/// <summary>
		/// Retrieves characters before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of characters standing before the character.</returns>
		protected virtual IEnumerable<Character> GetCharactersBefore(float radius)
		{
			return
				GetEntitiesBefore(radius)
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
		protected virtual Usables GetUsables(float radius, List<string> included = null)
		{
			List<MapElement> objects = GetEntitiesBefore(Settings.ObjectManipulationRadius)
				.Cast<MapElement>()
				?.ToList();
			Door door = GetDoorBefore(GetStepDirection(), true);
			if (door != null)
				objects.Add(door);

			// No objects in range
			if (objects.IsNullOrEmpty())
				return new();

			// Help variable
			List<MapElement> reachable = objects
				.Where(o => World.IsInRange(o, Owner, Settings.ObjectManipulationRadius))
				.ToList();

			// Return usable reachable objects.
			IEnumerable<MapElement> usableReachable = reachable
				.Where(o => o.Usable || (included != null && included.Contains(o.Name.Inner)));
			if (usableReachable.Any())
				return new(usableReachable.ToList(), UsablesResult.Success);

			// Report that there are only unusable items in front of the NPC.
			bool unusableOnly = reachable.Any() &&
								reachable
									.All(o => !o.Usable);

			if (unusableOnly)
				return new(null, UsablesResult.Unusable);

			return new();
		}

		/// <summary>
		/// Retrieves pickable items before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of items standing before the character.</returns>
		protected virtual Pickables GetPickables(float radius)
		{
			List<Items.Item> items = GetItemsBefore(Settings.ObjectManipulationRadius)?.ToList();

			if (items.IsNullOrEmpty())
				return new();

			// Some items are before the NPC but they can't be picked.
			List<Item> pickableItems = items
				.Where(i => i.CanBePicked())
				.ToList();
			if (pickableItems.IsNullOrEmpty())
			{
				// If there are only decorative items in front of the character, we return the Unpickable result.
				if (items.Any(i => i.Decorative))
					return new(null, PickablesResult.Unpickable);
				return new(null, PickablesResult.Unpickable);
			}

			// Check if there are any items that are reachable from distance of _objectManipulationRadius.
			return !pickableItems.Any(i => World.IsInRange(i, Owner, Settings.ObjectManipulationRadius))
				? new(null, PickablesResult.NothingFound)
				: new(pickableItems, PickablesResult.Success);
		}

		/// <summary>
		/// Retrieves items before the character.
		/// </summary>
		/// <param name="radius">The radius of the search</param>
		/// <returns>Enumeration of items standing before the character.</returns>
		protected virtual IEnumerable<Items.Item> GetItemsBefore(float radius)
		{
			return
				GetEntitiesBefore(radius)
					?.OfType<Item>();
		}

		/// <summary>
		/// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
		/// </summary>
		/// <param name="step">The distance between the NPC and the required tile</param>
		/// <returns>A reference to an tile that lays in the specified distance and direction</returns>
		/// <see cref="Orientation"/>
		protected virtual Vector2? GetNextPoint()
		{
			Vector2 point = default;
			if (_path.TryDequeue(out point))
				return point;
			return null;
		}

		/// <summary>
		/// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
		/// </summary>
		/// <param name="step">The distance between the NPC and the required tile</param>
		/// <returns>A reference to an tile that lays in the specified distance and direction</returns>
		/// <see cref="Orientation"/>
		protected (Vector2 position, Tile tile) GetNextTile(int step) => GetNextTile(Orientation, step);

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
		/// Specifies the length of one step in milliseconds.
		/// </summary>

		protected int _walkTimer;

		protected void UpdateWalkTimer()
		{
			if (_walkTimer < _speed)
				_walkTimer += (int)(1000 * Time.fixedDeltaTime);
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate() => base.GameUpdate();

		/// <summary>
		/// The goal that Tuttle is just going to.
		/// </summary>
		protected Vector2 _goal;

		/// <summary>
		/// Returns reference to the tile the NPC currently stands on.
		/// </summary>

		protected (Vector2 position, Tile tile) CurrentTile
			=> (_area.Value.Center, World.Map[_area.Value.Center]);

		protected float GetDistanceCoefficient(float distance)
		{
			if (distance < 5)
				return 3;
			if (distance < 10)
				return 2;
			if (distance < 30)
				return 1;

			return .6f;
		}

		/// <summary>
		/// Computes length of the next step of the NPC.
		/// </summary>
		protected virtual int GetSpeed()
		{
			if (_state == CharacterState.GoingToPlayer)
				return (int)(GetTerrainSpeed() * GetDistanceCoefficient(_path.Count));
			return GetTerrainSpeed();
		}

		/// <summary>
		/// Returns speed of the terrain on which the NPC stands.
		/// </summary>
		/// <returns></returns>
		protected int GetTerrainSpeed() => _speeds[CurrentTile.tile.Terrain];

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
			[TerrainType.Mud] = 740,
			[TerrainType.Puddle] = 450,
			[TerrainType.Concrete] = 468,
			[TerrainType.Clay] = 460,
			[TerrainType.Bush] = 970
		};

		/// <summary>
		/// Coordinates of the area currently occupied by the NPC
		/// </summary>
		protected Rectangle? _area;

		/// <summary>
		/// Stores reference to a zone in which the NPC is currently located.
		/// </summary>
		public Zone Zone
		{
			get => _area == null ? null : World.GetZone(Center);
		}

		/// <summary>
		/// Current orientation of the NPC
		/// </summary>
		protected Orientation2D _orientation;

		/// <summary>
		/// Specifies the length of one step.
		/// </summary>

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
		/// Handles the Reloaded message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected virtual void OnGameReloaded(Reloaded message)
		{
			InnerMessage(new OrientationChanged(this, _orientation, _orientation, TurnType.None, true));
			InnerMessage(new PositionChanged(this, _area.Value, _area.Value, Zone, Zone, ObstacleType.None, true));
			_random = new();

			// Try to find a new path to the player if necessary.
			if (_state == CharacterState.GoingToPlayer)
				FindNewPathToPlayer();
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case Hide h:
					OnHide(h);
					break;

				case Reveal m:
					OnReveal(m);
					break;

				case FollowPath m: OnFollowPath(m); break;
				case StopFollowingPlayer stf:
					OnStopFollowingPlayer(stf);
					break;

				case TryGoTo m:
					OnTryGoTo(m);
					break;

				case StartFollowingPlayer m:
					OnStartFollowingPlayer(m);
					break;

				case StateChanged m:
					OnStateChanged(m);
					break;

				case GotoPoint m:
					OnGotoPoint(m);
					break;

				case CharacterMoved em:
					OnCharacterMoved(em);
					break;

				case Reloaded m: OnGameReloaded(m); break;
				case SetPosition m: OnSetPosition(m); break;
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
		protected Angle GetAngle(Vector2 point) => World.GetAngle(point, _area.Value.Center, _orientation);

		/// <summary>
		/// Immediately changes position of the NPC.
		/// </summary>
		/// <param name="x">X coordinate of the target position</param>
		/// <param name="y">Y coordinate of the target position</param>
		/// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
		protected void JumpTo(float x, float y, bool silently = false) => JumpTo(new Vector2(x, y), silently);

		/// <summary>
		/// Moves the character in the specified direction by distance defined in <see cref="_stepLength"/>.
		/// </summary>
		/// <param name="direction">The direction in which to move the object.</param>
		/// <param name="silently">Optional. Determines whether to move silently or not. Defaults to false.</param>
		public virtual Vector2 Move(Vector2 direction, bool silently = false)
		{
			Rectangle target = new(_area.Value);
			target.Move(direction * _stepLength);
			JumpTo(target);
			return target.Center;
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
		/// Specifies final distance from the Detective Chipotle when in process of approaching to it.
		/// </summary>
		protected int _targetPlayerDistance;

		/// <summary>
		/// An instance of the Random number generator
		/// </summary>

		protected Random _random = new();

		/// <summary>
		/// Indicates what the NPC is doing in the moment.
		/// </summary>
		protected CharacterState _state = CharacterState.Waiting;

		/// <summary>
		/// Measures elapsed time from the last attempt of finding a path.
		/// </summary>
		protected int _finderTimer;

		/// <summary>
		/// Specifies if Tuttle should start approaching to player when a new path is found after
		/// walk was interrupted.
		/// </summary>
		protected bool _restartApproaching;

		/// <summary>
		/// Represents the inventory. The objects are stored as indexed names.
		/// </summary>
		protected HashSet<string> _inventory = new();

		/// <summary>
		/// Immediately changes position of the NPC.
		/// </summary>
		/// <param name="target">Coordinates of the target position</param>
		/// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
		protected virtual void JumpTo(Vector2 target, bool silently = false) => JumpTo(Rectangle.FromCenter(target, Height, Width), silently);

		/// <summary>
		/// Immediately changes position of the NPC.
		/// </summary>
		/// <param name="target">coordinates of the target position</param>
		/// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
		protected virtual void JumpTo(Rectangle target, bool silently = false)
		{
			Zone sourceZone = Zone;
			Zone targetZone = World.GetZone(target.Center);
			Rectangle? sourcePosition = _area;
			_area = target;

			// If it isn't the player detect obstacles between him and this NPC.
			Character player = World.Player;
			ObstacleType obstacle = ObstacleType.None;
			if (Owner != player && Owner.Area != null) // Ignore until the NPC is initialized.
				obstacle = World.Collisions.DetectOcclusion(Owner);

			// Announce changes
			PositionChanged changed = new(this, sourcePosition, target, sourceZone, targetZone, obstacle, silently);
			InnerMessage(changed);

			if (targetZone != sourceZone)
				InnerMessage(new ZoneChanged(this, sourceZone, targetZone));
			LogPosition();
		}

		protected void LogPosition()
		{
			string title = "Krok";
			string name = Owner.Name.Inner;

			string position = string.Empty;
			if (Owner.Area == null)
				position += ": pozice: nenastavena";
			else
				position += $"Pozice: {Owner.Area.Value.Center.GetString()}";

			string zone = string.Empty;
			if (Owner.Zone == null)
				zone = "Lokace: neznámá";
			else
				zone = $"Lokace: {Owner.Zone.Name.Inner}";

			string relativePosition = string.Empty;
			if (Owner.Zone != null)
				relativePosition = $"Relativní pozice: {Rectangle.GetRelativeCoordinates(Owner.Area.Value.Center)}";

			string objectsBefore = GetObjectsBeforeForLog();

			Logger.LogInfo(title, name, position, relativePosition, zone, objectsBefore);
		}

		protected string GetObjectsBeforeForLog()
		{
			string objectsBefore = null;
			if (Owner.Area != null)
			{
				IEnumerable<Entity> query = GetEntitiesBefore(Settings.ObjectManipulationRadius);
				if (!query.IsNullOrEmpty())
				{
					string[] objects = query
						.Select(e => e.Name.Inner)?
						.ToArray();
					objectsBefore = string.Join(',', objects);
					objectsBefore = $"Objekty před: {objects}";
				}
			}

			return objectsBefore;
		}

		/// <summary>
		/// Processes the SetPosition message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnSetPosition(SetPosition message)
		{
			Rectangle area = Rectangle.FromCenter(message.Target, Height, Width);
			JumpTo(area, message.Silently);
		}

		/// <summary>
		/// Calculates the angle between the character and the target MapElement.
		/// </summary>
		/// <param name="target">Another map element</param>
		/// <returns>An angle in compass degrees</returns>
		protected float GetAngle(Rectangle target, Vector2? direction = null)
		{
			Vector2 targetPoint = target.GetClosestPoint(Center);
			Vector2 characterPosition = _area.Value.Center;
			Vector2 orientation = direction != null ? direction.Value : _orientation.UnitVector;

			Vector2 targetDirection = targetPoint - characterPosition;

			// Normalizujeme směrový vektor k cíli
			targetDirection.Normalize();

			// Calculate the angle between north (0, -1) and toward the goal
			float angleToNorth = (float)Math.Atan2(-targetDirection.x, -targetDirection.y);

			// convert an angle to degrees and adjust for a compass format
			float angleDegrees = angleToNorth * Mathf.Rad2Deg;
			angleDegrees = (angleDegrees + 360) % 360;

			// Calculate the angle of the character orientation of the north
			float characterAngle = (float)Math.Atan2(-orientation.x, -orientation.y);
			float characterAngleDegrees = characterAngle * Mathf.Rad2Deg;
			characterAngleDegrees = (characterAngleDegrees + 360) % 360;

			// Calculate the relative angle between the character orientation and the target
			float relativeAngle = (angleDegrees - characterAngleDegrees + 360) % 360;
			if (relativeAngle < 0f)
				relativeAngle += 360f;
			if (relativeAngle >= 359.9999f)
				relativeAngle = 0f;

			return relativeAngle;
		}

		/// <summary>
		/// Rounds the given number to the nearest 90.
		/// </summary>
		/// <param name="n">The number to round.</param>
		/// <returns>The rounded number.</returns>
		protected int RoundToNearest90(float n) => n is >= 315 or < 45 ? 0 : n < 135 ? 90 : n < 225 ? 180 : 270;

		/// <summary>
		/// Returns the nearest door in front of the NPC.
		/// </summary>
		protected Door GetDoorBefore(Vector2? direction = null, bool inOneStep = false, Zone leadingTo = null)
		{
			float radius = inOneStep
				? _stepLength
				: Settings.DoorManipulationRadius + _area.Value.DistanceFromCenterToCorner;

			Vector2 finalDirection = direction ?? GetStepDirection();
			List<Door> doors = World.GetNearestDoors(Center, radius)
				.Where(d => AngleAllowed(d, finalDirection))
				.ToList();

			if (leadingTo != null)
				doors = doors.Where(d => d.LeadsTo(leadingTo))
							 .ToList();

			return doors.FirstOrDefault();

			bool AngleAllowed(Door door, Vector2 direction)
			{
				const float epsilon = 45; // epsilon for float angle comparison
				float angle = Math.Abs(GetAngle(door.Area.Value, direction));
				return angle < epsilon || angle >= 360 - epsilon;
			}
		}

		/// <summary>
		/// Finds the manipulation point for a given map element.
		/// </summary>
		protected Vector2 FindManipulationPoint(MapElement element)
		{
			// If the element is an item held by the NPC, use center of the NPC due to sound realism.
			if (element is Item i && i.HeldBy == Owner)
				return Center;

			// Find an alligned point near the NPC.
			Vector2? point = element.Area.Value.GetAlignedPoint(Center);
			if (point == null)
				point = element.Area.Value.GetClosestPoint(Center);
			return point.Value;
		}

		/// <summary>
		/// Performs walk on a preplanned route.
		/// </summary>
		protected virtual void PerformWalk()
		{
			UpdateWalkTimer();

			if (!_walking)
				return;

			if (_path.IsNullOrEmpty())
				StopWalk();
			else if (_walkTimer >= _speed)
				MakeStep();
		}

		/// <summary>
		/// Returns distance from this passage to the player.
		/// </summary>
		/// <returns>Distance in meters</returns>
		protected float GetDistanceToPlayer() => World.GetDistance(Owner, World.Player);

		/// <summary>
		/// Computes distance between the NPC and the current goal.
		/// </summary>
		/// <returns>Distance between the NPC and the current goal</returns>
		protected float GetDistanceToGoal() => World.GetDistance(_area.Value.Center, _goal);

		protected bool HasReachedTarget() => (_state != CharacterState.GoingToPlayer && _path.IsNullOrEmpty()) || HasReachedPlayer();

		protected bool HasReachedPlayer()
		{
			if (_state != CharacterState.GoingToPlayer)
				return false;

			float distance = GetDistanceToPlayer();
			return
				 distance <= _targetPlayerDistance
				&& _player.SameZone(Owner);
		}

		protected Character _player => World.Player;

		/// <summary>
		/// starts walking the shortest path to the player.
		/// </summary>
		protected void GoToPlayer()
		{
			// This is helpful in test mode if the character is set to follow the player right from the beginning. If the player isn't initialized yet the character will keep trying to approach him.
			if (_area == null || Owner.Zone == null || World.Player == null)
				return;

			Vector2 goal = _player.Center;
			_path = FindPath(goal);
			if (_path == null)
				return;

			_goal = goal;
			SetState(CharacterState.GoingToPlayer);
			_walkTimer = 0;
			_targetPlayerDistance = _random.Next(_minPlayerDistance, _maxPlayerDistance);
		}

		/// <summary>
		/// Repeats an attempt to find path to a goal specified in _goal field.
		/// </summary>
		protected void FindNewPathToPlayer()
		{
			if (_finderTimer < 30)
			{
				_finderTimer++;
				return;
			}

			_path = FindPath(_goal);

			if (_path == null)
				return;

			_restartApproaching = false;
			_finderTimer = 0;
			GoToPlayer();
		}

		/// <summary>
		/// Constructs path from the NPC to the specified goal avoiding all obstacles.
		/// </summary>
		/// <param name="goal">The target position</param>
		/// <returns>Queue with nodes leading to the target</returns>
		protected Queue<Vector2> FindPath(Vector2 goal, bool withStart = false, bool withGoal = true)
		{
			if (_area == null)
				return null;

			Vector2 start = _area.Value.Center;
			Vector2? goal1 = goal, goal2 = goal;
			Zone startZone = Zone;
			Zone targetZone = World.Map[goal].Zone;
			bool sameZone = startZone == targetZone; // Don't go to another zones if the goal is in the same zone.

			// Give it up if the goal is in another zone and the NPC can't go there.
			if (!sameZone)
			{
				if (!startZone.IsAccessible(targetZone))
					return null;

				Passage closestExit =
				targetZone.GetExitsTo(targetZone)
				.OrderBy(p => p.Area.Value.GetDistanceFrom(_area.Value))
				.FirstOrDefault();

				Vector2 center = closestExit.Center;
				IEnumerable<Vector2> points = closestExit.GetPointsOfZone(targetZone)
					.OrderBy(p => World.GetDistance(center, p));
				if (points.IsNullOrEmpty())
					return null;
				goal1 = points.First();
			}

			if (IsZoneTight(targetZone))
				return null;

			PathfindingParams parameters = new(
				start,
				goal1.Value,
				sameZone,
				withStart,
				withGoal,
				Owner
				);
			Queue<Vector2> path1 = PathFinder.FindPath(parameters);
			if (path1 == null)
				return null;

			if (!sameZone)
			{
				// Construct the rest of the path from the entrance to the goal.
				parameters = new(
					goal1.Value,
					goal2.Value,
					true,
					false,
					withGoal,
					Owner
					);
				Queue<Vector2> path2 = PathFinder.FindPath(parameters);
				if (path2 == null)
					return null;

				Queue<Vector2> finalPath = new(path1.Concat(path2));
				return finalPath;
			}

			return path1;
		}

		protected bool IsZoneTight(Zone zone)
		{
			float depth = zone.Area.Value.Height;
			float width = zone.Area.Value.Width;
			return depth <= _tightZoneThreshold || width <= _tightZoneThreshold;
		}

		private Vector2? FindPointBehindPassage(Zone targetZone)
		{
			Passage closestExit =
				targetZone.GetExitsTo(targetZone)
				.OrderBy(p => p.Area.Value.GetDistanceFrom(_area.Value))
				.FirstOrDefault();
			if (closestExit == null)
				return null;

			Rectangle targetArea = targetZone.Area.Value;
			HashSet<Vector2> exitPoints = closestExit.GetPointsOfZone(Zone);
			if (exitPoints.IsNullOrEmpty())
				return null;

			Vector2 exitPoint = exitPoints.FirstOrDefault();
			Vector2? target = targetArea.GetAlignedPoint(exitPoint, 2 * _area.Value.Size);
			return target;
		}

		/// <summary>
		/// sets state of the NPC and announces the change to other components.
		/// </summary>
		protected void SetState(CharacterState state, bool informComponents = true)
		{
			_state = state;
			if (informComponents)
				InnerMessage(new StateChanged(this, state));
		}

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnCharacterMoved(CharacterMoved message)
		{
			// Test if the component has been initialized.
			if (Owner.Area == null || _state != CharacterState.WatchingPlayer || message.Sender != _player)
				return;

			// Avoid too long paths.
			if (GetDistanceToPlayer() > _maxPathFindingDistance)
			{
				// Simply jump nearer.
				Vector2? target = FindSpotByPlayer()
					?? throw new InvalidOperationException("Tuttle couldn't get to Chipotle.");
				JumpTo(target.Value);
			}

			GoToPlayerIfTooFar();
		}

		private Vector2? FindSpotByPlayer()
		{
			Rectangle playerArea = _player.Area.Value;
			playerArea.Extend(_maxPlayerDistance);
			IEnumerable<Vector2> points = World.Placements.GetFreePlacements(new() { Owner }, playerArea, transform.localScale.z, transform.localScale.x);
			Vector2? target =
				points.OrderBy(p => World.GetDistance(p, playerArea.Center))
				.FirstOrDefault();
			return target;
		}

		/// <summary>
		/// Checks if the NPC isn't too far away from the player.
		/// </summary>
		protected void GoToPlayerIfTooFar()
		{
			float distance = GetDistanceToPlayer();
			if (
				_state == CharacterState.WatchingPlayer && _area != null
													 && (distance > _maxPlayerDistance || distance <= _maxPlayerDistance && !_player.SameZone(Owner))
			)
				GoToPlayer();
			else if (_state == CharacterState.GoingToPlayer && distance <= _targetPlayerDistance)
				StopWalk();
		}

		/// <summary>
		/// Handles the FollowPath message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnFollowPath(FollowPath message)
		{
			_path = message.Path;
			_goal = _path.Last();
			SetState(CharacterState.GoingToTarget);
		}

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			_orientation = new(0, 1);
			_area = Owner.Area;
			base.Activate();
		}

		protected void UseDoor(Door door)
		{
			Vector2 point = FindManipulationPoint(door);
			door.TakeMessage(new UseDoor(Owner, point));
			LogDoorUsage(door);
		}

		protected void LogDoorUsage(Door door)
		{
			string title = "Postava použila dveře";
			string name = Owner.Name.Inner;
			string doorDestination = door.AnotherZone(Zone).To;
			string doorName = $"Název dveří: {door.Name.Inner}";

			Logger.LogInfo(title, name, doorDestination, doorName);
		}

		/// <summary>
		/// Processes the GotoPoint message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnGotoPoint(GotoPoint message)
		{
			_path = FindPath(message.Goal);

			if (_path == null) // No path exists
				return;

			_goal = message.Goal;

			CharacterState state = message.WatchPlayer ? CharacterState.GoingToTargetAndWatchingPlayer : CharacterState.GoingToTarget;
			SetState(state);
		}

		/// <summary>
		/// Handles the CharacterStateChanged message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnStateChanged(StateChanged message) => SetState(message.State, false);

		/// <summary>
		/// Processes the StartFollowing message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnStartFollowingPlayer(StartFollowingPlayer message) => StartFollowingPlayer();

		/// <summary>
		/// starts walking after the Detective Chipotle NPC.
		/// </summary>
		protected void StartFollowingPlayer()
		{
			StopWalk();
			SetState(CharacterState.WatchingPlayer);
		}

		/// <summary>
		/// Stops following the Detective Chipotle NPC.
		/// </summary>
		protected void StopFollowingPlayer()
		{
			StopWalk();
			SetState(CharacterState.Waiting);
		}

		/// <summary>
		/// Processes the StopFollowing message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnStopFollowingPlayer(StopFollowingPlayer message) => StopFollowingPlayer();

		/// <summary>
		/// Handles the TryGoTo message.
		/// </summary>
		/// <param name="message"><The message to be processed/param>
		protected void OnTryGoTo(TryGoTo message)
		{
			if (message.Sender is not CharacterComponent)
				throw new InvalidOperationException("Message of type TryGoTo was sent to inappropriate object.");

			foreach (Vector2 point in message.Points)
			{
				Queue<Vector2> path = FindPath(point, false, true);
				if (path == null)
					continue;

				_path = path;
				_goal = point;

				CharacterState state = message.WatchPlayer ? CharacterState.GoingToTargetAndWatchingPlayer : CharacterState.GoingToTarget;
				SetState(state);
				return;
			}
		}

		/// <summary>
		/// Stops ongoing walk and deletes current preplanned route.
		/// </summary>
		protected virtual void StopWalk()
		{
			_path = null;
			_walkTimer = 0;

			if (_state is CharacterState.GoingToPlayer or CharacterState.GoingToTargetAndWatchingPlayer)
				SetState(CharacterState.WatchingPlayer);
			else
				SetState(CharacterState.Waiting);
		}

		/// <summary>
		/// Performs one step in the direction given by the current orientation of the entity.
		/// </summary>
		protected virtual void MakeStep()
		{
			_speed = GetSpeed();
			_walkTimer = 0;
			Vector2? point = GetNextPoint();
			if (point == null)
				return;

			Rectangle area = Rectangle.FromCenter(point.Value, Height, Width);
			if (DetectCollisions(area))
			{
				_path = FindPath(_goal, false);
				return;
			}

			JumpTo(area);

			if (HasReachedTarget())
				StopWalk();
		}

		/// <summary>
		/// Processes the Reveal message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnReveal(Reveal message)
		{
			_area = message.Location;
			StartFollowingPlayer();
		}

		/// <summary>
		/// Processes the Hide message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnHide(Hide message) => StopFollowingPlayer();
		/// <summary>
		/// Avoids an obstacle or walks through a door if any.
		/// </summary>
		/// <param name="area">The target coordinates</param>
		protected virtual bool DetectCollisions(Rectangle area)
		{
			CollisionParams parameters = new(
				new() { Owner, _player },
				area,
				true);
			Collisions collisions = World.Collisions.Detect(parameters);
			if (collisions.Obstacles == null && !collisions.OutOfMap)
				return false;

			object obstacle = collisions.Obstacles?.First();
			return !HandleCollisions(obstacle);
		}

		protected virtual bool HandleCollisions(object obstacle)
		{
			if (obstacle is Door door && door.State == PassageState.Closed)
			{
				UseDoor(door); // Open it and keep walking.
				return true;
			}

			return false;
		}

		/// <summary>
		/// Logs the change in orientation.
		/// </summary>
		/// <param name="initial">The initial orientation</param>
		/// <param name="target">The target orientation</param>
		/// <param name="delta">The change in orientation</param>
		protected void LogOrientationChange(Orientation2D initial, Orientation2D target, int delta)
		{
			string title = "Orientace změněna";
			string name = Owner.Name.Inner;
			string initialCardinalDirection = initial.Angle.GetCardinalDirection().GetDescription();
			string targetCardinalDirection = target.Angle.GetCardinalDirection().GetDescription();
			float initialCompassDegrees = ((float)initial.Angle.CompassDegrees).Round();
			float targetCompassDegrees = ((float)target.Angle.CompassDegrees).Round();
			string initialMessage = $"Původní orientace: {initialCardinalDirection} ({initialCompassDegrees})";
			string targetMessage = $"Nová orientace: {targetCardinalDirection} ({targetCompassDegrees})";
			string deltaMessage = $"Změna: {delta}°";
			string objectsBefore = GetObjectsBeforeForLog();

			Logger.LogInfo(title, name, initialMessage, targetMessage, deltaMessage, objectsBefore);
		}

		protected void LogDoorCollision(Door door)
		{
			string title = "Postava narazila do dveří";
			string name = $"Postava: {Owner.Name.Inner}";
			string doorDestination = door.AnotherZone(Zone).To;
			string doorName = $"Název dveří: {door.Name.Inner}";

			Logger.LogInfo(title, name, doorDestination, doorName);
		}

		protected void LogEntityCollision(Entity entity, Vector2 contactPoint)
		{
			string title = "Postava narazila do entity";
			string character = $"Postava: {Owner.Name.Inner}";
			string collidedEntity = $"Kolidovaná entita: {entity.Name.Inner}";
			string pointOfCollision = $"Bod srážky: {contactPoint.GetString()}";
			Logger.LogInfo(title, character, collidedEntity, pointOfCollision);
		}

		protected void LogItemUsage(Items.Item item, Vector2 point)
		{
			string title = "Postava použila objekt";
			string character = Owner.Name.Inner;
			string itemName = $"Objekt: {item?.Name.Inner}";
			string pointMessage = $"Bod: {point.GetString()}";

			Logger.LogInfo(title, character, itemName, pointMessage);
		}

		protected void LogItemUsedToTarget(Items.Item itemToUse, MapElement target, Vector2 point)
		{
			string title = "Postava použila objekt na objekt";
			string character = Owner.Name.Inner;
			string item1 = $"Objekt: {itemToUse.Name.Inner}";
			string item2 = $"Druhý objekt: {target.Name.Inner}";
			string pointMessage = $"Bod: {point.GetString()}";

			Logger.LogInfo(title, character, item1, item2, pointMessage);
		}

		protected void LogItemPickup(Item item, PickUpItemResult.ResultType result)
		{
			string title = "Postava se pokusila sebrat předmět";
			string itemName = item == null ? string.Empty : $"Objekt: {item.Name.Inner}";
			string resultDescription = $"Výsledek: {result}";

			Logger.LogInfo(title, itemName, resultDescription);
		}

		protected bool PlaceItemInFront(Item item)
		{
			if (!_inventory.Contains(item.Name.Inner))
				throw new InvalidOperationException($"Item {item.Name.Inner} isn't in inventary");

			Rectangle? place = FindVacancyForItem(item);
			if (place == null)
			{
				InnerMessage(new PlaceItemResult(this, item, false));
				return false;
			}

			_inventory.Remove(item.Name.Inner);
			InnerMessage(new PlaceItemResult(this, item, true));
			PlaceItem placeMessage = new(Owner, item, Owner, place);
			item.TakeMessage(placeMessage);
			return true;
		}
		/// <summary>
		/// Tries to find a free spot on the ground to place the object on.
		/// </summary>
		/// <param name="lowerLeftCorner">A point that should intersect with the placed object</param>
		/// <returns>An area the object could be placed on</returns>
		protected Rectangle? FindVacancyForItem(Item item)
		{
			float height = item.Dimensions.height;
			float width = item.Dimensions.width;
			List<Rectangle> positions = new()
			{
Rectangle.FromCenter(Center, height, width),
Rectangle.FromCenter(Center, width, height)
			};

			return FindVacancy(positions[0]) ?? FindVacancy(positions[1]);

			Rectangle? FindVacancy(Rectangle rectangle)
			{
				// Move the rectangle in front of the character
				float step = 0.1f;
				float distance;

				// Move the rectangle in front of the chracter.
				do
				{
					rectangle = rectangle.Move(_orientation.UnitVector, step);
					distance = rectangle.GetDistanceFrom(_area.Value);
				}
				while (distance < 0.1f);
				if (ValidatePosition(rectangle))
					return rectangle;

				// Try moving the rectangle to the left and to the right untill it fits.
				float offset = Mathf.Max(height, width) + .1f;
				int steps = Mathf.CeilToInt(offset / step) + 1;
				Vector2 sideDirection1 = Vector2.Perpendicular(_orientation.UnitVector);
				Vector2 sideDirection2 = new(_orientation.UnitVector.y, -_orientation.UnitVector.x);

				return TryLateralDirection(rectangle, sideDirection1, offset, steps, step)
								?? TryLateralDirection(rectangle, sideDirection2, offset, steps, step);

				bool ValidatePosition(Rectangle rectangle)
				{
					CollisionParams parameters = new(
						null,
						rectangle,
						true);
					Collisions result = World.Collisions.Detect(parameters);

					float compassAngle = GetAngle(rectangle);
					return compassAngle == 0 && !result.OutOfMap && result.Obstacles == null;
				}
			}

			Rectangle? TryLateralDirection(Rectangle rectangle, Vector2 direction, float offset, int steps, float step)
			{
				Rectangle tempRectangle = rectangle;
				for (int i = 0; i < steps; i++)
				{
					tempRectangle = tempRectangle.Move(direction, step);
					CollisionParams parameters = new(
						null,
						tempRectangle,
						true);
					Collisions result = World.Collisions.Detect(parameters);
					if (!result.OutOfMap && result.Obstacles == null)
						return rectangle;
				}
				return null;
			}
		}

		protected float GetPassageMargin()
		{
			float height = _area.Value.Height;
			float width = _area.Value.Width;
			return Mathf.Max(height, width) * .5f;
		}

		/// <summary>
		/// Returns text descriptions of the specified exits including distance and position.
		/// </summary>
		/// <returns>A string array</returns>
		protected List<NavigableExitInfo> GetNavigableExits()
		{
			List<Passage> exits =
				Zone.GetNearestExits(Center);
			List<NavigableExitInfo> info = exits
				.Select(e => GetNavigableExitInfo(e))
				.ToList();

			return info;
		}

		protected NavigableExitInfo GetNavigableExitInfo(Passage exit)
		{
			float distance = World.GetDistance(Owner, exit);
			float angle = GetAngle(exit.Area.Value);
			Zone targetZone = exit.AnotherZone(Zone);
			NavigableExitInfo info = new(distance, exit, angle, _stepLength, targetZone, Owner);
			return info;
		}

		protected NavigableCharacterInfo GetNavigableCharacterInfo(Character character)
		{
			float distance = World.GetDistance(Owner, character);
			float angle = GetAngle(character.Area.Value);
			NavigableCharacterInfo info = new(distance, character, angle, _stepLength, Owner);
			return info;
		}


		protected List<NavigableCharacterInfo> GetNavigableCharacters()
		{
			Zone playersZone = Zone;
			List<Character> characters = World.GetNearestCharacters(Center, _navigableObjectsRadius)
				.Where(c => c != Owner && c.IsInAccessibleZone(playersZone))
				.ToList();
			List<NavigableCharacterInfo> info = characters
				.Select(GetNavigableCharacterInfo)
				.ToList();
			return info;
		}

		protected NavigableItemInfo GetNavigableItemInfo(Item item)
		{
			string name = item.Name.Friendly;
			if (Settings.SayInnerItemNames)
				name += " " + item.Name.Inner;
			float distance = World.GetDistance(Owner, item);
			float angle = GetAngle(item.Area.Value);
			bool intersects = item.Area.Value.Intersects(Owner.Area.Value);

			NavigableItemInfo info = new(distance, item, angle, _stepLength, Owner, intersects);
			return info;
		}

		/// <summary>
		/// Returns information about all navigable objects from current zone in the specified radius around the NPC.
		/// </summary>
		/// <returns>Tuple with an object list and text descriptions including distance and position of each object</returns>
		protected List<NavigableItemInfo> GetNavigableItems()
		{
			List<Item> items = Zone.GetNearByItems(Center, _navigableObjectsRadius).ToList();
			return
				items
				.Select(GetNavigableItemInfo)
				.ToList();
		}
	}
}