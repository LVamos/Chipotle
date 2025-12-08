using System.Collections.Generic;

namespace Assets.Scripts.Models
{
	public class ZoneLoopInfo
	{
		public List<string> PortalAudibleZones;
		public string Sound;
		public float Volume;
		public float? PortalMaxDistance;
		public float? OpenDoorMaxDistance;
		public float? ClosedDoorMaxDistance;
	}
}
