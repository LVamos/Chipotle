using Assets.Scripts.Audio;
using Assets.Scripts.Models;

using Game.Audio;
using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Mapping.Saves;
using Game.Messaging;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Models;
using Game.Serialization.Protobuf.Snapshots.Spatial;

using System;
using System.Collections.Generic;
using System.Linq;


using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
    /// <summary>
    /// Represents one region on the game map (e.g. a room).
    /// </summary>

    public class Zone : MapElement
    {
        public void Restore(ZoneSave data)
        {
            base.Restore(data);
            _ceiling = data.Ceiling;
            _exits = !data.Exits.IsNullOrEmpty() ? new(data.Exits) : new();
            _characters = !data.Characters.IsNullOrEmpty() ? new(data.Characters) : new();
            _items = !data.Items.IsNullOrEmpty() ? new(data.Items) : new();
            _neighbours = new(data.Neighbours);
            _nonwalkables = data.Nonwalkables.ToVector2HashSet();
            DefaultTerrain = data.DefaultTerrain;
            Description = data.Description;
            To = data.To;
            Type = data.Type;
        }

        public ZoneSave Export()
        {
            var save = base.Export().ToZoneSave();

            save.Ceiling = _ceiling;
            save.Exits = new(_exits);
            save.Characters = new(_characters);
            save.Items = new(_items);
            save.Neighbours = new(_neighbours);
            save.Nonwalkables = _nonwalkables.ToVector2SaveHashSet();
            save.DefaultTerrain = DefaultTerrain;
            save.Description = Description;
            save.To = To;
            save.Type = Type;

            return save;
        }

        public bool SameAmbients(string soundName)
             => _ambientController != null && _ambientController.SameAmbients(soundName);

        /// <summary>
        /// Returns all open passages between this zone and the specified one..
        /// </summary>
        /// <returns>Enumeration of all open passages between this zone and the specified one</returns>
        public IEnumerable<Passage> GetOpenExits(Zone target) => GetOpenExits().Where(p => p.LeadsTo(target));

        /// <summary>
        /// Returns all open passages.
        /// </summary>
        /// <returns>Enumeration of all open passages</returns>
        public IEnumerable<Passage> GetOpenExits() => Exits.Where(p => p.Open);

        public bool HasPath(Zone target) => World.ZoneHasPath(this, target);

        public IEnumerable<Door> GetClosedDoors()
        {
            return
                Exits
                .OfType<Door>()
                .Where(d => d.State is PassageState.Closed or PassageState.Locked);
        }

        public bool IsWalkable(Rectangle area)
        {
            HashSet<Vector2> points = area.GetPoints(TileMap.TileSize);
            return points.All(IsWalkable);
        }

        public bool IsWalkable(Vector2 point) => !_nonwalkables.Contains(point);

        private HashSet<Vector2> _nonwalkables;
        public void GatherNonwalkables(Item item)
        {
            if (item.CanBePicked())
                return;

            float tileSize = TileMap.TileSize;

            HashSet<Vector2> points = item.Area.Value.GetPoints(tileSize);

            // I "snap" each point to the nearest half and put it in the list of non-passable points
            foreach (Vector2 point in points)
                _nonwalkables.Add(TileMap.SnapToGrid(point));
        }

        public bool PlayerInHere()
        {
            Vector2 player = World.Player.Area.Value.Center;
            return Area.Value.Contains(player);
        }

        /// <summary>
        /// Enumerates all passages leading to the specified zone.
        /// </summary>
        /// <param name="target">The target zone</param>
        /// <returns>enumeration of passages</returns>
        public List<Passage> GetExitsTo(Zone target) => Exits.Where(p => p.LeadsTo(target)).ToList();

        /// <summary>
        /// Enumerates all accessible zones.
        /// </summary>
        /// <returns>All accessible zones</returns>
        public IEnumerable<Zone> GetAccessibleZones()
        {
            return Exits.Select(p => p.AnotherZone(this)).Distinct();
        }

        /// <summary>
        /// Checks if the specified point lays in front or behind a passage.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>The passage by in front or behind which the specified point lays or null if nothing found</returns>
        public Passage GetPassageInFront(Vector2 point) => Exits.FirstOrDefault(p => p.IsInFrontOrBehind(point));

        /// <summary>
        /// Checks if the specified entity is in any neighbour zone.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>An instance of the zone in which the specified entity is located or null if it wasn't found</returns>
        public Zone IsInNeighbourZone(Character e) => Neighbours.FirstOrDefault(l => l.IsItHere(e));

        /// <summary>
        /// Checks if the specified object is in any neighbour zone.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>An instance of the zone in which the specified entity is located or null if it wasn't found</returns>
        public Zone IsInNeighbourZone(Item o) => Neighbours.FirstOrDefault(l => l.IsItHere(o));

        /// <summary>
        /// Checks if the specified entity is in any accessible neighbour zone.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>An instance of the zone in which the specified entity is located or null if it wasn't found</returns>
        public Zone IsInAccessibleZone(Character e) => GetZonesBehindDoor().FirstOrDefault(l => l.IsItHere(e));

        /// <summary>
        /// Checks if the specified object is in any accessible neighbour zone.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>An instance of the zone in which the specified entity is located or null if it wasn't found</returns>
        public Zone IsInAccessibleZone(Item o) => GetZonesBehindDoor().FirstOrDefault(l => l.IsItHere(o));

        /// <summary>
        /// Chekcs if the specified zone is accessible from this zone.
        /// </summary>
        /// <param name="l">The zone to be checked</param>
        /// <returns>True if the specified zone is accessible form this zone</returns>
        public bool IsBehindDoor(Zone l) => GetZonesBehindDoor().Any(zone => zone.Name.Inner == l.Name.Inner);

        /// <summary>
        /// Checks if it's possible to get to the specified zone from this zone over doors or open passages.
        /// </summary>
        /// <param name="zone">The target zone</param>
        /// <returns>True if there's a way between this loclaity and the specified zone</returns>
        public bool IsAccessible(Zone zone)
        {
            if (zone == this)
                return true;
            return GetAccessibleZones().Contains(zone);
        }

        /// <summary>
        /// Checks if the specified zone is next to this zone.
        /// </summary>
        /// <param name="l">The zone to be checked</param>
        /// <returns>True if the speicifed zone is adjecting to this zone</returns>
        public bool IsNeighbour(Zone l) => Neighbours.Contains(l);

        /// <summary>
        /// Maps all adejcting zones.
        /// </summary>
        private void FindNeighbours()
        {
            Rectangle a = Area.Value;
            a.Extend();
            _neighbours =
            (
                from p in a.GetPerimeterPoints()
                let l = World.GetZone(p)
                where l != null
                select l.Name.Inner
            ).Distinct().ToList();
        }

        /// <summary>
        /// List of adjecting zones
        /// </summary>
        private List<string> _neighbours;

        /// <summary>
        /// List of adjecting zones
        /// </summary>

        public Zone[] Neighbours => _neighbours.Select(World.GetZone).ToArray();

        /// <summary>
        /// Enumerates passages ordered by distance from the specified point.
        /// </summary>
        /// <param name="point">The reference point</param>
        /// <param name="radius">Optional search radius</param>
        /// <returns>List of passages</returns>
        public List<Passage> GetNearestExits(Vector2 point, float? radius = null, bool includePlayersPosition = false)
        {
            List<Passage> result = new();

            foreach (Passage e in Exits)
            {
                bool intersects = e.Area.Value.Contains(point);
                float distance = e.Area.Value.GetDistanceFrom(point);
                bool inRadius = radius == null || distance <= radius;
                bool matches = (includePlayersPosition && inRadius)
                  || (!includePlayersPosition && !intersects && inRadius);
                if (matches)
                    result.Add(e);
            }



            List<Passage> exits =
                (from e in Exits
                 let intersects = e.Area.Value.Contains(point)
                 let distance = e.Area.Value.GetDistanceFrom(point)
                 let inRadius = radius == null || distance <= radius
                 where (includePlayersPosition && inRadius)
                       || (!includePlayersPosition && !intersects && inRadius)
                 orderby distance
                 select e)
                .ToList();

            return exits.ToList();
        }

        /// <summary>
        /// Enumerates all characters around the specified <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point in whose surroundings the characters should be listed.</param>
        /// <param name="radius">Max distance from the specified <paramref name="point"/></param>
        /// <param name="ignoredCharacter">A character that shouldn't be included in the results</param>
        /// <returns>Enumeration of characters</returns>
        public IEnumerable<Character> GetNearByCharacters(Vector2 point, int radius, Character ignoredCharacter = null)
        {
            return
                from c in Characters
                let notTheIgnoredOne = c != ignoredCharacter
                let visible = c.Area != null
                let distance = c.Area.Value.GetDistanceFrom(point)
                let inRange = distance <= radius
                where notTheIgnoredOne && visible && inRange
                orderby distance
                select c;
        }

        /// <summary>
        /// Enumerates all items around the specified <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point in whose surroundings the items should be listed.</param>
        /// <param name="radius">Max distance from the specified <paramref name="point"/></param>
        /// <param name="includeDecoration">Specifies if the method lists decorative objects such as fences or rails.</param>
        /// <returns>Enumeration of items</returns>
        public IEnumerable<Item> GetNearByItems(Vector2 point, int radius, bool includeDecoration = false)
        {
            IEnumerable<Item> items = Items.Where(o => o.Area != null);

            return
                from o in items
                let distance = o.Area.Value.GetDistanceFrom(point)
                let inRange = distance <= radius
                let decorationMatches = o.Decorative == includeDecoration
                where decorationMatches && inRange
                orderby distance
                select o;
        }

        /// <summary>
        /// Height of ceiling of the zone (0 in case of outdoor zones)
        /// </summary>
        public readonly float Ceiling;

        /// <summary>
        /// All exits from the zone
        /// </summary>

        public Passage[] Exits
        {
            get
            {
                _exits ??= new();

                return _exits.Select(World.GetPassage).Where(p => p != null)
.ToArray();
            }
        }

        /// <summary>
        /// Text description of the zone
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Name of the zone in a shape that expresses a direction to the zone.
        /// </summary>
        public string To { get; private set; }

        /// <summary>
        /// Specifies if the zone is outside or inside a building.
        /// </summary>
        public ZoneType Type { get; private set; }

        /// <summary>
        /// The minimum permitted Y dimension of the floor in this zone
        /// </summary>
        private const int MinimumHeight = 3;

        /// <summary>
        /// The minimum permitted X dimension of the floor in this zone
        /// </summary>
        private const int MinimumWidth = 3;

        /// <summary>
        /// Height of ceiling of the zone (0 in case of outdoor zones)
        /// </summary>
        private float _ceiling;

        /// <summary>
        /// List of NPCs present in this zone.
        /// </summary>
        private HashSet<string> _characters;

        /// <summary>
        /// List of objects present in this zone.
        /// </summary>
        private HashSet<string> _items;

        /// <summary>
        /// List of exits from this zone
        /// </summary>
        private HashSet<string> _exits;

        private void CreateComponents(bool loopExists)
        {
            List<MessagingObject> components = new();
            _ambientController = gameObject.AddComponent<ZoneAmbientController>();
            components.Add(_ambientController);

            if (loopExists)
            {
                _portalController = gameObject.AddComponent<ZonePortalController>();
                components.Add(_portalController);
            }
            _components = components.ToArray();
        }

        public void Initialize(Name name, string description, string to, ZoneType type, float ceiling, Rectangle area, TerrainType defaultTerrain, ZoneLoopInfo loop, ZoneMaterials materials = null)
        {
            base.Initialize(name, area);
            _characters = new();
            _exits = new();
            _items = new();
            _neighbours = null;
            _nonwalkables = new();
            Description = description;
            To = to;
            Type = type;
            _ceiling = Type == ZoneType.Outdoor && ceiling <= 2 ? 0 : ceiling;
            _ceiling = type == ZoneType.Outdoor ? 0 : ceiling;
            DefaultTerrain = defaultTerrain;
            Rectangle temp = Area.Value;
            temp.MinimumHeight = MinimumHeight;
            temp.MinimumWidth = MinimumWidth;
            Area = temp;
            transform.position = area.Center.ToVector3(ceiling / 2);
            transform.localScale = new Vector3(area.Width, ceiling, area.Height);

            CreateComponents(loop != null && Settings.PlayZoneLoops);
            if (!Settings.PlayZoneLoops)
                _ambientController.Initialize(this, null, materials);
            else _ambientController.Initialize(this, loop, materials);
            _portalController?.Initialize(this, loop);
        }

        public TerrainType DefaultTerrain { get; private set; }

        /// <summary>
        /// List of NPCs present in this zone.
        /// </summary>

        public List<Character> Characters
        {
            get
            {
                List<Character> result = new();
                foreach (string name in _characters)
                {
                    Character character = World.GetCharacter(name)
                    ?? throw new ArgumentNullException($"Character {name} not found");
                    result.Add(character);
                }
                return result;
            }
        }

        /// <summary>
        /// Gets all acoustic obstacles in the zone within a specified distance from a point, including items marked as obstacles and closed doors.
        /// </summary>
        /// <param name="point">The reference point to measure distance from.</param>
        /// <param name="radius">The maximum allowed distance from the point. If null, no distance filtering is applied.</param>
        /// <returns>A list of map elements that act as acoustic obstacles within the specified distance.</returns>
        public AcousticObstacles GetAcousticObstacles(Character character, float radius)
        {
            HashSet<MapElement> objects =
                Filter(Items)
                .Concat(Filter(Exits))
                .Concat(Filter(Characters))
                .Where(o => o.Area != null && o != character && o.Area.Value.GetDistanceFrom(character.Center) <= radius)
                .ToHashSet();

            // Find obstacles in terrain
            Rectangle area = Rectangle.FromCenter(character.Center, radius, radius);
            HashSet<TileInfo> tiles = area.GetTiles()
                .Where(t => !character.Area.Value.Contains(t.Position)
                && t.Tile.Terrain is TerrainType.Bush or TerrainType.Wall)
                .ToHashSet();

            return new(objects, tiles);

            IEnumerable<MapElement> Filter(IEnumerable<MapElement> elements) =>
                elements.Where(e => e.AcousticObstacle);
        }

        public List<Item> GetMovableItems
            => Items.Where(i => i.CanBePicked())
            .ToList();

        /// <summary>
        /// List of objects present in this zone.
        /// </summary>

        public IEnumerable<Item> Items
        {
            get
            {
                _items ??= new();

                return _items.Select(World.GetItem)
                    .Where(o => o != null);
            }
        }

        /// <summary>
        /// Returns all adjecting zones to which it's possible to get from this zone.
        /// </summary>
        /// <returns>An enumeration of all adjecting accessible zones</returns>
        public IEnumerable<Zone> GetZonesBehindDoor() => Exits.Select(p => p.AnotherZone(this));

        /// <summary>
        /// Checks if the specified game object is present in this zone in the moment.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>True if the object is present in the zone</returns>
        public bool IsItHere(Item o) => _items.Contains(o.Name.Inner);

        /// <summary>
        /// Checks if an entity is present in this zone in the moment.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>True if the entity is here</returns>
        public bool IsItHere(Character e) => Characters.Contains(e);

        /// <summary>
        /// Checks if a passage lays in the zone.
        /// </summary>
        /// <param name="p">The passage to be checked</param>
        /// <returns>True if the passage lays in this zone</returns>
        public bool IsItHere(Passage p) => Exits.Contains(p);

        /// <summary>
        /// Gets a message from another messaging object and stores it for processing.
        /// </summary>
        /// <param name="message">The message to be received</param>
        /// <param name="routeToNeighbours">Specifies if the message should be distributed to the neighbours of this zone</param>
        public void TakeMessage(Message message, bool routeToNeighbours)
        {
            TakeMessage(message);

            if (routeToNeighbours)
            {
                foreach (Zone neighbour in Neighbours)
                    neighbour.TakeMessage(message);
            }
        }

        /// <summary>
        /// Gets a message from another messaging object and stores it for processing.
        /// </summary>
        /// <param name="message">The message to be received</param>
        public override void TakeMessage(Message message)
        {
            base.TakeMessage(message);

            if (message is ChipotlesCarMoved)
                return; // Don't send this to other objects and entities.

            MessageItems(message);
            MessageCharacters(message);
            MessagePassages(message);
        }

        /// <summary>
        /// Adds an passage to the zone.
        /// </summary>
        /// <param name="p">The passage to be added</param>
        public void Register(Passage p)
        {
            // Check if exit isn't already in list
            if (IsItHere(p))
                throw new InvalidOperationException("exit already registered");

            _exits.Add(p.Name.Inner);
        }

        /// <summary>
        /// Adds a game object to list of present objects.
        /// </summary>
        /// <param name="o">The object ot be added</param>
        private void Register(Item o) => _items.Add(o.Name.Inner);

        /// <summary>
        /// Adds an entity to zone.
        /// </summary>
        /// <param name="character">The entity to be added</param>
        public void Register(Character character)
        {
            _characters.Add(character.Name.Inner);
        }

        /// <summary>
        /// Initializes the zone and starts its message loop.
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            ActivateComponents();
            FindNeighbours();
        }

        private void ActivateComponents()
        {
            if (_components == null)
                return;

            foreach (MessagingObject component in _components)
                component?.Activate();
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(Message message)
        {
            switch (message)
            {
                case ItemLeftZone m: OnObjectDisappearedFromZone(m); break;
                case ItemAppearedInZone m: OnItemAppearedInZone(m); break;
                case CharacterLeftZone ll: OnCharacterLeftZone(ll); break;
                case CharacterCameToZone m: OnCharacterCameToZone(m); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        private void OnObjectDisappearedFromZone(ItemLeftZone m) => Unregister(m.Item);

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        private void OnItemAppearedInZone(ItemAppearedInZone message)
        {
            Register(message.Item);
            GatherNonwalkables(message.Item);
        }

        /// <summary>
        /// Immediately removes a game object from list of present objects.
        /// </summary>
        /// <param name="i"></param>
        private void Unregister(Item i) => _items.Remove(i.Name.Inner);

        /// <summary>
        /// Immediately removes an entity from list of present entities.
        /// </summary>
        /// <param name="e">The entity to be removed</param>
        public void Unregister(Character e) => _characters.Remove(e.Name.Inner);

        /// <summary>
        /// Removes a passage from the zone.
        /// </summary>
        /// <param name="p">The passage to be removed</param>
        public void Unregister(Passage p)
        {
            if (!Exits.Contains(p))
                throw new InvalidOperationException("Unregistered passage");

            _exits.Remove(p.Name.Inner);
        }

        /// <summary>
        /// Sends a message to all game objects in the zone.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        protected void MessageItems(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            foreach (Item o in Items)
            {
                if (o != message.Sender)
                    o.TakeMessage(message);
            }
        }

        private void MessageCharacters(Message message)
        {
            foreach (Character c in Characters)
            {
                if (c != message.Sender)
                    c?.TakeMessage(message);
            }
        }

        /// <summary>
        /// Handles the ZoneEntered message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnCharacterCameToZone(CharacterCameToZone message)
        {
            if (message.CurrentZone == this)
                Register(message.Character);
        }

        /// <summary>
        /// Distributes a game message to all passages.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        private void MessagePassages(Message message)
        {
            foreach (Passage p in Exits)
            {
                if (p != message.Sender)
                    p.TakeMessage(message);
            }
        }

        /// <summary>
        /// Handles the ZoneLeft message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnCharacterLeftZone(CharacterLeftZone message)
        {
            if (message.LeftZone != this)
                return;

            Unregister(message.Sender as Character);
        }

        private ZoneAmbientController _ambientController;
        private ZonePortalController _portalController;

        /// <summary>
        /// Returns all passages leading to the specified zone.
        /// </summary>
        /// <param name="zone">The zone to which the passages should lead</param>
        /// <returns> all passages leading to the specified zone</returns>
        private IEnumerable<Passage> GetPassagesTo(Zone zone) => Exits.Where(p => p.Zones.Contains(zone));

        public AudioSource ReleaseAmbientSource() => _ambientController.ReleaseAmbientSource();
    }

}