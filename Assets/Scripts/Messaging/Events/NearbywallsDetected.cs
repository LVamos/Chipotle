using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Messaging.Events
{
	/// <summary>
	/// Message that indicates that walls were detected near a NPC.
	/// </summary>
	[Serializable]
	public class NearbywallsDetected : Message
	{
		/// <summary>
		/// List of points that belong to nearby walls.
		/// </summary>
		/// <remarks>There's only one point per each wall</remarks>
		public readonly List<Vector2> Points;

		public NearbywallsDetected(object sender, List<Vector2> points) : base(sender)
		{
			Points = points ?? throw new ArgumentNullException(nameof(points));
		}
	}
}
