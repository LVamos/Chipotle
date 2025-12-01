using Game.Messaging;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Messaging.Commands.Characters
{
	public class NavigateToExit:Message
	{
		public readonly Passage Exit;

		public NavigateToExit(object sender,Passage exit):base(sender)
		{
			Exit = exit;
		}
	}
}
