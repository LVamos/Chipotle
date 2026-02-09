using System.Collections.Generic;

namespace Game.Models
{
	public class Collisions
	{
		public HashSet<object> Obstacles;
		public bool OutOfMap;

		public Collisions(HashSet<object> obstacles = null, bool outOfMap = false)
		{
			Obstacles = obstacles;
			OutOfMap = outOfMap;
		}
	}
}