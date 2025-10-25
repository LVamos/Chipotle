using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Audio
{
	/// <summary>
	/// Defines playback modes for zone background sound based on player's proximity.
	/// </summary>
	public enum ZoneSoundMode
	{
		/// <summary>
		/// If the player is in the zone, the sound plays in full stereo.
		/// </summary>
		InZone,

		/// <summary>
		/// If the player is in an accessible zone, the sound plays softly from every passage leading to the player's current zone.
		/// </summary>
		InAccessibleZone,

		/// <summary>
		/// If the player is far, the sound plays softly from the center of the zone, provided the player is within the sound's radius defined by _soundRadius.
		/// </summary>
		InInaccessibleZone
	}
}
