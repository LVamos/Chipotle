namespace Game.Entities.Characters.Components.PhysicsComponent.Results
{

	/// <summary>
	/// Result of the search.
	/// </summary>
	public enum PickablesResult
	{
		/// <summary>
		/// No items before the NPC and in range
		/// </summary>
		NothingFound,
		/// <summary>
		/// Only unpickable items before the NPC
		/// </summary>
		Unpickable,
		/// <summary>
		/// Pickable items before the NPC
		/// </summary>
		Success
	}
}
