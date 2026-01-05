using Game.Entities.Characters;
using Game.Terrain;

namespace Game.Models
{
	public class NavigableExitInfo:NavigableObjectInfo
	{
		public Passage Exit;
		public Zone TargetZone;

		public NavigableExitInfo(float distance, Passage exit, float angle, float observerStepLength, Zone targetZone, Character observer)
			: base(distance, angle, observerStepLength, observer)
		{
			Exit = exit;
			TargetZone = targetZone;
		}
	}
}
