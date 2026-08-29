using System.Collections.Generic;

using UnityEngine;

namespace Game.PathFinding
{
	/// <summary>
	/// An immutable, self-contained snapshot of everything <see cref="PathFinder.FindPath"/> needs to
	/// know about the game world for one search.
	/// </summary>
	/// <remarks>
	/// Built synchronously on the main thread by <see cref="PathFinder.BuildSnapshot"/>, before a search
	/// is handed off to a background thread. <see cref="PathFinder.FindPath"/> itself never touches
	/// <c>World</c>, <c>Zone</c> or <c>Character</c> - all of those are live, mutable, and get changed
	/// on the main thread during normal gameplay (a character's <c>Area</c> on every step, a zone's set
	/// of characters on every zone change, a zone's non-walkable tiles whenever an item appears), so
	/// reading them from a background thread while the main thread might be writing to them at the same
	/// moment would be a race. A plain-data snapshot sidesteps that entirely: nothing on the background
	/// thread is shared with anything the main thread can mutate.
	/// </remarks>
	public sealed class PathfindingSnapshot
	{
		/// <summary>
		/// The area the snapshot covers. No node outside this box is ever considered by the search, so
		/// nothing outside it needs to be present in <see cref="WalkablePositions"/>.
		/// </summary>
		public readonly BoundingBox Box;

		/// <summary>
		/// Positions (already snapped to the tile grid) that are walkable for a single tile - terrain is
		/// passable, the tile isn't covered by a static obstacle, and it isn't occupied by another
		/// character's footprint. Anything not in this set - including positions outside the map
		/// entirely - is treated as blocked.
		/// </summary>
		public readonly HashSet<Vector2> WalkablePositions;

		public PathfindingSnapshot(BoundingBox box, HashSet<Vector2> walkablePositions)
		{
			Box = box;
			WalkablePositions = walkablePositions;
		}
	}
}
