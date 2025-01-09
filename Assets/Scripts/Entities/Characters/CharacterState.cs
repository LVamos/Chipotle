namespace Game.Entities
{
	/// <summary>
	/// Indicates what the NPC is doing in the moment.
	/// </summary>
	public enum CharacterState
	{
		/// <summary>
		/// Does nothing and waits for an event
		/// </summary>
		Waiting,

		/// <summary>
		/// In process of walking towards the player
		/// </summary>
		GoingToPlayer,

		/// <summary>
		/// In process of walking to a concrete point.
		/// </summary>
		GoingToTarget,

		/// <summary>
		/// In process of walking to a concrete point. Tuttle will then watch the player when arrives to the target.
		/// </summary>
		GoingToTargetAndWatchingPlayer,

		/// <summary>
		/// Ready to go to the player as soon as he leaves
		/// </summary>
		WatchingPlayer
	};
}