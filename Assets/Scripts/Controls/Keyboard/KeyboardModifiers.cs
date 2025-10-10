using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Controls
{
	/// <summary>
	/// set of key modifiers
	/// </summary>
	[Flags]
	public enum KeyboardModifiers
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
}
