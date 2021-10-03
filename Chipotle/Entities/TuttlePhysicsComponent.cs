using System;
using System.Collections.Generic;

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
    public class TuttlePhysicsComponent : PhysicsComponent
    {
        /// <summary>
        /// Specifies the maximum allowed distance from the Detective Chipotle NPC.
        /// </summary>
        /// <remarks>Used when following the Detective Chipotle NPC</remarks>
        private const int _maxDistanceFromPlayer = 10;

        /// <summary>
        /// Specifies the minimum allowed distance from the Detective Chipotle NPC.
        /// </summary>
        /// <remarks>Used when following the Detective Chipotle NPC</remarks>
        private const int _minDistanceFromPlayer = 2;

        /// <summary>
        /// Reference to a path finder instance
        /// </summary>
        private readonly PathFinder _finder = new PathFinder();

        /// <summary>
        /// An instance of the Random number generator
        /// </summary>
        private readonly Random _random = new Random();

        /// <summary>
        /// Indicates if the NPC walks to the Detective Chipotle NPC.
        /// </summary>
        private bool _approachToPlayer;

        /// <summary>
        /// Specifies final distance from the Detective Chipotle when in process of approaching to it.
        /// </summary>
        private int _desiredDistanceFromPlayer;

        /// <summary>
        /// Indicates if the NPC keeps following the Detective Chipotle NPC.
        /// </summary>
        private bool _followPlayer;

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
        private int _pathFinderTimer;

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
        /// Specifies if Tuttle should repeatedly try to find a path to the current goal.
        /// </summary>
        private bool _tryFindPath;

        /// <summary>
        /// Specifies if the NPC is currently walking.
        /// </summary>
        private bool _walk;

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            _orientation = new Orientation2D(0, 1);
            _player = World.Player;

            RegisterMessages(new Dictionary<Type, Action<Messaging.GameMessage>>
            {
                [typeof(Reveal)] = (message) => OnReveal((Reveal)message),
                [typeof(Hide)] = (message) => OnHide((Hide)message),
                [typeof(GotoPoint)] = (m) => OnGotoPoint((GotoPoint)m),
                [typeof(StartFollowing)] = (m) => OnStartFollowing((StartFollowing)m),
                [typeof(StopFollowing)] = (m) => OnStopFollowing((StopFollowing)m),
                [typeof(LocalityLeft)] = (message) => OnLocalityLeft((LocalityLeft)message),
                [typeof(EntityMoved)] = (message) => OnEntityMoved((EntityMoved)message)
            }
                );

            base.Start();
        }

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
        private int ComputeWalkSpeed(int distance)
        {
            if (distance < 5)
                return 1200;

            if (distance < 7)
                return 1000;

            if (distance < 10)
                return 900;

            if (distance < 15)
                return 500;

            if (distance < 20)
                return 300;

            return 150;
        }

        private void FindNewPath()
        {
            if (_pathFinderTimer < 30)
            {
                _pathFinderTimer++;
                return;
            }

            _path = _finder.FindPath(_area.Center, _goal);

            if (_path == null)
                return;

            _tryFindPath = false;
            _pathFinderTimer = 0;
            if (_restartApproaching)
            {
                _restartApproaching = false;
                GoToPlayer();
            }
        }

        /// <summary>
        /// finds a walkable tile near the Detective Chipotle NPC.
        /// </summary>
        /// <returns>Coordinates of tthe target tile</returns>
        private Vector2? FindPlaceNearPlayer()
=> _player.Area.FindNearestWalkableTile(_maxDistanceFromPlayer);

        /// <summary>
        /// Computes distance between two points.
        /// </summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The second point</param>
        /// <returns>The distance between the points</returns>
        private int GetDistance(Vector2 a, Vector2 b)
