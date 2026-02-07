using Game.Entities;
using Game.Terrain;

using System.Collections.Generic;

namespace Game.Models
{
	/// <summary>
	/// A model for PhysicsComponent.GetUsableObjectsBefore.
	/// </summary>
	public class Usables
	{
		/// <summary>
		/// Result of the search.
		/// </summary>
		public enum ResultType
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
			Success,
			/// <summary>
			/// Unreachable objects before the NPC
			/// </summary>
			Far
		};

		/// <summary>
		/// Usable items before the NPC.
		/// </summary>
		public readonly List<MapElement> Objects;

		/// <summary>
		/// Result of the search
		/// </summary>
		public ResultType Result;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="objects">The list of items and characters.</param>
		/// <param name="result">Result of the action</param>
		public Usables(List<MapElement> objects = null, ResultType result = ResultType.NothingFound)
		{
			Objects = objects;
			Result = result;
		}
	}
}