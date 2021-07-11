using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Luky;
using Game.UI;

namespace Game
{
	public class KeydownMessage: Message
	{
		public readonly KeyShortcut Shortcut;

		public KeydownMessage(object sender, KeyShortcut shortcut) : base(sender) 
			=> Shortcut= shortcut;
	}
}
