using Game.UI;

using System;

namespace Game.Messaging.Events
{
    [Serializable]
    public class KeyReleased : KeyPressed
    {
        public KeyReleased(object sender, KeyShortcut shortcut) : base(sender, shortcut) { }
    }
}