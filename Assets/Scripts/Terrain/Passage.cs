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
	/// Represents a passage between two localities.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(Door))]
	public class Passage : MapElement
	{
		public List<Vector2> GetPointsOfLocality(Locality locality)
		{
			List<Vector2> points = _area.Value.GetPoints(World.Map.TileSize).ToList();

			return
				points
				.Where(p => locality.Area.Value.Contains(p))
				.ToList();
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
		public bool IsInFrontOrBehind(Vector2 point) => IsInRelatedLocality(point) && (IsInHorizontalRange(point) || IsInVerticalRange(point));

		private bool IsInHorizontalRange(Vector2 point) => IsHorizontal() && point.x >= _area.Value.UpperLeftCorner.x && point.x <= _area.Value.UpperRightCorner.x;

		private bool IsInVerticalRange(Vector2 point)
		{
			return IsVertical()
				&& point.y >= _area.Value.LowerLeftCorner.y && point.y <= _area.Value.UpperLeftCorner.y;
		}

		/// <summary>
		/// Chekcs if the specified point lays in one of the localities connected by the passage.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>True if the specified point lays in one of the localities connected by the passage</returns>
		public bool IsInRelatedLocality(Vector2 point) => Localities.Any(l => l.Area.Value.Contains(point));

		/// <summary>
		/// Checks if the passage is horizontal.
		/// </summary>
		/// <returns>True if the passage is horizontal</returns>
		public bool IsHorizontal()
		{
			// Tests if both upper left corner and lower left corner lay in different localities (faster than World.GetLocality)
			return
				Localities.First().Area.Value.Contains(_area.Value.UpperLeftCorner)
				^ Localities.First().Area.Value.Contains(_area.Value.LowerLeftCorner);
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
		/// Checks if the passage leads to the specified locality.
		/// </summary>
		/// <param name="l">The locality to be checked</param>
		/// <returns>True if the passage leads to the specified locality</returns>
		public bool LeadsTo(Locality l) => Localities.Contains(l);

		/// <summary>
		/// Localities connected by the passage
		/// </summary>
		[ProtoIgnore]
		public IEnumerable<Locality> Localities
		{
			get
			{
				_localities ??= new string[2];

				return _localities.Select(World.GetLocality)
					.Where(l => l != null);
			}
		}

		/// <summary>
		/// Localities connected by the passage
		/// </summary>
		protected string[] _localities = new string[2];

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the passage</param>
		/// <param name="area">Coordinates of the are occupied by the passage</param>
		/// <param name="localities">Localities connected by the passage</param>
		public virtual void Initialize(Name name, Rectangle area, IEnumerable<string> localities)
		{
			base.Initialize(name, area);

			// Validate parameters
			if (
				localities == null
				|| localities.Count() != 2
				|| localities.First() == null
				|| localities.Last() == null
				|| localities.First() == localities.Last())
				throw new ArgumentException("Two different localities required");

			_sounds["navigation"] = "ExitLoop";
			_localities = localities.ToArray<string>();
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
		/// <param name="comparedLocality">The locality to be compared</param>
		/// <returns>The other side of the passage than the specified one</returns>
		public Locality AnotherLocality(Locality comparedLocality) => Localities.First(l => l.Name.Indexed != comparedLocality.Name.Indexed);

		/// <summary>
		/// Displays the passage in the game world.
		/// </summary>
		protected void Appear()
		{
			List<TileInfo> tiles = Area.Value.GetTiles();

			foreach (TileInfo info in tiles)
			{
				Locality locality = World.GetLocality(info.Position);
				if (locality != null)
					info.Tile.Edit(locality.DefaultTerrain);
			}
			foreach (Locality locality in Localities)
				locality.Register(this);

			foreach (Locality l in Localities)
				l.Register(this);
		}

		/// <summary>
		/// Erases the passage from the game world.
		/// </summary>
		protected void Disappear()
		{
			foreach (Locality l in Localities)
				l.Unregister(this);
		}

		/// <summary>
		/// Returns the point closest to the player.
		/// </summary>
		protected Vector2 GetClosestPointToPlayer() => _area.Value.GetClosestPoint(World.Player.Area.Value.Center);

		/// <summary>
		/// stores a locality in which the player is located after navigation start.
		/// </summary>
		[ProtoIgnore]
		protected Locality _playersLocality;

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

			UpdateNavigatingSound();
		}
	}
}