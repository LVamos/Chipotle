using Game.Terrain;

using System.Collections.Generic;

namespace Game.Messaging.Events.GameInfo
{
	public class SayExitsResult : Message
	{
		public readonly List<Zone> TargetZones;

		/// <summary>
		/// Information about the exits including description of their location.
		/// </summary>
		public readonly List<List<string>> ExitDescriptions;

		/// <summary>
		/// An exit the NPC stands in.
		/// </summary>
		public readonly Passage OccupiedPassage;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="exitInfo">Information about the exits</param>
		public SayExitsResult(object sender, List<List<string>> exitDescriptions, List<Zone> targetZones) : base(sender)
		{
			ExitDescriptions = exitDescriptions;
			TargetZones = targetZones;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="occupiedPassage">An exit the NPC stands in</param>
		public SayExitsResult(object sender, Passage occupiedPassage) : base(sender)
		{
			OccupiedPassage = occupiedPassage;
			ExitDescriptions = null;
		}

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayExitsResult(object sender) : base(sender) { }
	}
}