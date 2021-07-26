using Game.UI;

namespace Game.Messaging.Events
{
    public class KeyPressed : GameMessage
    {
        public readonly KeyShortcut Shortcut;

        public KeyPressed(object sender, KeyShortcut shortcut) : base(sender)
            => Shortcut = shortcut;
    }
}
