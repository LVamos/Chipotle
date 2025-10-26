using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Terrain
{
	/// <summary>
	/// Indicates if a zone is inside a building or outside.
	/// </summary>
	public enum ZoneType
	{
		/// <summary>
		/// A room or corridor in a building
		/// </summary>
		Indoor,

		/// <summary>
		/// An openair place like yard or meadow
		/// </summary>
		Outdoor
	}
}
