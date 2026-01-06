using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands.Characters
{
	public class NavigateToCharacter:Message
	{
		public readonly Character Character;

		public NavigateToCharacter(object sender, Character character) : base(sender)
		{
			Character = character;
		}

	}
}
