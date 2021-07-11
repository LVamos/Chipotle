using System;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Game.UI
{
    public  struct KeyShortcut
    {
        [Flags]
        public enum Modifiers
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            AltShift = Alt | Shift,
            ControlAlt = Control | Alt,
            ControlShift = Control | Shift,
            ControlAltShift = Control | Alt | Shift
        };



        /// <summary>
        /// Indicates whether control key was pressed.
        /// </summary>
        public readonly bool Control;

        /// <summary>
        /// Indicates whether shift key was pressed.
        /// </summary>
        public readonly bool Shift;

        /// <summary>
        /// Indicates whether alt key was pressed.
        /// </summary>
        public readonly bool Alt;

        /// <summary>
        /// Value of pressed keys.
        /// </summary>
        public readonly Keys Key;

        public KeyShortcut(Modifiers modifiers, Keys key)
        {
            Control = modifiers.HasFlag(Modifiers.Control);
            Alt = modifiers.HasFlag(Modifiers.Alt);
            Shift = modifiers.HasFlag(Modifiers.Shift);
            Key = key;
        }

        /// <summary>
        /// Construktor
        /// </summary>
        /// <param name="control">Was control pressed?</param>
        /// <param name="shift">Was shift pressed?</param>
        /// <param name="alt">Was alt pressed?</param>
        /// <param name="key">Value of pressed keys</param>
        public KeyShortcut(bool control, bool shift, bool alt, Keys key)
        {
            Control = control;
            Shift = shift;
            Alt = alt;
            Key = key;
        }

        /// <summary>
        /// Construktor
        /// </summary>
        /// <param name="key">Value of pressed key</param>
        public KeyShortcut(Keys key):this(false, false, false, key)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Event parameters</param>
        public KeyShortcut(KeyEventParams e):this(e.Control, e.Shift, e.Alt, e.Key)
        {
        }

        public override bool Equals(Object obj)
            => (obj is KeyShortcut && this == (KeyShortcut)obj);

        public override int GetHashCode()
            => Key.GetHashCode();

        public static bool operator ==(KeyShortcut k1, KeyShortcut k2)
            => (k1.Control==k2.Control && k1.Shift==k2.Shift && k1.Alt==k2.Alt && k1.Key==k2.Key);


        public static bool operator !=(KeyShortcut k1, KeyShortcut k2)
            => !(k1 == k2);
    }
}
