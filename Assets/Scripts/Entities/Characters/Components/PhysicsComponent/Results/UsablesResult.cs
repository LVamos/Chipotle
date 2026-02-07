namespace Game.Entities.Characters.Components.PhysicsComponent.Results
{
	/// <summary>
	/// Result of the search.
	/// </summary>
	public enum UsablesResult
	{
		/// <summary>
		/// No objects before the NPC and in range
		/// </summary>
		NothingFound,
		/// <summary>
		/// Only unusable objects before the NPC
		/// </summary>
		Unusable,
		/// <summary>
		/// Usable objects before the NPC
		/// </summary>
		Success
	}
}
