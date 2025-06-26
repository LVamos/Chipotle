using Assets.Scripts.Models;

using Game.Audio;
using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;

using ProtoBuf;

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
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class Zone : MapElement
	{
		protected ReadyPortalModel GetPortalByPassage(List<ReadyPortalModel> portals, Passage passage)
		{
			ReadyPortalModel result = portals
				.First(loop => loop.Passage == passage);
			return result;
		}

		private List<Passage> ReadyPortalsToPassages(List<ReadyPortalModel> portals)
		{
			List<Passage> result = portals
				.Select(loop => loop.Passage)
				.ToList();
			return result;
		}


		protected ReadyPortalModel RemoveReadyPortalNearPlayer(List<ReadyPortalModel> portals)
		{
			List<Passage> passages = ReadyPortalsToPassages(portals);
			Passage closestPassage = World.GetClosestElement(passages, World.Player) as Passage;
			ReadyPortalModel closestPortal = GetPortalByPassage(portals, closestPassage);
			portals.Remove(closestPortal);
			return closestPortal;
		}




		public AudioSource ReleaseAmbientSource()
		{
			AudioSource source = _ambientSource;
			_ambientSource = null;
			return source;
		}

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

		public bool IsWalkable(Vector2 point) => !_nonpassables.Contains(point);

		private HashSet<Vector2> _nonpassables;
		public void GatherNonwalkables(Item item)
		{
			if (item.CanBePicked())
				return;

			float tileSize = TileMap.TileSize;

			HashSet<Vector2> points = item.Area.Value.GetPoints(tileSize);

			// I "snap" each point to the nearest half and put it in the list of non-passable points
			foreach (Vector2 point in points)
				_nonpassables.Add(World.Map.SnapToGrid(point));
		}

		private bool PlayerInHere()
		{
			Vector2 player = World.Player.Area.Value.Center;
			return Area.Value.Contains(player);
		}

		private bool PlayerInSoundRadius
		{ get => GetDistanceToPlayer() <= _zoneSoundRadius; }

		/// <summary>
		/// Surfacematerials for walls, floor and ceiling
		/// </summary>
		public ZoneMaterialsDefinitionModel Materials { get; private set; }

		[ProtoIgnore]
		private AudioSource _ambientSource;

		/// <summary>
		/// Enumerates all passages leading to the specified zone.
		/// </summary>
		/// <param name="target">The target zone</param>
		/// <returns>enumeration of passages</returns>
		public List<Passage> GetExitsTo(Zone target) => Exits.Where(p => p.LeadsTo(target)).ToList();

		/// <summary>
		/// Indicates if a zone is inside a building or outside.
		/// </summary>
		public enum ZoneType
		{
			/// <summary>
			/// A room or corridor in a building
			/// </summary>
			Indoor,

			/// <summary>
			/// An openair place like yard or meadow
			/// </summary>
			Outdoor
		}

		/// <summary>
		/// Enumerates all accessible zones.
		/// </summary>
		/// <returns>All accessible zones</returns>
		public IEnumerable<Zone> GetAccessibleZones() => Exits.Select(p => p.AnotherZone(this)).Distinct();

		/// <summary>
		/// Handles the EntityMoved message.
		/// </summary>
		/// <param name="message">The mesage to be handled</param>
		private void OnCharacterMoved(CharacterMoved message)
		{
			if (message.Sender == World.Player)
				UpdatePortals();
		}

		/// <summary>
		/// Updates position of passage sound loops.
		/// </summary>
		private void UpdatePortals()
		{
			foreach (Passage passage in _portals.Keys)
			{
				Vector2 player = World.Player.Area.Value.Center;
				AudioSource source = _portals[passage].AudioSource;

				// If the player is standing right in the passage locate the sound right on his position.
				if (passage.Area.Value.Contains(player))
				{
					source.transform.position = player.ToVector3(4);
					continue;
				}

				Vector2? point = passage.Area.Value.GetAlignedPoint(player)
				?? passage.Area.Value.GetClosestPoint(player);
				source.transform.position = point.Value.ToVector3(2);
				UpdatePortalAmbientSpatialBlend(passage, source);
				if (source.volume <= 0)
					UpdatePortalVolume(passage, _portals[passage]);
			}
		}

		private void UpdatePortalAmbientSpatialBlend(Passage passage, AudioSource source)
		{
			float oldSpatialBlend = source.spatialBlend;
			if (passage is Door)
			{
				source.spatialBlend = 1;
				source.outputAudioMixerGroup = Sounds.ResonanceGroup;
				source.spatialize = true;
				return;
			}

			int distance = (int)passage.Area.Value.GetDistanceFrom(World.Player.Area.Value);
			source.spatialBlend = distance > 10 ? 1 : distance * .1f;

			// Turn off spatialization if spatial blend was set to 1.
			if (oldSpatialBlend >= 1)
			{
				source.spatialize = false;
				source.outputAudioMixerGroup = null;
			}
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
		/// Returns all open passages.
		/// </summary>
		/// <returns>Enumeration of all open passages</returns>
		public IEnumerable<Passage> GetApertures() => Exits.Where(p => p.State == PassageState.Open);

		/// <summary>
		/// Chekcs if the specified zone is accessible from this zone.
		/// </summary>
		/// <param name="l">The zone to be checked</param>
		/// <returns>True if the specified zone is accessible form this zone</returns>
		public bool IsBehindDoor(Zone l) => GetZonesBehindDoor().Any(zone => zone.Name.Indexed == l.Name.Indexed);

		/// <summary>
		/// Checks if it's possible to get to the specified zone from this zone over doors or open passages.
		/// </summary>
		/// <param name="zone">The target zone</param>
		/// <returns>True if there's a way between this loclaity and the specified zone</returns>
		public bool IsAccessible(Zone zone) => GetAccessibleZones().Any(l => l == zone);

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
				select l.Name.Indexed
			).Distinct().ToList();
		}

		/// <summary>
		/// List of adjecting zones
		/// </summary>
		private List<string> _neighbours;

		/// <summary>
		/// List of adjecting zones
		/// </summary>
		[ProtoIgnore]
		public Zone[] Neighbours => _neighbours.Select(World.GetZone).ToArray();

		/// <summary>
		/// Returns all open passages between this zone and the specified one..
		/// </summary>
		/// <returns>Enumeration of all open passages between this zone and the specified one</returns>
		public IEnumerable<Passage> GetApertures(Zone l) => GetApertures().Where(p => p.LeadsTo(l));

		/// <summary>
		/// Enumerates passages ordered by distance from the specified point.
		/// </summary>
		/// <param name="point">The default point</param>
		/// <param name="radius">distance in which exits from current zone are searched</param>
		/// <returns>Enumeration of passages</returns>
		public List<Passage> GetNearestExits(Vector2 point, int? radius = null)
		{
			IEnumerable<Passage> exits = null;

			if (radius != null)
			{
				exits =
					from e in Exits
					let intersects = e.Area.Value.Contains(point)
					let distance = e.Area.Value.GetDistanceFrom(point)
					where !intersects && distance <= radius
					orderby distance
					select e;
			}
			else
			{
				exits =
	from e in Exits
	let intersects = e.Area.Value.Contains(point)
	let distance = e.Area.Value.GetDistanceFrom(point)
	where !intersects
	orderby distance
	select e;
			}

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
		public IEnumerable<Item> GetNearByObjects(Vector2 point, int radius, bool includeDecoration = false)
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
		[ProtoIgnore]
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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name of the zone</param>
		/// <param name="to">
		/// Name of the zone in a shape that expresses a direction to the zone
		/// </param>
		/// <param name="type">Specifies if the zone is outside or inside a building.</param>
		/// <param name="ceiling">Ceiling height of the zone (should be 0 for outdoor zones)</param>
		/// <param name="area">Coordinates of the area occupied by the zone</param>
		/// <param name="defaultTerrain">Lowest layer of the terrain in the zone</param>
		/// <param name="backgroundInfo">A background sound played in loop</param>
		public void Initialize(Name name, string description, string to, ZoneType type, float ceiling, Rectangle area, TerrainType defaultTerrain, string loop, float volume, ZoneMaterialsDefinitionModel materials = null)
		{
			base.Initialize(name, area);
			_characters = new();
			_ambientSource = null;
			_exits = new();
			_items = new();
			_neighbours = null;
			_nonpassables = new();
			_portals = new();
			_soundMode = default;
			AmbientSound = null;

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

			AmbientSound = loop;
			_defaultVolume = volume;

			gameObject.transform.position = new Vector3(area.Center.x, ceiling / 2, area.Center.y);
			gameObject.transform.localScale = new Vector3(area.Width, ceiling, area.Height);
			Materials = materials;
		}

		/// <summary>
		/// Defines the lowest layer of terrain in the zone.
		/// </summary>
		[ProtoIgnore]
		public TerrainType DefaultTerrain { get; private set; }

		/// <summary>
		/// List of NPCs present in this zone.
		/// </summary>
		[ProtoIgnore]
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

		[ProtoIgnore]
		public IEnumerable<Item> MovableItems => Items.Where(i => i.CanBePicked());

		/// <summary>
		/// List of objects present in this zone.
		/// </summary>
		[ProtoIgnore]
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
		public bool IsItHere(Item o) => _items.Contains(o.Name.Indexed);

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

			_exits.Add(p.Name.Indexed);
		}

		/// <summary>
		/// Adds a game object to list of present objects.
		/// </summary>
		/// <param name="o">The object ot be added</param>
		private void Register(Item o) => _items.Add(o.Name.Indexed);

		/// <summary>
		/// Adds an entity to zone.
		/// </summary>
		/// <param name="character">The entity to be added</param>
		public void Register(Character character)
		{
			_characters.Add(character.Name.Indexed);
		}

		/// <summary>
		/// Initializes the zone and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			FindNeighbours();
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
				case ChipotlesCarMoved ccmv: OnChipotlesCarMoved(ccmv); break;
				case CharacterMoved em: OnCharacterMoved(em); break;
				case DoorManipulated dm: OnDoorManipulated(dm); break;
				case Reloaded gr: OnGameReloaded(); break;
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
		/// Handles the ChipotlesCarMoved message.
		/// </summary>
		/// <param name="message"The message to be handled></param>
		private void OnChipotlesCarMoved(ChipotlesCarMoved message)
		{
			if (World.GetZone(message.Target.Center) != this)
				StopAmbientSounds();
		}

		/// <summary>
		/// Handles the DoorManipulated message.
		/// </summary>
		/// <param name="message">Source of the message</param>
		private void OnDoorManipulated(DoorManipulated message)
		{
			if (!_portals.ContainsKey(message.Sender))
				return;

			List<ReadyPortalModel> PreparedPortalAmbients = PreparePassageLoops();
			ReadyPortalModel preparedPortalAmbient = PreparedPortalAmbients.First(l => l.Passage == message.Sender);

			PortalModel portalAmbient = _portals[message.Sender];
			UpdatePortalVolume(preparedPortalAmbient.Passage, portalAmbient);
			SetDoorOcclusion(preparedPortalAmbient, portalAmbient);
		}

		/// <summary>
		/// Immediately removes a game object from list of present objects.
		/// </summary>
		/// <param name="i"></param>
		private void Unregister(Item i) => _items.Remove(i.Name.Indexed);

		/// <summary>
		/// Immediately removes an entity from list of present entities.
		/// </summary>
		/// <param name="e">The entity to be removed</param>
		public void Unregister(Character e) => _characters.Remove(e.Name.Indexed);

		/// <summary>
		/// Removes a passage from the zone.
		/// </summary>
		/// <param name="p">The passage to be removed</param>
		public void Unregister(Passage p)
		{
			if (!Exits.Contains(p))
				throw new InvalidOperationException("Unregistered passage");

			_exits.Remove(p.Name.Indexed);
		}

		/// <summary>
		/// Erases the zone from game world.
		/// </summary>
		protected void Disappear()
		{
			// Delete objects.
			foreach (Item o in Items)
				World.Remove(o);

			// Delete passages.
			foreach (Passage p in Exits)
				World.Remove(p);

			// Delete character.
			foreach (Character e in Characters)
				World.Remove(e);

			// Delete zone from the map.
			foreach (Vector2 p in _area.Value.GetPoints())
				World.Map[p] = null;
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
		/// Handles the Reloaded message.
		/// </summary>
		private void OnGameReloaded()
		{
			_portals = new();
			UpdateAmbientSounds();
		}

		/// <summary>
		/// Handles the ZoneEntered message.
		/// </summary>
		/// <param name="message">The message</param>
		private void OnCharacterCameToZone(CharacterCameToZone message)
		{
			if (message.CurrentZone == this)
			{
				Register(message.Character);

				if (message.Character == World.Player)
					Sounds.SetRoomParameters(this);
			}

			if (message.Character == World.Player)
				UpdateAmbientSounds(message.PreviousZone);
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

		/// <summary>
		/// Name of background sound played in loop.
		/// </summary>
		public string AmbientSound;

		protected bool TryStealAmbient(Zone previousZone)
		{
			// Coming from another zone.
			if (previousZone == null
								|| !SameAmbients(previousZone))
				return false;

			if (PlayerInHere())
			{
				_ambientSource = previousZone.ReleaseAmbientSource();
				StopPortals();
				return true;
			}
			return false;
		}

		private bool SameAmbients(Zone zone) => string.Equals(zone.AmbientSound, AmbientSound, StringComparison.OrdinalIgnoreCase);

		protected bool IsSameAmbientNearBy()
=> Neighbours.Any(n => SameAmbients(n));

		/// <summary>
		/// Plays the background sound of this zone in a loop.
		/// </summary>
		/// <param name="playerMoved">Specifies if the player just moved from one zone to another one.</param>
		private void UpdateAmbientSounds(Zone previousZone = null)
		{
			if (string.IsNullOrEmpty(AmbientSound))
				return;

			if (previousZone != null && TryStealAmbient(previousZone))
				return;

			bool playerHere = PlayerInHere();
			if (IsSameAmbientNearBy())
			{
				if (playerHere)
					Play2dAmbient();
				else if (SameAmbients(World.Player.Zone))
					StopPortals();
				else PlayAmbientFromExits(previousZone);
			}
			else
			{
				if (playerHere)
					Play2dAmbient();
				else
					PlayAmbientFromExits(previousZone);
			}
		}

		/// <summary>
		/// Playback mode for background sounds
		/// </summary>
		private SoundMode _soundMode;

		/// <summary>
		/// Defines playback modes for zone background sound based on player's proximity.
		/// </summary>
		public enum SoundMode
		{
			/// <summary>
			/// If the player is in the zone, the sound plays in full stereo.
			/// </summary>
			InZone,

			/// <summary>
			/// If the player is in an accessible zone, the sound plays softly from every passage leading to the player's current zone.
			/// </summary>
			InAccessibleZone,

			/// <summary>
			/// If the player is far, the sound plays softly from the center of the zone, provided the player is within the sound's radius defined by _soundRadius.
			/// </summary>
			InInaccessibleZone
		}

		private void StopLoops()
		{
			Stop(ref _ambientSource);
			foreach (PortalModel loop in _portals.Values)
				Stop(ref loop.AudioSource);
			_portals = new();

			void Stop(ref AudioSource source)
			{
				if (source != null && source.isPlaying)
				{
					source.Stop();
					source = null;
				}
			}
		}

		/// <summary>
		/// Stores the identifiers of location audio loops played in passages.
		/// </summary>
		[ProtoIgnore]
		private Dictionary<Passage, PortalModel> _portals;

		private bool _reloaded;
		private const float _portalAmbientMaxDistance = 9;

		/// <summary>
		/// The maximum distance the player is from the zone at which it makes sense to play the location audio.
		/// </summary>
		private const float _zoneSoundRadius = 100;
		private const int _portalAmbientOpenDoorMaxDistance = 13;
		private const float _portalAmbientClosedDoorMaxDistance = 4.5f;
		private const float _doorOpeningOcclusionDuration = 3;
		private const float _doorClosingOcclusionDuration = .5f;
		private const int _ambient2dFadeDuration = 2;
		private const float _ambient3dFadeDuration = .5f;

		private void PlayAmbientFromExits(Zone previousZone = null)
		{
			// Portal ambients already playing
			if (previousZone != null && _portals != null && _portals.Any(p => p.Value.AudioSource.isPlaying))
				return;

			Zone playersZone = World.Player.Zone;
			List<ReadyPortalModel> readyPortals = PreparePassageLoops();

			/* 
			 * Player leaved this zone.
			 */
			if (previousZone == this && _ambientSource != null && _ambientSource.isPlaying)
			{
				// Change 2D ambient sound to 3D and place it in the nearest passage between this and new zone. Start playing 3D ambient sounds from other passages between this and the new zone.
				ReadyPortalModel closestPortal = RemoveReadyPortalNearPlayer(readyPortals);
				MoveAmbientToPassage(closestPortal, _defaultVolume, _ambientSource);
			}

			// Start playback in The remaining exits.
			foreach (ReadyPortalModel loop in readyPortals)
				PlayPortal(loop, _defaultVolume);
		}

		private List<ReadyPortalModel> PreparePassageLoops()
		{
			Zone playersZone = World.Player.Zone;
			List<ReadyPortalModel> result = new();

			foreach (Passage passage in Exits)
			{
				Vector2 position = default;
				Vector2 player = World.Player.Area.Value.Center;

				// Player stands in the passage
				if (passage.Area.Value.Contains(player))
				{
					Zone other = passage.AnotherZone(playersZone);
					position = other.Area.Value.GetClosestPoint(player);
				}
				else
				{
					// Is the player standing in opposit to the passage?
					Vector2? tmp = passage.Area.Value.GetAlignedPoint(player);
					position = tmp != null ? tmp.Value : passage.Area.Value.GetClosestPoint(player);
				}

				if (passage.LeadsTo(playersZone))
					position = playersZone.Area.Value.GetAlignedPoint(position).Value;
				Vector3 position3d = position.ToVector3(2);

				// Make it quieter if the player is in a inadjecting zone behind a closed door.
				Zone between = playersZone.GetZonesBehindDoor().FirstOrDefault(a => a.IsBehindDoor(this));
				bool doubleAttenuation = between != null && playersZone.GetApertures(between).IsNullOrEmpty();
				float volume = passage.State == PassageState.Closed ? _defaultVolume : Sounds.GetOverClosedDoorVolume(_defaultVolume);
				if (doubleAttenuation)
					volume *= .01f;

				result.Add(new(passage, position3d, doubleAttenuation));
			}
			return result;
		}

		private void Play2dAmbient(Zone previousZone = null)
		{
			string description = $"2d ambient; {Name.Indexed}";
			if (_portals.IsNullOrEmpty())
			{
				_ambientSource = Sounds.Play2d(AmbientSound, 0, true, false, description: description);
				Sounds.SlideVolume(_ambientSource, _ambient2dFadeDuration, _defaultVolume);
				return;
			}

			/*
			 * Find a passage sound that is closest to the player, 
			] * change it to full stereo, disable Low pass and stop the rest of the passage loops.
			 */
			PortalModel portalAmbient = TakeClosestPassageLoop();
			Sounds.ConvertTo2d(portalAmbient.AudioSource, portalAmbient.Muffled);
			_ambientSource = portalAmbient.AudioSource;
			_ambientSource.name = description;

			StopPortals();
		}

		private PortalModel TakeClosestPassageLoop()
		{
			Passage closest = _portals.Keys
				.OrderBy(p => p.Area.Value.GetDistanceFrom(World.Player.Area.Value))
				.FirstOrDefault();
			if (closest == null)
				return null;

			PortalModel loop = _portals[closest];
			_portals.Remove(closest);
			return loop;
		}

		private void PlayPortal(ReadyPortalModel preparedPortalAmbient, float volume)
		{
			string description = GetPortalAmbientDescription(preparedPortalAmbient.Passage);
			PortalModel newPortalAmbient = new()
			{
				AudioSource = Sounds.Play(AmbientSound, preparedPortalAmbient.Position, 0, true, description: description)
			};
			SetDistanceAttenuation(preparedPortalAmbient, newPortalAmbient);
			UpdatePortalAmbientSpatialBlend(preparedPortalAmbient.Passage, newPortalAmbient.AudioSource);
			UpdatePortalVolume(preparedPortalAmbient.Passage, newPortalAmbient);
			SetDoorOcclusion(preparedPortalAmbient, newPortalAmbient);
			_portals[preparedPortalAmbient.Passage] = newPortalAmbient;
		}

		private void SetDistanceAttenuation(ReadyPortalModel preparedPortalAmbient, PortalModel portalAmbient)
		{
			portalAmbient.AudioSource.rolloffMode = AudioRolloffMode.Linear;
			if (preparedPortalAmbient.Passage is Door door)
			{
				if (door.State == PassageState.Open)
					portalAmbient.AudioSource.maxDistance = _portalAmbientOpenDoorMaxDistance;
				else
					portalAmbient.AudioSource.maxDistance = _portalAmbientClosedDoorMaxDistance;
			}
			else portalAmbient.AudioSource.maxDistance = _portalAmbientMaxDistance;
		}

		private void UpdatePortalVolume(Passage passage, PortalModel portalAmbient)
		{
			float targetVolume = _defaultVolume;
			float duration = _ambient2dFadeDuration;
			if (passage is Door)
			{
				if (passage.State is PassageState.Closed or PassageState.Locked)
				{
					float defaultVolume = Sounds.GetOverClosedDoorVolume(_defaultVolume);
					targetVolume = Sounds.GetLinearRolloffAttenuation(portalAmbient.AudioSource, defaultVolume);
					duration = _doorOpeningOcclusionDuration;
				}
				else duration = _doorClosingOcclusionDuration;
			}

			Sounds.SlideVolume(portalAmbient.AudioSource, duration, targetVolume);
		}

		protected string GetPortalAmbientDescription(Passage passage)
		{
			Zone[] zones = passage.Zones.ToArray();
			string description = $"3d portal ambient for {Name.Indexed}; passage between {zones[0].Name.Indexed} and {zones[1].Name.Indexed}";
			return description;
		}

		private void MoveAmbientToPassage(ReadyPortalModel preparedPortalAmbient, float volume, AudioSource stereoAmbientSound)
		{
			stereoAmbientSound.transform.position = preparedPortalAmbient.Position;
			PortalModel newPortalAmbient = new()
			{
				AudioSource = stereoAmbientSound
			};
			newPortalAmbient.AudioSource.maxDistance = _portalAmbientMaxDistance;
			string description = GetPortalAmbientDescription(preparedPortalAmbient.Passage);
			newPortalAmbient.AudioSource.name = description;
			_portals[preparedPortalAmbient.Passage] = newPortalAmbient;
			UpdatePortalAmbientSpatialBlend(preparedPortalAmbient.Passage, newPortalAmbient.AudioSource);
			UpdatePortalVolume(preparedPortalAmbient.Passage, newPortalAmbient);
			SetDoorOcclusion(preparedPortalAmbient, newPortalAmbient);
			newPortalAmbient.AudioSource.rolloffMode = AudioRolloffMode.Linear;
		}

		private void SetDoorOcclusion(ReadyPortalModel preparedPortalAmbient, PortalModel portalAmbient)
		{
			if (preparedPortalAmbient.Passage.State is PassageState.Closed or PassageState.Locked)
			{
				float frequency = preparedPortalAmbient.DoubleAttenuation ? Sounds.OverWallLowpass : Sounds.OverClosedDoorLowpass;
				Sounds.SlideLowPass(portalAmbient.AudioSource, _doorClosingOcclusionDuration, frequency);
				portalAmbient.Muffled = true;
				return;
			}

			// Door open
			if (portalAmbient.Muffled)
			{
				Sounds.SlideLowPass(portalAmbient.AudioSource, _doorOpeningOcclusionDuration, 22000, true);
				portalAmbient.Muffled = false;
			}
		}

		/// <summary>
		/// Returns all passages leading to the specified zone.
		/// </summary>
		/// <param name="zone">The zone to which the passages should lead</param>
		/// <returns> all passages leading to the specified zone</returns>
		private IEnumerable<Passage> GetPassagesTo(Zone zone) => Exits.Where(p => p.Zones.Contains(zone));

		//protected new float OverDoorVolume => .05f * _defaultVolume;

		/// <summary>
		/// Stops all isntances of background sound.
		/// </summary>
		/// <param name="fadeOut">Specifies if the loop is faded out</param>
		private void StopAmbientSounds()
		{
			if (_ambientSource != null && _ambientSource.isPlaying)
			{
				Sounds.SlideVolume(_ambientSource, _ambient2dFadeDuration, 0);
				_ambientSource = null;
			}

			StopPortals();
		}

		private void StopPortals()
		{
			foreach (PortalModel loop in _portals.Values)
				Sounds.SlideVolume(loop.AudioSource, .5f, 0);

			_portals = new();
		}
	}
}