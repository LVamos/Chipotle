using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Models
{
	public class NavigableCharacterInfo:NavigableObjectInfo
	{
		public Character Character;

		public NavigableCharacterInfo(float distance, Character character, float angle, float observerStepLength,Character observer)
			:base(distance, angle, observerStepLength, observer)
		{
			Character = character;
		}
	}
}
