using Assets.Scripts.Models;

using Game.Models;

using System.Collections.Generic;

namespace Game.Messaging.Commands.UI
{
	public class SelectNavigableCharacter : Message
	{
		public readonly List<NavigableCharacterInfo> Characters;

		public SelectNavigableCharacter(object sender, List<NavigableCharacterInfo> characters) : base(sender)
		{
			Characters = characters;
		}
	}
}
