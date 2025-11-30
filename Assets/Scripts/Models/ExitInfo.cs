using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Terrain;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Models
{
	public class ExitInfo
	{
		public Character Observer;
		public Passage Exit;
		public float Distance;
		public float Angle;
		public float StepLength;

		public ExitInfo(float distance, Passage exit, float angle,float stepLength, Character observer)
		{
			Exit = exit;
			Distance = distance;
			Angle = angle;
			StepLength= stepLength; 
			Observer = observer;
		}
	}
}
