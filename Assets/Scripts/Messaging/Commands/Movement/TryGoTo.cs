

using UnityEngine;

using Message = Game.Messaging.Message;

/// <summary>
/// Instructs a physics component of an entity to walk to one of the specified points if possible.
/// </summary>
public class TryGoTo : Message
{
	/// <summary>
	/// List of points to try.
	/// </summary>
	public readonly Vector2[] Points;

	/// <summary>
	/// Specifies if entity should watch the player during the walk.
	/// </summary>
	public readonly bool WatchPlayer;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender">Source of the message</param>
	/// <param name="points">List of the points to try</param>
	/// <param name="watchPlayer">Specifies if entity should watch the player during the walk.</param>
	public TryGoTo(object sender, Vector2[] points, bool watchPlayer = false) : base(sender)
	{
		Points = points;
		WatchPlayer = watchPlayer;
	}
}