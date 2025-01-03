﻿using Game.Entities.Characters.Chipotle;
using Game.Entities.Characters.Tuttle;

using ProtoBuf;

using System.Collections;

using UnityEngine;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Controls the sound output of an NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(ChipotleSound))]
	[ProtoInclude(101, typeof(TuttleSound))]
	public class Sound : CharacterComponent
	{
		/// <summary>
		/// Default voluem of sound output.
		/// </summary>
		protected const float _defaultVolume = 1;

		/// <summary>
		/// Adjusts the volume of an audio source over a specified duration to a target volume.
		/// </summary>
		/// <param name="source">An audio source to be adjusted</param>
		/// <param name="duration">Duration of the adjustment</param>
		/// <param name="targetVolume">Target volume</param>
		protected void AdjustVolume(AudioSource source, float duration, float targetVolume)
		{
			if (targetVolume == source.volume)
				return;

			StartCoroutine(Ajust());

			IEnumerator Ajust()
			{
				float startVolume = source.volume;

				for (float t = 0; t < duration; t += Time.deltaTime)
				{
					source.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
					yield return null;
				}
				source.volume = targetVolume;

				if (targetVolume <= 0)
					source.Stop();
			}
		}


		/// <summary>
		/// Volume for foot steps
		/// </summary>
		protected float _walkVolume = 1;

		/// <summary>
		/// Converts a string list to a CSV string.
		/// </summary>
		/// <param name="stringList">The string list to be formatted</param>
		/// <param name="addAnd">Specifies if last two items should by separated with " a " conjunction.</param>
		/// <returns>CSV string</returns>
		protected string FormatStringList(string[] stringList, bool addAnd = false)
		{
			int count = stringList.Length;
			if (count == 1)
				return stringList[0];

			if (count > 2)
			{
				for (int i = 0; i < count - 2; i++)
					stringList[i] += ", ";
			}

			stringList[count - 2] += addAnd ? " a " : ", ";
			return string.Join(string.Empty, stringList);
		}
	}
}