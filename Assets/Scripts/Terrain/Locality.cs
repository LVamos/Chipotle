﻿using Assets.Scripts.Models;

using Game.Audio;
using Game.Entities;
using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.VisualScripting;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
	/// <summary>
	/// Represents one region on the game map (e.g. a room).
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class Locality : MapElement
	{
		private bool PlayerInSoundRadius
			=> GetDistanceFromPlayer() <= _localitySoundRadius;

		/// <summary>
		/// Surfacematerials for walls, floor and ceiling
		/// </summary>
		public LocalityMaterialsDefinitionModel Materials { get; private set; }

		[ProtoIgnore]
		private AudioSource _ambientSource;

		/// <summary>
		/// Enumerates all passages leading to the specified locality.
		/// </summary>
		/// <param name="l">The target locality</param>
		/// <returns>enumaretion of passages</returns>
		public IEnumerable<Passage> GetExitsTo(Locality l)
		{
			return Passages.Where(p => p.LeadsTo(l));
		}

		/// <summary>
		/// Indicates if a locality is inside a building or outside.
		/// </summary>
		public enum LocalityType
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
		/// Enumerates all accessible localities.
		/// </summary>
		/// <returns>All accessible localities</returns>
		public IEnumerable<Locality> GetAccessibleLocalities()
		{
			return Passages.Select(p => p.AnotherLocality(this)).Distinct();
		}

		/// <summary>
		/// Handles the EntityMoved message.
		/// </summary>
		/// <param name="message">The mesage to be handled</param>
		private void OnCharacterMoved(CharacterMoved message)
		{
			if (message.Sender == World.Player)
				UpdatePassageLoops();
		}

		/// <summary>
		/// Updates position of passage sound loops.
		/// </summary>
		private void UpdatePassageLoops()
		{
			foreach (Passage passage in _passageLoops.Keys)
			{
				Vector2 player = World.Player.Area.Value.Center;
				AudioSource source = _passageLoops[passage].AudioSource;

				// If the player is standing right in the passage locate the sound right on his position.
				if (passage.Area.Value.Intersects(player))
				{
					source.transform.position = player.ToVector3(2);
					continue;
				}

				Vector2? point = passage.Area.Value.FindAlignedPoint(player)
				?? passage.Area.Value.GetClosestPoint(player);
				source.transform.position = point.Value.ToVector3(2);

				// Update spatial blend
				int distance = (int)GetDistanceFromPlayer();
				source.spatialBlend = distance > 10 ? 1 : distance * .1f;
			}
		}

		/// <summary>
		/// Checks if the specified point lays in front or behind a passage.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>The passage by in front or behind which the specified point lays or null if nothing found</returns>
		public Passage IsAtPassage(Vector2 point)
		{
			return Passages.FirstOrDefault(p => p.IsInFrontOrBehind(point));
		}

		/// <summary>
		/// Checks if the specified entity is in any neighbour locality.
		/// </summary>
		/// <param name="e">The entity to be checked</param>
		/// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
		public Locality IsInNeighbourLocality(Character e)
		{
			return Neighbours.FirstOrDefault(l => l.IsItHere(e));
		}

		/// <summary>
		/// Checks if the specified object is in any neighbour locality.
		/// </summary>
		/// <param name="o">The object to be checked</param>
		/// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
		public Locality IsInNeighbourLocality(Item o)
		{
			return Neighbours.FirstOrDefault(l => l.IsItHere(o));
		}

		/// <summary>
		/// Checks if the specified entity is in any accessible neighbour locality.
		/// </summary>
		/// <param name="e">The entity to be checked</param>
		/// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
		public Locality IsInAccessibleLocality(Character e)
		{
			return GetLocalitiesBehindDoor().FirstOrDefault(l => l.IsItHere(e));
		}

		/// <summary>
		/// Checks if the specified object is in any accessible neighbour locality.
		/// </summary>
		/// <param name="o">The object to be checked</param>
		/// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
		public Locality IsInAccessibleLocality(Item o)
		{
			return GetLocalitiesBehindDoor().FirstOrDefault(l => l.IsItHere(o));
		}

		/// <summary>
		/// Returns all open passages.
		/// </summary>
		/// <returns>Enumeration of all open passages</returns>
		public IEnumerable<Passage> GetApertures()
		{
			return Passages.Where(p => p.State == PassageState.Open);
		}

		/// <summary>
		/// Chekcs if the specified locality is accessible from this locality.
		/// </summary>
		/// <param name="l">The locality to be checked</param>
		/// <returns>True if the specified locality is accessible form this locality</returns>
		public bool IsBehindDoor(Locality l)
		{
			return GetLocalitiesBehindDoor().Any(locality => locality.Name.Indexed == l.Name.Indexed);
		}

		/// <summary>
		/// Checks if it's possible to get to the specified locality from this locality over doors or open passages.
		/// </summary>
		/// <param name="locality">The target locality</param>
		/// <returns>True if there's a way between this loclaity and the specified locality</returns>
		public bool IsAccessible(Locality locality)
		{
			return GetAccessibleLocalities().Any(l => l.Name.Indexed == locality.Name.Indexed);
		}

		/// <summary>
		/// Checks if the specified locality is next to this locality.
		/// </summary>
		/// <param name="l">The locality to be checked</param>
		/// <returns>True if the speicifed locality is adjecting to this locality</returns>
		public bool IsNeighbour(Locality l)
		{
			return Neighbours.Contains(l);
		}

		/// <summary>
		/// Maps all adejcting localities.
		/// </summary>
		private void FindNeighbours()
		{
			Rectangle a = Area.Value;
			a.Extend();
			_neighbours =
			(
				from p in a.GetPerimeterPoints()
				let l = World.GetLocality(p)
				where l != null
				select l.Name.Indexed
			).Distinct().ToList();
		}

		/// <summary>
		/// List of adjecting localities
		/// </summary>
		private List<string> _neighbours;

		/// <summary>
		/// List of adjecting localities
		/// </summary>
		[ProtoIgnore]
		public IEnumerable<Locality> Neighbours => _neighbours.Select(World.GetLocality);

		/// <summary>
		/// Returns all open passages between this locality and the specified one..
		/// </summary>
		/// <returns>Enumeration of all open passages between this locality and the specified one</returns>
		public IEnumerable<Passage> GetApertures(Locality l)
		{
			return GetApertures().Where(p => p.LeadsTo(l));
		}

		/// <summary>
		/// Enumerates passages ordered by distance from the specified point.
		/// </summary>
		/// <param name="point">The default point</param>
		/// <param name="radius">distance in which exits from current locality are searched</param>
		/// <returns>Enumeration of passages</returns>
		public IEnumerable<Passage> GetNearestExits(Vector2 point, int radius)
		{
			return
				from e in Passages
				where !e.Area.Value.Intersects(point) && World.GetDistance(e.Area.Value.GetClosestPoint(point), point) <= radius
				orderby e.Area.Value.GetDistanceFrom(point)
				select e;
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
			IEnumerable<Item> items = Objects.Where(o => o.Area != null);

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
		/// Height of ceiling of the locality (0 in case of outdoor localities)
		/// </summary>
		public readonly float Ceiling;

		/// <summary>
		/// All exits from the locality
		/// </summary>
		[ProtoIgnore]
		public IEnumerable<Passage> Passages
		{
			get
			{
				_passages ??= new();

				return _passages.Select(World.GetPassage).Where(p => p != null);
			}
		}

		/// <summary>
		/// Text description of the locality
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Name of the locality in a shape that expresses a direction to the locality.
		/// </summary>
		public string To { get; private set; }

		/// <summary>
		/// Specifies if the locality is outside or inside a building.
		/// </summary>
		public LocalityType Type { get; private set; }

		/// <summary>
		/// The minimum permitted Y dimension of the floor in this locality
		/// </summary>
		private const int MinimumHeight = 3;

		/// <summary>
		/// The minimum permitted X dimension of the floor in this locality
		/// </summary>
		private const int MinimumWidth = 3;

		/// <summary>
		/// Height of ceiling of the locality (0 in case of outdoor localities)
		/// </summary>
		private float _ceiling;

		/// <summary>
		/// List of NPCs present in this locality.
		/// </summary>
		private HashSet<string> _characters = new();

		/// <summary>
		/// List of objects present in this locality.
		/// </summary>
		private HashSet<string> _objects = new();

		/// <summary>
		/// List of exits from this locality
		/// </summary>
		private HashSet<string> _passages = new();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name of the locality</param>
		/// <param name="to">
		/// Name of the locality in a shape that expresses a direction to the locality
		/// </param>
		/// <param name="type">Specifies if the locality is outside or inside a building.</param>
		/// <param name="ceiling">Ceiling height of the locality (should be 0 for outdoor localities)</param>
		/// <param name="area">Coordinates of the area occupied by the locality</param>
		/// <param name="defaultTerrain">Lowest layer of the terrain in the locality</param>
		/// <param name="backgroundInfo">A background sound played in loop</param>
		public void Initialize(Name name, string description, string to, LocalityType type, float ceiling, Rectangle area, TerrainType defaultTerrain, string loop, float volume, LocalityMaterialsDefinitionModel materials = null)
		{
			base.Initialize(name, area);

			Description = description;
			To = to;
			Type = type;
			_ceiling = Type == LocalityType.Outdoor && ceiling <= 2 ? 0 : ceiling;
			_ceiling = type == LocalityType.Outdoor ? 0 : ceiling;
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
		/// Defines the lowest layer of terrain in the locality.
		/// </summary>
		[ProtoIgnore]
		public TerrainType DefaultTerrain { get; private set; }

		/// <summary>
		/// List of NPCs present in this locality.
		/// </summary>
		[ProtoIgnore]
		public IEnumerable<Character> Characters => _characters.Select(World.GetCharacter);

		/// <summary>
		/// List of objects present in this locality.
		/// </summary>
		[ProtoIgnore]
		public IEnumerable<Item> Objects
		{
			get
			{
				_objects ??= new();

				return _objects.Select(World.GetItem).Where(o => o != null);
			}
		}

		/// <summary>
		/// Returns all adjecting localities to which it's possible to get from this locality.
		/// </summary>
		/// <returns>An enumeration of all adjecting accessible localities</returns>
		public IEnumerable<Locality> GetLocalitiesBehindDoor()
		{
			return Passages.Select(p => p.AnotherLocality(this));
		}

		/// <summary>
		/// Checks if the specified game object is present in this locality in the moment.
		/// </summary>
		/// <param name="o">The object to be checked</param>
		/// <returns>True if the object is present in the locality</returns>
		public bool IsItHere(Item o)
		{
			return _objects.Contains(o.Name.Indexed);
		}

		/// <summary>
		/// Checks if an entity is present in this locality in the moment.
		/// </summary>
		/// <param name="e">The entity to be checked</param>
		/// <returns>True if the entity is here</returns>
		public bool IsItHere(Character e)
		{
			return Characters.Contains(e);
		}

		/// <summary>
		/// Checks if a passage lays in the locality.
		/// </summary>
		/// <param name="p">The passage to be checked</param>
		/// <returns>True if the passage lays in this locality</returns>
		public bool IsItHere(Passage p)
		{
			return Passages.Contains(p);
		}

		/// <summary>
		/// Gets a message from another messaging object and stores it for processing.
		/// </summary>
		/// <param name="message">The message to be received</param>
		/// <param name="routeToNeighbours">Specifies if the message should be distributed to the neighbours of this locality</param>
		public void TakeMessage(Message message, bool routeToNeighbours)
		{
			TakeMessage(message);

			if (routeToNeighbours)
			{
				foreach (Locality neighbour in Neighbours)
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

			MessageObjects(message);
			MessageEntities(message);
			MessagePassages(message);
		}

		/// <summary>
		/// Adds an passage to the locality.
		/// </summary>
		/// <param name="p">The passage to be added</param>
		public void Register(Passage p)
		{
			// Check if exit isn't already in list
			if (IsItHere(p))
				throw new InvalidOperationException("exit already registered");

			_passages.Add(p.Name.Indexed);
		}

		/// <summary>
		/// Adds a game object to list of present objects.
		/// </summary>
		/// <param name="o">The object ot be added</param>
		private void Register(Item o)
		{
			_objects.Add(o.Name.Indexed);
		}

		/// <summary>
		/// Adds an entity to locality.
		/// </summary>
		/// <param name="c">The entity to be added</param>
		public void Register(Character c)
		{
			_characters.Add(c.Name.Indexed);
		}

		/// <summary>
		/// Initializes the locality and starts its message loop.
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
				case ObjectDisappearedFromLocality m: OnObjectDisappearedFromLocality(m); break;
				case ObjectAppearedInLocality m: OnObjectAppearedInLocality(m); break;
				case ChipotlesCarMoved ccmv: OnChipotlesCarMoved(ccmv); break;
				case CharacterMoved em: OnCharacterMoved(em); break;
				case DoorManipulated dm: OnDoorManipulated(dm); break;
				case Reloaded gr: OnGameReloaded(); break;
				case CharacterLeftLocality ll: OnCharacterLeftLocality(ll); break;
				case CharacterCameToLocality le: OnCharacterCameToLocality(le); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		private void OnObjectDisappearedFromLocality(ObjectDisappearedFromLocality m)
		{
			Unregister(m.Object);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="m">The message to be handled</param>
		private void OnObjectAppearedInLocality(ObjectAppearedInLocality m)
		{
			Register(m.Object);
		}

		/// <summary>
		/// Handles the ChipotlesCarMoved message.
		/// </summary>
		/// <param name="message"The message to be handled></param>
		private void OnChipotlesCarMoved(ChipotlesCarMoved message)
		{
			if (message.Target.GetLocalities() != this)
				StopAmbientSounds(true);
		}

		/// <summary>
		/// Handles the DoorManipulated message.
		/// </summary>
		/// <param name="message">Source of the message</param>
		private void OnDoorManipulated(DoorManipulated message)
		{
			UpdateLoop();
		}

		/// <summary>
		/// Indicates if the player is in the locality.
		/// </summary>
		/// <returns>True if the player is in the locality.</returns>
		[ProtoIgnore]
		private bool _playerInHere;

		/// <summary>
		/// Immediately removes a game object from list of present objects.
		/// </summary>
		/// <param name="o"></param>
		private void Unregister(Entity o)
		{
			_objects.Remove(o.Name.Indexed);
		}

		/// <summary>
		/// Immediately removes an entity from list of present entities.
		/// </summary>
		/// <param name="e">The entity to be removed</param>
		public void Unregister(Character e)
		{
			_characters.Remove(e.Name.Indexed);
		}

		/// <summary>
		/// Removes a passage from the locality.
		/// </summary>
		/// <param name="p">The passage to be removed</param>
		public void Unregister(Passage p)
		{
			if (!Passages.Contains(p))
				throw new InvalidOperationException("Unregistered passage");

			_passages.Remove(p.Name.Indexed);
		}

		/// <summary>
		/// Erases the locality from game world.
		/// </summary>
		protected void Disappear()
		{
			// Delete objects.
			foreach (Item o in Objects)
				World.Remove(o);

			// Delete passages.
			foreach (Passage p in Passages)
				World.Remove(p);

			// Delete character.
			foreach (Character e in Characters)
				World.Remove(e);

			// Delete locality from the map.
			foreach (Vector2 p in _area.Value.GetPoints())
				World.Map[p] = null;
		}

		/// <summary>
		/// Sends a message to all game objects in the locality.
		/// </summary>
		/// <param name="message">The message to be sent</param>
		protected void MessageObjects(Message message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			foreach (Item o in Objects)
			{
				if (o != message.Sender)
					o.TakeMessage(message);
			}
		}

		private void MessageEntities(Message message)
		{
			foreach (Character e in Characters)
			{
				if (e != message.Sender)
					e.TakeMessage(message);
			}
		}

		/// <summary>
		/// Handles the Reloaded message.
		/// </summary>
		private void OnGameReloaded()
		{
			_passageLoops = new();
			_playerInHere = IsItHere(World.Player);
			UpdateLoop();
		}

		/// <summary>
		/// Handles the LocalityEntered message.
		/// </summary>
		/// <param name="message">The message</param>
		private void OnCharacterCameToLocality(CharacterCameToLocality message)
		{
			if (message.Locality != this)
				return;

			Register(message.Character);
			if (message.Character != World.Player)
				return;

			Sounds.SetRoomParameters(this);
			_playerInHere = true;
			UpdateLoop();
		}

		/// <summary>
		/// Distributes a game message to all passages.
		/// </summary>
		/// <param name="message">The message to be sent</param>
		private void MessagePassages(Message message)
		{
			foreach (Passage p in Passages)
			{
				if (p != message.Sender)
					p.TakeMessage(message);
			}
		}

		/// <summary>
		/// Handles the LocalityLeft message.
		/// </summary>
		/// <param name="message">The message</param>
		private void OnCharacterLeftLocality(CharacterLeftLocality message)
		{
			if (message.Locality != this)
				return;

			Unregister(message.Sender as Character);

			if (message.Character != World.Player)
				return;

			_playerInHere = false;
			UpdateLoop();
		}

		/// <summary>
		/// Name of background sound played in loop.
		/// </summary>
		public string AmbientSound;

		/// <summary>
		/// Plays the background sound of this locality in a loop.
		/// </summary>
		/// <param name="playerMoved">Specifies if the player just moved from one locality to another one.</param>
		private void UpdateLoop()
		{
			StopInaudibleSounds();

			if (string.IsNullOrWhiteSpace(AmbientSound))
				return;

			if (_playerInHere)
				PlayAmbientHere();

			Locality playersLocality = World.Player.Locality;
			if (string.Equals(playersLocality.AmbientSound, AmbientSound, StringComparison.InvariantCultureIgnoreCase))
				return;

			if (IsAccessible(playersLocality))
				PlayAmbientAccessible();
			else if (PlayerInSoundRadius)
				PlayAmbientInaccessible();
		}

		/// <summary>
		/// Playback mode for background sounds
		/// </summary>
		private SoundMode _soundMode;

		/// <summary>
		/// Defines playback modes for locality background sound based on player's proximity.
		/// </summary>
		public enum SoundMode
		{
			/// <summary>
			/// If the player is in the locality, the sound plays in full stereo.
			/// </summary>
			InLocality,

			/// <summary>
			/// If the player is in an accessible locality, the sound plays softly from every passage leading to the player's current locality.
			/// </summary>
			InAccessibleLocality,

			/// <summary>
			/// If the player is far, the sound plays softly from the center of the locality, provided the player is within the sound's radius defined by _soundRadius.
			/// </summary>
			InInaccessibleLocality
		}

		private void StopInaudibleSounds()
		{
			if (PlayerInSoundRadius)
				return;

			Stop(ref _ambientSource);
			foreach (PassageLoopModel loop in _passageLoops.Values)
				Stop(ref loop.AudioSource);

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
		private Dictionary<Passage, PassageLoopModel> _passageLoops = new();

		private bool _reloaded;

		/// <summary>
		/// The maximum distance the player is from the locality at which it makes sense to play the location audio.
		/// </summary>
		private const float _localitySoundRadius = 100;

		/// <summary>
		/// Plays soudn loop of the locality.
		/// </summary>
		/// <param name="playerInHere">Determines if the player is in this locality.</param>
		/// <param name="accessible">Determines if the player is in an neighbour accessible locality.</param>
		protected void PlayAmbientInaccessible()
		{
			Vector3 position = new(_area.Value.Center.x, 2, _area.Value.Center.y);

			if (_soundMode == SoundMode.InInaccessibleLocality)
				_ambientSource = TakeClosestPassageLoop()?.AudioSource;

			if (_ambientSource != null)
			{
				_ambientSource.spatialBlend = 1;
				_ambientSource.transform.position = position;
			}
			else
				_ambientSource = Sounds.Play(AmbientSound, position, _defaultVolume, true);

			Sounds.SetLowPass(_ambientSource, Sounds.OverWallLowpass);
			_soundMode = SoundMode.InInaccessibleLocality;
		}

		private void PlayAmbientAccessible()
		{
			Locality playersLocality = World.Player.Locality;
			List<PreparedPassageLoopModel> loops = PreparePassageLoops();

			/* 
			 * If the ambient sounds are already playing in the player's locality, 
			 * use a sound source from a passage closest to the player.
			 */
			if (_ambientSource != null && _ambientSource.isPlaying)
			{
				IEnumerable<Passage> passages = loops.Select(loop => loop.Passage);
				Passage closestPassage = World.GetClosestElement(passages, World.Player) as Passage;
				PreparedPassageLoopModel closestLoop;
				closestLoop = loops.First(loop => loop.Passage == closestPassage);
				loops.Remove(closestLoop);

				PlayPassageLoop(closestLoop, _defaultVolume, _ambientSource);
			}

			// Start playback in The remaining exits.
			foreach (PreparedPassageLoopModel loop in loops)
				PlayPassageLoop(loop, _defaultVolume);

			_soundMode = SoundMode.InAccessibleLocality;
		}

		private List<PreparedPassageLoopModel> PreparePassageLoops()
		{
			Locality playersLocality = World.Player.Locality;
			List<Passage> exits = GetExitsTo(playersLocality).ToList();
			List<PreparedPassageLoopModel> result = new();

			foreach (Passage passage in exits)
			{
				Vector2 position = default;
				Vector2 player = World.Player.Area.Value.Center;

				// Player stands in the passage
				if (passage.Area.Value.Intersects(player))
				{
					Locality other = passage.AnotherLocality(playersLocality);
					position = other.Area.Value.GetClosestPoint(player);
				}
				else
				{
					// Is the player standing in opposit to the passage?
					Vector2? tmp = passage.Area.Value.FindAlignedPoint(player);
					position = tmp != null ? tmp.Value : passage.Area.Value.GetClosestPoint(player);
				}
				Vector3 position3d = new(position.x, 2, position.y);

				// Make it quieter if the player is in a inadjecting locality behind a closed door.
				Locality between = playersLocality.GetLocalitiesBehindDoor().FirstOrDefault(a => a.IsBehindDoor(this));
				bool doubleAttenuation = between != null && playersLocality.GetApertures(between).IsNullOrEmpty();
				float volume = passage.State == PassageState.Closed ? _defaultVolume : OverDoorVolume;
				if (doubleAttenuation)
					volume *= .01f;

				result.Add(new(passage, position3d, doubleAttenuation));
			}
			return result;
		}

		private void PlayAmbientHere()
		{
			SoundMode oldMode = _soundMode;
			_soundMode = SoundMode.InLocality;

			if (oldMode == SoundMode.InInaccessibleLocality)
			{
				_ambientSource.spatialBlend = 0;
				return;
			}

			if (_ambientSource == null || !_ambientSource.isPlaying)
			{
				_ambientSource = Sounds.Play2d(AmbientSound, _defaultVolume, true, true);
				return;
			}

			return;
			/*
			 * Switching from InInaccessibleLocality mode
			 * Find a passage sound that is closest to the player, 
			 * change it to full stereo, disable Low pass and stop the rest of the passage loops.
			 */
			PassageLoopModel loop = TakeClosestPassageLoop();
			loop.AudioSource.spatialBlend = 0;
			if (loop.Muffled)
				Sounds.DisableLowpass(loop.AudioSource);

			StopPassageLoops();
			_ambientSource = loop.AudioSource;
		}

		private PassageLoopModel TakeClosestPassageLoop()
		{
			Passage closest = World.GetClosestElement(Passages, World.Player) as Passage;
			PassageLoopModel passageLoop = _passageLoops[closest];
			_passageLoops.Remove(closest);
			return passageLoop;
		}

		private void PlayPassageLoop(PreparedPassageLoopModel loop, float volume)
		{
			PassageLoopModel model = new()
			{
				AudioSource = Sounds.Play(AmbientSound, loop.Position, _defaultVolume, true, true)
			};
			_passageLoops[loop.Passage] = model;
			ApplyLowPass(loop, model);
			model.AudioSource.volume = 0;
			Sounds.SlideVolume(model.AudioSource, .5f, _defaultVolume);
		}

		private void PlayPassageLoop(PreparedPassageLoopModel loop, float volume, AudioSource stereoAmbientSound)
		{
			stereoAmbientSound.transform.position = loop.Position;
			PassageLoopModel model = new()
			{
				AudioSource = stereoAmbientSound
			};
			_passageLoops[loop.Passage] = model;
			ApplyLowPass(loop, model);
		}

		private static void ApplyLowPass(PreparedPassageLoopModel loop, PassageLoopModel model)
		{
			if (loop.Passage.State is PassageState.Closed or PassageState.Locked)
			{
				int frequency = loop.DoubleAttenuation ? Sounds.OverWallLowpass : Sounds.OverDoorLowpass;
				Sounds.SetLowPass(model.AudioSource, frequency);
				model.Muffled = true;
			}
		}

		/// <summary>
		/// Returns all passages leading to the specified locality.
		/// </summary>
		/// <param name="locality">The locality to which the passages should lead</param>
		/// <returns> all passages leading to the specified locality</returns>
		private IEnumerable<Passage> GetPassagesTo(Locality locality)
		{
			return Passages.Where(p => p.Localities.Contains(locality));
		}

		//protected new float OverDoorVolume => .05f * _defaultVolume;

		/// <summary>
		/// Stops all isntances of background sound.
		/// </summary>
		/// <param name="fadeOut">Specifies if the loop is faded out</param>
		private void StopAmbientSounds(bool fadeOut = false)
		{
			if (_ambientSource != null && _ambientSource.isPlaying)
			{
				Sounds.SlideVolume(_ambientSource, .5f, 0);
				_ambientSource = null;
			}

			StopPassageLoops();
		}

		private void StopPassageLoops()
		{
			foreach (PassageLoopModel loop in _passageLoops.Values)
				Sounds.SlideVolume(loop.AudioSource, .5f, 0);

			_passageLoops = new();
		}
	}
}