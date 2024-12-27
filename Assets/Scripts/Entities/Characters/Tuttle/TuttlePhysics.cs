using Game.Entities.Characters.Components;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;
using Physics = Game.Entities.Characters.Components.Physics;
using Random = System.Random;

namespace Game.Entities.Characters.Tuttle
{
	/// <summary>
	/// Controls movement of the Tuttle NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class TuttlePhysics : Physics
	{
		/// <summary>
		/// sets state of the NPC and announces the change to other components.
		/// </summary>
		private void SetState(TuttleState state)
		{
			_state = state;
			InnerMessage(new TuttleStateChanged(this, state));
		}

		/// <summary>
		/// Indicates what the NPC is doing in the moment.
		/// </summary>
		protected TuttleState _state = TuttleState.Waiting;

		/// <summary>
		/// Constructs path from the NPC to the specified goal avoiding all obstacles.
		/// </summary>
		/// <param name="goal">The target position</param>
		/// <returns>Queue with nodes leading to the target</returns>
		protected Queue<Vector2> FindPath(Vector2 goal, bool throughGoal = true)
		{
			if (_area == null)
				return null;

			bool sameLocality = World.GetLocality(_area.Value.Center) == World.GetLocality(goal); // Don't go to another localities if the goal is in the same locality.
			bool throughDoors = !sameLocality;

			return World.FindPath(_area.Value.Center, goal, false, throughDoors, false, sameLocality, true, throughGoal);
		}

		/// <summary>
		/// Specifies the maximum allowed distance from the Detective Chipotle NPC.
		/// </summary>
		/// <remarks>Used when following the Detective Chipotle NPC</remarks>
		private const int _maxDistanceFromPlayer = 10;

		/// <summary>
		/// Specifies the minimum allowed distance from the Detective Chipotle NPC.
		/// </summary>
		/// <remarks>Used when following the Detective Chipotle NPC</remarks>
		private const int _minDistanceFromPlayer = 6;

		/// <summary>
		/// An instance of the Random number generator
		/// </summary>
		[ProtoIgnore]
		private Random _random = new();

		/// <summary>
		/// Specifies final distance from the Detective Chipotle when in process of approaching to it.
		/// </summary>
		private int _targetDistance;

		/// <summary>
		/// Measures elapsed time from the last attempt of finding a path.
		/// </summary>
		private int _finderTimer;

		/// <summary>
		/// Reference to the Detective Chipotle NPC
		/// </summary>
		[ProtoIgnore]
		private Character _player => World.Player;

		/// <summary>
		/// Specifies if Tuttle should start approaching to player when a new path is found after
		/// walk was interrupted.
		/// </summary>
		private bool _restartApproaching;

		/// <summary>
		/// Specifies the maximum distance allowed for path finding.
		/// </summary>
		private const int _maxPathFindingDistance = 60;

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			_orientation = new(0, 1);
			base.Activate();
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case TryGoTo m:
					OnTryGoTo(m);
					break;

				case FollowPath m:
					OnFollowPath(m);
					break;

				case Reloaded m:
					OnGameReloaded(m);
					break;

				case TuttleStateChanged m:
					OnTuttleStateChanged(m);
					break;

				case Reveal m:
					OnReveal(m);
					break;

				case Hide h:
					OnHide(h);
					break;

				case GotoPoint gp:
					OnGotoPoint(gp);
					break;

				case StartFollowing sf:
					OnStartFollowing(sf);
					break;

				case StopFollowing stf:
					OnStopFollowing(stf);
					break;

				case CharacterMoved em:
					OnEntityMoved(em);
					break;

				default:
					base.HandleMessage(message);
					break;
			}
		}

		/// <summary>
		/// Handles the TryGoTo message.
		/// </summary>
		/// <param name="m"><The message to be processed/param>
		private void OnTryGoTo(TryGoTo m)
		{
			if (!(m.Sender is CharacterComponent))
				throw new InvalidOperationException("Message of type TryGoTo was sent to inappropriate object.");

			foreach (Vector2 point in m.Points)
			{
				Queue<Vector2> path = FindPath(point);
				if (path != null)
				{
					_path = path;
					_goal = point;
					SetState(m.WatchPlayer ? TuttleState.GoingToTargetAndWatchingPlayer : TuttleState.GoingToTarget);
					break;
				}
			}
		}

		/// <summary>
		/// Handles the FollowPath message.
		/// </summary>
		/// <param name="m">The message to be processed</param>
		private void OnFollowPath(FollowPath m)
		{
			_path = m.Path;
			_goal = _path.Last();
			SetState(TuttleState.GoingToTarget);
		}

		/// <summary>
		/// Handles the Reloaded message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void OnGameReloaded(Reloaded message)
		{
			base.OnGameReloaded(message);
			_random = new();

			// Try to find a new path to the player if necessary.
			if (_state == TuttleState.GoingToPlayer)
				FindNewPath();
		}

		/// <summary>
		/// Handles the TuttleStateChanged message.
		/// </summary>
		/// <param name="message">The message</param>
		private void OnTuttleStateChanged(TuttleStateChanged message)
			=> _state = message.State;

		/// <summary>
		/// Repeats an attempt to find path to a goal specified in _goal field.
		/// </summary>
		private void FindNewPath()
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
		/// finds a walkable tile near the Detective Chipotle NPC. Prefers the locality in which the player is located.
		/// </summary>
		/// <returns>Coordinates of tthe target tile</returns>
		private Vector2? GetSpotByPlayer()
			=> (from p in _player.Area.Value.GetWalkableSurroundingPoints(_minDistanceFromPlayer, _maxDistanceFromPlayer)
				let distance = World.GetDistance(_area.Value.Center, p)
				orderby distance
				select p)
				.FirstOrDefault();

		/// <summary>
		/// Computes distance between the NPC and the Detective Chipotle NPC.
		/// </summary>
		/// <returns>Distance between the NPC and the Detective Chipotle NPC</returns>
		private float GetDistanceFromPlayer()
			=> World.GetDistance(_area.Value.Center, _player.Area.Value.Center);

		/// <summary>
		/// Computes distance between the NPC and the current goal.
		/// </summary>
		/// <returns>Distance between the NPC and the current goal</returns>
		private float GetDistanceFromGoal()
			=> World.GetDistance(_area.Value.Center, _goal);

		/// <summary>
		/// start walking the shortest path to the Detective Chipotle NPC.
		/// </summary>
		private void GoToPlayer()
		{
			// This is helpful in test mode if the Tuttle is set to follow the player right from the beginning. If the Chipotle isn't initialized yet Tuttle will keep trying to approach him.
			if (_area == null || Owner.Locality == null || World.Player == null)
				return;

			_path = FindPath(_player.Area.Value.Center);

			if (_path == null)
				return;

			_goal = _player.Area.Value.Center;
			SetState(TuttleState.GoingToPlayer);
			_walkTimer = 0;
			_targetDistance = _random.Next(_minDistanceFromPlayer, _maxDistanceFromPlayer);
		}

		/// <summary>
		/// Checks if the NPC isn't too far away from the Detective Chipotle NPC.
		/// </summary>
		private void CheckDistance()
		{
			float distance = GetDistanceFromPlayer();
			if (
				_state == TuttleState.WatchingPlayer && _area != null
													 && (distance > _maxDistanceFromPlayer || distance <= _maxDistanceFromPlayer && !_player.SameLocality(Owner))
			)
				GoToPlayer();
			else if (_state == TuttleState.GoingToPlayer && distance <= _targetDistance)
				StopWalk();
		}

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnEntityMoved(CharacterMoved message)
		{
			// Test if the component has been initialized.
			if (_area == null || _state != TuttleState.WatchingPlayer || message.Sender != _player)
				return;

			// Avoid too long paths.
			if (GetDistanceFromPlayer() > _maxPathFindingDistance)
			{
				// Simply jump nearer.
				Vector2? nearerSpot =
					(from p in _player.Area.Value.GetWalkableSurroundingPoints(_maxDistanceFromPlayer, _maxPathFindingDistance)
					 let distance = World.GetDistance(_area.Value.Center, p)
					 orderby distance
					 select p)
					.FirstOrDefault();

				if (nearerSpot.HasValue)
					JumpTo(nearerSpot.Value);
				else throw new InvalidOperationException("Tuttle couldn't get tu Chipotle.");
				return;
			}

			CheckDistance();
		}

		/// <summary>
		/// Processes the GotoPoint message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnGotoPoint(GotoPoint message)
		{
			_path = FindPath(message.Goal);

			if (_path == null) // No path exists
				return;

			_goal = message.Goal;

			TuttleState state = message.WatchPlayer ? TuttleState.GoingToTargetAndWatchingPlayer : TuttleState.GoingToTarget;
			SetState(state);
		}

		/// <summary>
		/// Checks if the NPC is walking in the moment.
		/// </summary>
		protected bool Walking => _state == TuttleState.GoingToTarget || _state == TuttleState.GoingToTargetAndWatchingPlayer || _state == TuttleState.GoingToPlayer;

		/// <summary>
		/// Processes the Hide message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnHide(Hide message)
		{
			StopFollowing();
		}

		/// <summary>
		/// Processes the Reveal message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnReveal(Reveal message)
		{
			_area = message.Location;
			StartFollowing();
		}

		/// <summary>
		/// Processes the StartFollowing message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnStartFollowing(StartFollowing message)
			=> StartFollowing();

		/// <summary>
		/// Processes the StopFollowing message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnStopFollowing(StopFollowing message)
			=> StopFollowing();

		/// <summary>
		/// Performs walk on a preplanned route.
		/// </summary>
		protected override void PerformWalk()
		{
			base.PerformWalk();

			if (!Walking)
				return;

			if (_path.IsNullOrEmpty())
				StopWalk();
			else if (_walkTimer >= _speed)
				MakeStep();
		}

		/// <summary>
		/// starts walking after the Detective Chipotle NPC.
		/// </summary>
		private void StartFollowing()
		{
			StopWalk();
			SetState(TuttleState.WatchingPlayer);
		}

		/// <summary>
		/// Computes length of the next step of the NPC.
		/// </summary>
		protected override int GetSpeed()
		{
			if (_state == TuttleState.GoingToPlayer)
				return base.GetSpeed();
			return GetTerrainSpeed();
		}

		/// <summary>
		/// Performs one step in the direction given by the current orientation of the entity.
		/// </summary>
		protected override void MakeStep()
		{
			base.MakeStep();

			// Move if there are no obstacles.
			Vector2 target = GetNextTile();
			if (!DetectCollisions(target))
				JumpTo(target);  // The road is clear! Move!

			// Stop if distance from player is in allowed range.
			if (_state == TuttleState.GoingToPlayer)
			{
				float distance = GetDistanceFromPlayer();
				if (distance <= _targetDistance && _player.SameLocality(Owner))
					StopWalk();
			}


		}

		/// <summary>
		/// Avoids an obstacle or walks through a door if any.
		/// </summary>
		/// <param name="obstacle">The target coordinates</param>
		protected override bool DetectCollisions(Vector2 obstacle)
		{
			// Temporaryly solve a weird error that causes that the NPC bumps to it self.
			Entity o = World.GetItem(obstacle);
			if (o != null && o != Owner)
			{
				if (_state == TuttleState.GoingToPlayer)
					_restartApproaching = true;

				return true;
			}

			// If the obstacle is a door open it.
			Passage p = World.GetPassage(obstacle);
			if (p != null && p is Door && p.State == PassageState.Closed)
				p.TakeMessage(new UseDoor(Owner, obstacle)); // Open the door and keep walking.

			return false;
		}

		/// <summary>
		/// Stops coming to the Detective Chipotle NPC.
		/// </summary>
		private void StopApproaching()
		{
			StopWalk();
			SetState(TuttleState.Waiting);
		}

		/// <summary>
		/// Stops following the Detective Chipotle NPC.
		/// </summary>
		private void StopFollowing()
		{
			StopWalk();
			SetState(TuttleState.Waiting);
		}

		/// <summary>
		/// Stops ongoing walk and deletes current preplanned route.
		/// </summary>
		private void StopWalk()
		{
			_path = null;
			_walkTimer = 0;

			if (_state == TuttleState.GoingToPlayer || _state == TuttleState.GoingToTargetAndWatchingPlayer)
				SetState(TuttleState.WatchingPlayer);
			else SetState(TuttleState.Waiting);
		}
	}
}