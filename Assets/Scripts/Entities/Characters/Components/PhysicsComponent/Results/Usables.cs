using Game.Terrain;

using System.Collections.Generic;

namespace Game.Entities.Characters.Components.PhysicsComponent.Results
{
	/// <summary>
	/// A model for PhysicsComponent.GetUsableObjectsBefore.
	/// </summary>
	public class Usables
	{
		/// <summary>
		/// Usable items before the NPC.
		/// </summary>
		public readonly List<MapElement> Objects;

		/// <summary>
		/// Result of the search
		/// </summary>
		public UsablesResult Result;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="objects">The list of items and characters.</param>
		/// <param name="result">Result of the action</param>
		public Usables(List<MapElement> objects = null, UsablesResult result = UsablesResult.NothingFound)
		{
			Objects = objects;
			Result = result;
		}
	}
}