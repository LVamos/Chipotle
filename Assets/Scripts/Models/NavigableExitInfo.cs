using Game.Entities.Characters;
using Game.Terrain;

namespace Game.Models
{
	public class NavigableExitInfo:NavigableObjectInfo
	{
		public Passage Exit;
		public Zone TargetZone;
		public int ClockHour;

		public NavigableExitInfo(float distance, Passage exit, float angle, float observerStepLength, Zone targetZone, Character observer, int clockHour)
			: base(distance, angle, observerStepLength, observer)
		{
			Exit = exit;
			TargetZone = targetZone;
			ClockHour = clockHour;
		}
	}
}
