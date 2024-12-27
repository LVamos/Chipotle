using System.Collections.Generic;
using System.Windows.Forms;

namespace Game.UI
{
    /// <summary>
    /// Parameters for the KeyDown event
    /// </summary>
    public struct KeyEventParams
    {
        /// <summary>
        /// Value of pressed key
        /// </summary>
        public readonly Keys Key;

        /// <summary>
        /// Indicates whether alt key was pressed.
        /// </summary>
        public readonly bool Alt;

        /// <summary>
        /// Indicates whether shift key was pressed.
        /// </summary>
        public readonly bool Shift;

        /// <summary>
        /// Indicates whether control key was pressed.
        /// </summary>
        public readonly bool Control;

        /// <summary>
        /// Indicates whether a digit key was pressed.
        /// </summary>
        public readonly bool Digit;

        /// <summary>
        /// Indicates whether a letter key was pressed.
        /// </summary>
        public readonly bool Letter;

        /// <summary>
        /// Indicates whether a function key was pressed.
        /// </summary>
        public readonly bool FunctionKey;

        /// <summary>
        /// Indicates whether an arrow key was pressed.
        /// </summary>
        public readonly bool ArrowKey;



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Event parameters</param>
        public KeyEventParams(KeyEventArgs e)
        {
            Key = e.KeyCode;
            Control = e.Control;
            Alt = e.Alt;
            Shift = e.Shift;

            Digit = (Key >= Keys.D0 && Key <= Keys.D9);
            Letter = (Key >= Keys.A && Key <= Keys.Z);
            FunctionKey = (Key >= Keys.F1 && Key <= Keys.F12);
            ArrowKey = new HashSet<Keys>() { Keys.Left, Keys.Right, Keys.Up, Keys.Down }.Contains(Key);
        }

    }
}
