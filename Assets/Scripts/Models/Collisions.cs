using System.Collections.Generic;

namespace Game.Models
{
	public class Collisions
	{
		public List<object> Obstacles;
		public bool OutOfMap;

		public Collisions(List<object> obstacles, bool outOfMap)
		{
			Obstacles = obstacles;
			OutOfMap = outOfMap;
		}
	}
}