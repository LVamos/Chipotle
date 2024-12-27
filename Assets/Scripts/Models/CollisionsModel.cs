using System.Collections.Generic;

namespace Game.Models
{
	public class CollisionsModel
	{
		public List<object> Obstacles;
		public bool OutOfMap;

		public CollisionsModel(List<object> obstacles, bool outOfMap)
		{
			Obstacles = obstacles;
			OutOfMap = outOfMap;
		}
	}
}