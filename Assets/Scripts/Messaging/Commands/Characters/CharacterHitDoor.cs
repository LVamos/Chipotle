using Game.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands.Characters
{
	public class CharacterHitDoor:Message
	{
		public readonly ExitInfo Exit;

		public CharacterHitDoor(object sender,ExitInfo exit):base(sender)
		{
			Exit = exit;
		}
	}
}