=> (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

        /// <summary>
        /// Computes distance between the NPC and the Detective Chipotle NPC.
        /// </summary>
        /// <returns>Distance between the NPC and the Detective Chipotle NPC</returns>
        private int GetDistanceFromPlayer()
=> GetDistance(_area.Center, _player.Area.Center);

        /// <summary>
        /// start walking the shortest path to the Detective Chipotle NPC.
        /// </summary>
        private void GoToPlayer()
        {
            _desiredDistanceFromPlayer = _random.Next(_minDistanceFromPlayer, _maxDistanceFromPlayer);
            Vector2? tempGoal = FindPlaceNearPlayer();

            if (!tempGoal.HasValue)
                return;

            Vector2 goal = (Vector2)tempGoal;
            _path = _finder.FindPath(_area.Center, goal);

            if (_path == null)
                return;

            _goal = goal;
            _approachToPlayer = _walk = true;
            _walkSpeed = ComputeWalkSpeed(GetDistanceFromPlayer());
            _stepInterval = 0;
        }

        /// <summary>
        /// Checks if the NPC isn't too far away from the Detective Chipotle NPC.
        /// </summary>
        private void CheckDistanceFromPlayer()
        {
            int distance = GetDistanceFromPlayer();
            if (distance > _maxDistanceFromPlayer && !_approachToPlayer)
                GoToPlayer();
            else if (_approachToPlayer && (distance <= _desiredDistanceFromPlayer || _path.IsNullOrEmpty()))
                StopApproachingToPlayer();
        }

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnEntityMoved(EntityMoved message)
        {
            if (message.Sender != _player)
                return;

            if (_followPlayer && !_approachToPlayer)
                CheckDistanceFromPlayer();
        }

        /// <summary>
        /// Processes the GotoPoint message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnGotoPoint(GotoPoint message)
        {
            _path = message.Path;
            _walkSpeed = message.StepLength;
            _walk = true;
        }

        /// <summary>
        /// Processes the Hide message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnHide(Hide message)
        {
            StopFollowing();
            DisAppear();
        }

        /// <summary>
        /// Processes the LocalityLeft message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityLeft(LocalityLeft message)
        {
            if (message.Sender != _player)
                return;

            StopApproachingToPlayer();
            GoToPlayer();
        }

        /// <summary>
        /// Processes the Reveal message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnReveal(Reveal message)
        {
            _area = message.Location;
            Appear(message.Location);
            StartFollowing();
        }

        /// <summary>
        /// Processes the StartFollowing message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnStartFollowing(StartFollowing message)
        {
            _path = null;
            _followPlayer = true;
        }

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
            if (!_walk)
                return;

            if (_tryFindPath)
            {
                FindNewPath();
                return;
            }

            if (_approachToPlayer)
                CheckDistanceFromPlayer();

            if (_stepInterval >= _walkSpeed)
            {
                _stepInterval = 0;
                Step();
            }
            else
                _stepInterval += World.DeltaTime;

            if (_path.IsNullOrEmpty())
                StopWalk();
        }

        /// <summary>
        /// starts walking after the Detective Chipotle NPC.
        /// </summary>
        private void StartFollowing()
            => _followPlayer = true;

        /// <summary>
        /// Performs one step in the direction given by the current orientation of the entity.
        /// </summary>
        private void Step()
        {
            // Get target tile
            Plane target = new Plane(_path.Dequeue());
            Tile targetTile = World.Map[target.Center];

            if (!targetTile.Walkable && (targetTile.IsOccupied && targetTile.Object != Owner))
            {
                _tryFindPath = true;

                if (_approachToPlayer)
                {
                    _restartApproaching = true;
                    StopApproachingToPlayer();
                }

                return;
            }

            // The road is clear! Move!
            SetPosition(target);
        }

        /// <summary>
        /// Stops coming to the Detective Chipotle NPC.
        /// </summary>
        private void StopApproachingToPlayer()
        {
            _walk = _approachToPlayer = false;
            _path = null;
        }

        /// <summary>
        /// Stops following the Detective Chipotle NPC.
        /// </summary>
        private void StopFollowing()
        {
            _followPlayer = false;
            StopWalk();
        }

        /// <summary>
        /// Stops ongoing walk and deletes current preplanned route.
        /// </summary>
        private void StopWalk()
        {
            _walk = false;
            _path = null;
            _stepInterval = 0;
        }
    }
}