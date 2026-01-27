using Game.Messaging;
using Game.Messaging.Events.Sound;

using System.Collections.Generic;

namespace Game.Terrain
{
	public class SweeneysFrontDoor : Door
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the door</param>
		/// <param name="area">Coordinates of the area the door occupies</param>
		/// <param name="zones">The zones connected by the door</param>
		public override void Initialize(Name name, Rectangle area, IEnumerable<string> zones)
		{
			base.Initialize(name, PassageState.Locked, area, zones);
		}

		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CutsceneEnded m: OnCutsceneEnded(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		private void OnCutsceneEnded(CutsceneEnded message)
		{
			if (message.CutsceneName == "cs23")
				Unlock();
		}
	}
}
