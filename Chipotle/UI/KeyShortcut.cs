using System;
using System.Windows.Forms;

namespace Game.UI
{
    /// <summary>
    /// Represents a keyboard shortcut.
    /// </summary>
    [Serializable]
    public struct KeyShortcut
    {
        /// <summary>
        /// Indicates whether alt key was pressed.
        /// </summary>
        public readonly bool Alt;

        /// <summary>
        /// Indicates whether control key was pressed.
        /// </summary>
        public readonly bool Control;

        /// <summary>
        /// Value of pressed keys.
        /// </summary>
        public readonly Keys Key;

        /// <summary>
        /// Indicates whether shift key was pressed.
        /// </summary>
        public readonly bool Shift;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modifiers">Key modifiers</param>
        /// <param name="key">The pressed key</param>
        public KeyShortcut(Modifiers modifiers, Keys key)
        {
            Control = modifiers.HasFlag(Modifiers.Control);
            Alt = modifiers.HasFlag(Modifiers.Alt);
            Shift = modifiers.HasFlag(Modifiers.Shift);
            Key = key;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="control">Specifies if the control key was pressed</param>
        /// <param name="shift">Specifies if the shift key was pressed</param>
        /// <param name="alt">Specifies if the alt key was pressed</param>
        /// <param name="key">Value of pressed keys</param>
        public KeyShortcut(bool control, bool shift, bool alt, Keys key)
        {
            Control = control;
            Shift = shift;
            Alt = alt;
            Key = key;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Value of pressed key</param>
        public KeyShortcut(Keys key) : this(false, false, false, key)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Event parameters</param>
        public KeyShortcut(KeyEventParams e) : this(e.Control, e.Shift, e.Alt, e.Key)
        {
        }

        /// <summary>
        /// set of key modifiers
        /// </summary>
        [Flags]
        public enum Modifiers
        {
            /// <summary>
            /// Alt key
            /// </summary>
            Alt = 1,

            /// <summary>
            /// Control key
            /// </summary>
            Control = 2,

            /// <summary>
            /// Shift key
            /// </summary>
            Shift = 4,

            /// <summary>
            /// Alt and shift keys
            /// </summary>
            AltShift = Alt | Shift,

            /// <summary>
            /// Control and alt keys
            /// </summary>
            ControlAlt = Control | Alt,

            /// <summary>
            /// Control and shift keys
            /// </summary>
            ControlShift = Control | Shift,

            /// <summary>
            /// Control, alt and shift keys
            /// </summary>
            ControlAltShift = Control | Alt | Shift
        };

        /// <summary>
        /// Overloads the != operator.
        /// </summary>
        /// <param name="k1">First operand</param>
        /// <param name="k2">second operand</param>
        /// <returns>True if the operands are inequal</returns>
        public static bool operator !=(KeyShortcut k1, KeyShortcut k2)
            => !(k1 == k2);

        /// <summary>
        /// Overloads the == operator.
        /// </summary>
        /// <param name="k1">First operand</param>
        /// <param name="k2">second operand</param>
        /// <returns>True if the operands are equal</returns>
        public static bool operator ==(KeyShortcut k1, KeyShortcut k2)
            => (k1.Control == k2.Control && k1.Shift == k2.Shift && k1.Alt == k2.Alt && k1.Key == k2.Key);

        /// <summary>
        /// Checks if two instances are equal.
        /// </summary>
        /// <param name="obj">Another object to be checked</param>
        /// <returns>True if both instances are equal</returns>
        public override bool Equals(Object obj)
            => (obj is KeyShortcut shortcut && this == shortcut);

        /// <summary>
        /// Returns a hash code for the instance.
        /// </summary>
        /// <returns>a hash code for the instance</returns>
        public override int GetHashCode()
            => Key.GetHashCode();
    }
}