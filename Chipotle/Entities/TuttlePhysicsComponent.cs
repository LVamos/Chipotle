using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;
using System.Drawing;

namespace Game.Entities
{
    /// <summary>
    /// Controls movement of the Tuttle NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class TuttlePhysicsComponent : PhysicsComponent
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
        protected Queue<Vector2> FindPath(Vector2 goal)
        {
            if (_finder == null)
                _finder = new PathFinder();

            bool sameLocality = World.GetLocality(_area.Center) == World.GetLocality(goal); // Don't go to another localities if the goal is in the same locality.
			bool throughDoors = !sameLocality;
			return _finder.FindPath(_area.Center, goal, false, throughDoors, false, sameLocality, true);
		}

		/// <summary>
		/// Specifies the maximum allowed distance from the Detective Chipotle NPC.
		/// </summary>
		/// <remarks>Used when following the Detective Chipotle NPC</remarks>
		private const int _maxDistance = 10;

        /// <summary>
        /// Specifies the minimum allowed distance from the Detective Chipotle NPC.
        /// </summary>
        /// <remarks>Used when following the Detective Chipotle NPC</remarks>
        private const int _minDistance = 2;

        /// <summary>
        /// An instance of the Random number generator
        /// </summary>
        [ProtoIgnore]
        private Random _random = new Random();

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
        private Entity _player => World.Player;

        /// <summary>
        /// Specifies if Tuttle should start approaching to player when a new path is found after
        /// walk was interrupted.
        /// </summary>
        private bool _restartApproaching;

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            _orientation = new Orientation2D(0, 1);
            base.Start();
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case GameReloaded m: OnGameReloaded(m); break;
                case TuttleStateChanged m: OnTuttleStateChanged(m); break;
                case Reveal m: OnReveal(m); break;
                case Hide h: OnHide(h); break;
                case GotoPoint gp: OnGotoPoint(gp); break;
                case StartFollowing sf: OnStartFollowing(sf); break;
                case StopFollowing stf: OnStopFollowing(stf); break;
                case EntityMoved em: OnEntityMoved(em); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Handles the GameReloaded message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void OnGameReloaded(GameReloaded message)
        {
            base.OnGameReloaded(message);

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
        private Vector2? FindPlaceNearPlayer()
        {
            // Find possible goals. Try select a goal from the current locality of the player.
            Vector2? farther = _player.Area.FindNearestWalkableTile(_maxDistance);

            if (Owner.IsInSameLocality(_player))
                return farther;

            Vector2? closer = _player.Area.FindNearestWalkableTile(_minDistance);
            if (closer.HasValue && _player.Locality.Area.LaysOnPlane((Vector2)closer))
                return closer;
            return farther;
        }

        /// <summary>
        /// Computes distance between the NPC and the Detective Chipotle NPC.
        /// </summary>
        /// <returns>Distance between the NPC and the Detective Chipotle NPC</returns>
        private float GetDistanceFromPlayer()
=> World.GetDistance(_area.Center, _player.Area.Center);

        /// <summary>
        /// Computes distance between the NPC and the current goal.
        /// </summary>
        /// <returns>Distance between the NPC and the current goal</returns>
        private float GetDistanceFromGoal()
=> World.GetDistance(_area.Center, _goal);

        /// <summary>
        /// start walking the shortest path to the Detective Chipotle NPC.
        /// </summary>
        private void GoToPlayer()
        {
            // This is helpful in test mode if the Tuttle is set to follow the player right from the beginning. If the Chipotle isn't initialized yet Tuttle will keep trying to approach him.
            if (_area == null || Owner.Locality == null || World.Player == null)
                return;


            if (_random == null)
                _random = new Random();
            _targetDistance = _random.Next(_minDistance, _maxDistance);
            Vector2? tmp = FindPlaceNearPlayer();

            if (!tmp.HasValue)
                return;

            Vector2 goal = (Vector2)tmp;
            _path = FindPath(goal);

            if (_path == null)
                return;

            _goal = goal;
            SetState(TuttleState.GoingToPlayer);
            _walkTimer = 0;
        }

        /// <summary>
        /// Checks if the NPC isn't too far away from the Detective Chipotle NPC.
        /// </summary>
        private void CheckDistance()
        {
            float distance = GetDistanceFromPlayer();
            if (distance > _maxDistance && _state == TuttleState.WatchingPlayer && _area != null)
                GoToPlayer();
            else if (_state == TuttleState.GoingToPlayer && distance <= _targetDistance)
                StopWalk();
        }

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnEntityMoved(EntityMoved message)
        {
            // Test if the component has been initialized.
            if (_area == null)
                return;

            // Avoid too long paths.
            if (GetDistanceFromPlayer() > 150)
                return;

            if (message.Sender == _player && _state == TuttleState.WatchingPlayer)
                CheckDistance();
        }

        /// <summary>
        /// Processes the GotoPoint message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnGotoPoint(GotoPoint message)
        {
            StopFollowing();
            _path = FindPath(message.Goal);

            if (_path == null) // No path exists
                return;

            _goal = message.Goal;

            SetState(TuttleState.GoingToTarget);
        }

        /// <summary>
        /// Checks if the NPC is walking in the moment.
        /// </summary>
        private bool Walking => _state == TuttleState.GoingToTarget || _state == TuttleState.GoingToPlayer;

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

            if (Walking && _path.IsNullOrEmpty())
                StopWalk();
            else if (_restartApproaching)
                FindNewPath();
            else if (_state == TuttleState.WatchingPlayer && _area != null)
                CheckDistance();
            else if (Walking && _walkTimer >= _speed)
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

        protected override int GetSpeed()
        {
            if(_state == TuttleState.GoingToPlayer)
            return base.GetSpeed();
            return (int)GetTerrainSpeed();
        }

        /// <summary>
        /// Performs one step in the direction given by the current orientation of the entity.
        /// </summary>
        protected override void MakeStep()
        {
            base.MakeStep();

            // Move if there are no obstacles.
            Vector2 target = GetNextTile();
            if (!SolveObstacle(target))
            Move(target);  // The road is clear! Move!
        }

        /// <summary>
        /// Avoids an obstacle or walks through a door if any.
        /// </summary>
        /// <param name="obstacle">The target coordinates</param>
        protected override bool SolveObstacle(Vector2 obstacle)
        {
            // Temporaryly solve a weird error that causes that the NPC bumps to it self.
            GameObject o = World.GetObject(obstacle);
            if (o != null && o != Owner)
            {
                if (_state == TuttleState.GoingToPlayer)
                    _restartApproaching = true;

                    return true;
            }

            // If the obstacle is a door open it.
            Passage p = World.GetPassage(obstacle);
            if (p != null && p.State == PassageState.Closed)
                p.TakeMessage(new UseObject(Owner, obstacle)); // Open the door and keep walking.

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
            SetState(TuttleState.Waiting    );
        }

        /// <summary>
        /// Stops ongoing walk and deletes current preplanned route.
        /// </summary>
        private void StopWalk()
        {
            _path = null;
            _walkTimer = 0;

            if (_state == TuttleState.GoingToPlayer)
                SetState(TuttleState.WatchingPlayer);
            else SetState(TuttleState.Waiting);
        }
    }
}