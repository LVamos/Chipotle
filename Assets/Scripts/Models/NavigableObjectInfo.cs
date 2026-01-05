using Game.Entities.Characters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Models
{
	public class NavigableObjectInfo
	{
		public float Angle;
		public float Distance;
		public float ObserverStepLength;
		public Character Observer;

		public NavigableObjectInfo(float distance, float angle, float observerStepLength, Character observer)
		{
			Angle = angle;
			Distance = distance;
			ObserverStepLength = observerStepLength;
			Observer = observer;
		}
	}
}
