using Game.Entities;
using Game.Messaging.Events.Movement;
using Game.Models;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a passage between two zones.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(Door))]
    public class Passage : MapElement
    {
        public List<Vector2> GetPointsOfZone(Zone zone)
        {
            HashSet<Vector2> points = _area.Value.GetPoints();

            List<Vector2> pointsOfZone = points
                .Where(p => zone.Area.Value.Contains(p))
                .ToList();
            List<Zone> test = points.Select(p => World.Map[p]?.Zone).Distinct().ToList();
            return pointsOfZone;
        }

        /// <summary>
        /// Returns text description of the passage.
        /// </summary>
        /// <returns>text description of the passage</returns>
        public override string ToString() => "průchod";

        /// <summary>
        /// Checks if the specified point lays in front or behind the passage.
        /// </summary>
        /// <returns>True if the specified point lays in front or behind the passage</returns>
        public bool IsInFrontOrBehind(Vector2 point) => IsInRelatedZone(point) && (IsInHorizontalRange(point) || IsInVerticalRange(point));

        private bool IsInHorizontalRange(Vector2 point) => IsHorizontal() && point.x >= _area.Value.UpperLeftCorner.x && point.x <= _area.Value.UpperRightCorner.x;

        private bool IsInVerticalRange(Vector2 point)
        {
            return IsVertical()
                && point.y >= _area.Value.LowerLeftCorner.y && point.y <= _area.Value.UpperLeftCorner.y;
        }

        /// <summary>
        /// Chekcs if the specified point lays in one of the zones connected by the passage.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>True if the specified point lays in one of the zones connected by the passage</returns>
        public bool IsInRelatedZone(Vector2 point) => Zones.Any(l => l.Area.Value.Contains(point));

        /// <summary>
        /// Checks if the passage is horizontal.
        /// </summary>
        /// <returns>True if the passage is horizontal</returns>
        public bool IsHorizontal()
        {
            // Tests if both upper left corner and lower left corner lay in different zones (faster than World.GetZone)
            return
                Zones.First().Area.Value.Contains(_area.Value.UpperLeftCorner)
                ^ Zones.First().Area.Value.Contains(_area.Value.LowerLeftCorner);
        }

        /// <summary>
        /// Checks if the passage is vertical.
        /// </summary>
        /// <returns>True if the passage is vertical</returns>
        public bool IsVertical() => !IsHorizontal();

        /// <summary>
        /// Indicates if the door is open or closed.
        /// </summary>
        public PassageState State { get; protected set; } = PassageState.Open;

        /// <summary>
        /// Checks if the passage leads to the specified zone.
        /// </summary>
        /// <param name="l">The zone to be checked</param>
        /// <returns>True if the passage leads to the specified zone</returns>
        public bool LeadsTo(Zone l) => Zones.Contains(l);

        /// <summary>
        /// Zones connected by the passage
        /// </summary>
        [ProtoIgnore]
        public IEnumerable<Zone> Zones
        {
            get
            {
                _zones ??= new string[2];

                return _zones.Select(World.GetZone)
                    .Where(l => l != null);
            }
        }

        /// <summary>
        /// Zones connected by the passage
        /// </summary>
        protected string[] _zones = new string[2];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the passage</param>
        /// <param name="area">Coordinates of the are occupied by the passage</param>
        /// <param name="zones">Zones connected by the passage</param>
        public virtual void Initialize(Name name, Rectangle area, IEnumerable<string> zones)
        {
            base.Initialize(name, area);

            // Validate parameters
            if (
                zones == null
                || zones.Count() != 2
                || zones.First() == null
                || zones.Last() == null
                || zones.First() == zones.Last())
                throw new ArgumentException("Two different zones required");

            _sounds["navigation"] = "ExitLoop";
            _zones = zones.ToArray<string>();
            Rectangle testArea = new("1054, 1029, 1055, 1028.4");
            IEnumerable<Entity> testItems = testArea.GetObjects();

            // Validate passage location
            IEnumerable<Entity> items = area.GetObjects();
            IEnumerable<Passage> passages = area.GetPassages();
            if (items.Any() && passages.Any())
                throw new ArgumentException("No objects or nested passages allowed");

            Appear();
        }

        /// <summary>
        /// Returns another side of this passage.
        /// </summary>
        /// <param name="comparedZone">The zone to be compared</param>
        /// <returns>The other side of the passage than the specified one</returns>
        public Zone AnotherZone(Zone comparedZone) => Zones.First(l => l.Name.Indexed != comparedZone.Name.Indexed);

        /// <summary>
        /// Displays the passage in the game world.
        /// </summary>
        protected void Appear()
        {
            List<TileInfo> tiles = Area.Value.GetTiles();

            foreach (TileInfo info in tiles)
            {
                Zone zone = World.GetZone(info.Position);
                if (zone != null)
                    info.Tile.Edit(zone.DefaultTerrain);
            }
            foreach (Zone zone in Zones)
                zone.Register(this);

            foreach (Zone l in Zones)
                l.Register(this);
        }

        /// <summary>
        /// Erases the passage from the game world.
        /// </summary>
        protected void Disappear()
        {
            foreach (Zone l in Zones)
                l.Unregister(this);
        }

        /// <summary>
        /// Returns the point closest to the player.
        /// </summary>
        protected Vector2 GetClosestPointToPlayer() => _area.Value.GetClosestPoint(World.Player.Area.Value.Center);

        /// <summary>
        /// stores a zone in which the player is located after navigation start.
        /// </summary>
        [ProtoIgnore]
        protected Zone _playersZone;

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(Message message)
        {
            base.HandleMessage(message);
            switch (message)
            {
                case CharacterMoved em: OnEntityMoved(em); break;
            }
        }

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnEntityMoved(CharacterMoved message)
        {
            if (!_navigating || message.Sender != World.Player)
                return;

            UpdateNavigatingSoundPosition();
        }
    }
}