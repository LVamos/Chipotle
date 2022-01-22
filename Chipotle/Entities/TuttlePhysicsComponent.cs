using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls movement of the Tuttle NPC.
    /// </summary>
    [Serializable]
    public class TuttlePhysicsComponent : PhysicsComponent
    {
        /// <summary>
        /// sets state of the NPC and announces the change to other components.
        /// </summary>
        private void SetState(TuttleState state)
        {
            _state = state;
            Owner.ReceiveMessage(new TuttleStateChanged(this, state));
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
            Vector2? start = _area.FindNearestWalkableTile(3);
            return start.HasValue ? _finder.FindPath((Vector2)start, goal) : null;
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
        /// Reference to a path finder instance
        /// </summary>
        private readonly PathFinder _finder = new PathFinder();

        /// <summary>
        /// An instance of the Random number generator
        /// </summary>
        private readonly Random _random = new Random();

        /// <summary>
        /// Specifies final distance from the Detective Chipotle when in process of approaching to it.
        /// </summary>
        private int _targetDistance;

        /// <summary>
        /// The goal that Tuttle is just going to.
        /// </summary>
        private Vector2 _goal;

        /// <summary>
        /// Stores the current shortest track towards the Detective Chipotle NPC.
        /// </summary>
        private Queue<Vector2> _path;

        /// <summary>
        /// Measures elapsed time from the last attempt of finding a path.
        /// </summary>
        private int _finderTimer;

        /// <summary>
        /// Reference to the Detective Chipotle NPC
        /// </summary>
        private Entity _player;

        /// <summary>
        /// Specifies if Tuttle should start approaching to player when a new path is found after
        /// walk was interrupted.
        /// </summary>
        private bool _restartApproaching;

        /// <summary>
        /// Specifies the length of one step in milliseconds.
        /// </summary>
        private float _stepInterval;

        /// <summary>
        /// Specifies if the NPC walks in constant speed.
        /// </summary>
        private bool _constantSpeed;

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            _orientation = new Orientation2D(0, 1);
            _player = World.Player;

            RegisterMessages(new Dictionary<Type, Action<Messaging.GameMessage>>
            {
                [typeof(TuttleStateChanged)] = (message) => OnTuttleStateChanged((TuttleStateChanged)message),
                [typeof(Reveal)] = (message) => OnReveal((Reveal)message),
                [typeof(Hide)] = (message) => OnHide((Hide)message),
                [typeof(GotoPoint)] = (m) => OnGotoPoint((GotoPoint)m),
                [typeof(StartFollowing)] = (m) => OnStartFollowing((StartFollowing)m),
                [typeof(StopFollowing)] = (m) => OnStopFollowing((StopFollowing)m),
                [typeof(EntityMoved)] = (message) => OnEntityMoved((EntityMoved)message)
            }
                );

            base.Start();
        }

        /// <summary>
        /// Handles the TuttleStateChanged message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnTuttleStateChanged(TuttleStateChanged message)
=> _state = message.State;

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            PerformWalk();
        }

        /// <summary>
        /// Computes speed of walk according to the distance from the Detective Chipotle NPC.
        /// </summary>
        /// <param name="distance">Distance from the Detective Chipotle NPC</param>
        /// <returns>The walk speed</returns>
        private int ComputeWalkSpeed(float distance)
        {
            const int minSpeed = 200;
            const int maxSpeed = 400;
            const int interval = 5;
            const int minDistance = 10;
            const int maxDistance = 80;
            const int speedInterval = (maxSpeed - minSpeed) / ((maxDistance - minDistance) / interval);

            if (distance > maxDistance)
                return minSpeed;

            int tp = (int)distance/ interval;
            return maxSpeed - (speedInterval *tp);
        }

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
            SetWalkSpeed();
            _stepInterval = 0;
        }

        /// <summary>
        /// Computes the walk speed according to distance from the player.
        /// </summary>
        private void SetWalkSpeed()
        {
                _walkSpeed = ComputeWalkSpeed(_state == TuttleState.GoingToPlayer ? GetDistanceFromPlayer() : GetDistanceFromGoal());

        }

        /// <summary>
        /// Checks if the NPC isn't too far away from the Detective Chipotle NPC.
        /// </summary>
        private void CheckDistance()
        {
            float distance = GetDistanceFromPlayer();
            if (distance > _maxDistance && _state == TuttleState.WatchingPlayer)
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

            if (message.WalkSpeed > 0)
            {
                _constantSpeed = true;
                _walkSpeed = message.WalkSpeed;
            }
            else SetWalkSpeed();

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
        private void PerformWalk()
        {
            if (Walking && _path.IsNullOrEmpty())
                StopWalk();
            else if (_restartApproaching)
                FindNewPath();
            else if (_state == TuttleState.WatchingPlayer)
                CheckDistance();
            else if (Walking && _stepInterval >= _walkSpeed)
            {
                _stepInterval = 0;
                Step();
            }
            else if (Walking)
                _stepInterval += World.DeltaTime;
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
        /// Performs one step in the direction given by the current orientation of the entity.
        /// </summary>
        private void Step()
        {
            SetWalkSpeed();
            Vector2 target = _path.Dequeue(); // Get target tile

            // Detect obstacles
            if (!World.IsWalkable(target))
                AvoidObstacle(target);
else SetPosition(target);  // The road is clear! Move!
        }

        /// <summary>
        /// Avoids an obstacle or walks through a door if any.
        /// </summary>
        /// <param name="target">The target coordinates</param>
        private void AvoidObstacle(Vector2 target)
        {
            // Temporaryly solve a weird error that causes that the NPC bumps to it self.
            if (World.GetObject(target) == Owner)
                return;

            // If the obstacle is a door open it.
            Passage p = World.GetPassage(target);
            if (p != null && p.State == PassageState.Closed)
                p.ReceiveMessage(new UseObject(Owner, target)); // Open the door and keep walking.
            else if (_state == TuttleState.GoingToPlayer)
                _restartApproaching = true;
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
            _constantSpeed = false;
            _path = null;
            _stepInterval = 0;

            if (_state == TuttleState.GoingToPlayer)
                SetState(TuttleState.WatchingPlayer);
            else SetState(TuttleState.Waiting);
        }
    }
}