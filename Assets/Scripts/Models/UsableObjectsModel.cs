using System.Collections.Generic;
using Game.Entities;

namespace Game.Models
{
	/// <summary>
	/// A model for PhysicsComponent.GetUsableItemsBefore.
	/// </summary>
	public class UsableObjectsModel
	{
		/// <summary>
		/// Result of the search.
		/// </summary>
		public enum ResultType
		{
			/// <summary>
			/// No item or characters before the NPC and in range
			/// </summary>
			NothingFound,
			/// <summary>
			/// Only unusable items or characters before the NPC
			/// </summary>
			Unusable,
			/// <summary>
			/// Usable items or characters before the NPC
			/// </summary>
			Success,
			/// <summary>
			/// Unreachable items or characters before the NPC
			/// </summary>
			Far
		};

		/// <summary>
		/// Usable items before the NPC.
		/// </summary>
		public readonly List<Entity> Objects;

		/// <summary>
		/// Result of the search
		/// </summary>
		public ResultType Result;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="objects">The list of items and characters.</param>
		/// <param name="result">Result of the action</param>
		public UsableObjectsModel(List<Entity> objects = null, ResultType result = ResultType.NothingFound)
		{
			Objects = objects;
			Result = result;
		}
	}
}