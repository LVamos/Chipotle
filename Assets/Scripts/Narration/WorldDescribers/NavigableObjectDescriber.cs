using Game.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Narration.WorldDescribers
{
	public abstract class NavigableObjectDescriber
	{
		public abstract List<string> GetDescriptions(List<NavigableObjectInfo> records);

		public abstract List<string> GetStructuredDescription(NavigableObjectInfo info);


		public abstract List<List<string>> GetStructuredDescriptions(List<NavigableObjectInfo> records);

		public string GetDescription(NavigableObjectInfo record)
		{
			List<string> result = GetStructuredDescription(record);
			return string.Join(' ', result);
		}

		/// <summary>
		/// Generates a text representation of the specified distance in Czech.
		/// </summary>
		/// <returns>The text representation of the specified distance</returns>
		protected string GetDistanceDescription(NavigableObjectInfo info)
		{
			if (info.Distance <= info.ObserverStepLength)
				return string.Empty;

			// Round the distance so that its value corresponds to a multiple of 0.5.
			int steps = (int)Math.Round(info.Distance / info.ObserverStepLength);

			// Compose output
			if (steps == 1)
				return "jeden krok";
			if (steps is > 1 and < 5)
				return $"{steps} kroky";
			return $"{steps} kroků";
		}

	}
}
