using Game.Entities.Characters;
using Game.Terrain;

using UnityEngine;

/// <summary>
/// Parameters used for pathfinding requests.
/// </summary>
public class PathfindingParams
{
	/// <summary>
	/// Start position of the path.
	/// </summary>
	public Vector2 Start { get; set; }

	/// <summary>
	/// Goal position of the path.
	/// </summary>
	public Vector2 Goal { get; set; }

	/// <summary>
	/// Whether start and goal are in the same zone.
	/// </summary>
	public bool SameZone { get; set; }

	/// <summary>
	/// Whether path must pass through the start position.
	/// </summary>
	public bool ThroughStart { get; set; }

	/// <summary>
	/// Whether path must pass through the goal position.
	/// </summary>
	public bool ThroughGoal { get; set; }

	/// <summary>
	/// Character reference used in pathfinding.
	/// </summary>
	public Character IgnoredCharacter { get; set; }

	public PathfindingParams(
		Vector2 start,
		Vector2 goal,
		bool sameZone,
		bool throughStart,
		bool throughGoal,
		Character ignoredCharacter
		)
	{
		Start = start;
		Goal = goal;
		SameZone = sameZone;
		ThroughStart = throughStart;
		ThroughGoal = throughGoal;
		IgnoredCharacter = ignoredCharacter;
	}
}
