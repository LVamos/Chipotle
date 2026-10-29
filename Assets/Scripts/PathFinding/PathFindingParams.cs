using UnityEngine;

/// <summary>
/// Parameters used for pathfinding requests.
/// </summary>
/// <remarks>
/// Deliberately holds only plain data and the pre-built <see cref="Game.PathFinding.PathfindingSnapshot"/> -
/// no live reference to a <c>Character</c>, <c>Zone</c> or <c>World</c> - so that
/// <see cref="Game.PathFinding.PathFinder.FindPath"/> can safely run on a background thread.
/// </remarks>
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
	/// Height of the searching character's footprint (captured once, up front - not a live reference).
	/// </summary>
	public float CharacterHeight { get; set; }

	/// <summary>
	/// Width of the searching character's footprint (captured once, up front - not a live reference).
	/// </summary>
	public float CharacterWidth { get; set; }

	/// <summary>
	/// Pre-built, thread-safe snapshot of the world data the search needs.
	/// </summary>
	public Game.PathFinding.PathfindingSnapshot Snapshot { get; set; }

	/// <summary>
	/// Length of the searching character's step. The found path is split into steps of this length; zero keeps the raw grid path.
	/// </summary>
	public float StepLength { get; set; }

	public PathfindingParams(
		Vector2 start,
		Vector2 goal,
		bool sameZone,
		bool throughStart,
		bool throughGoal,
		float characterHeight,
		float characterWidth,
		Game.PathFinding.PathfindingSnapshot snapshot,
		float stepLength = 0
		)
	{
		StepLength = stepLength;
		Start = start;
		Goal = goal;
		SameZone = sameZone;
		ThroughStart = throughStart;
		ThroughGoal = throughGoal;
		CharacterHeight = characterHeight;
		CharacterWidth = characterWidth;
		Snapshot = snapshot;
	}
}
