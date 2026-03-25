using System;

using UnityEngine;

namespace Game.Audio
{
	public static class Cutscene
	{

		public static void Init()
		{
			if (_player == null)
				CreateCutsceneplayer();
		}

		private static void CreateCutsceneplayer()
		{
			GameObject obj = new("CutScene player");
			_player = obj.AddComponent<CutscenePlayer>();
		}

		/// <summary>
		/// Resumes a paused cutscene.
		/// </summary>
		public static void Resume()
		{
			_player?.Resume();
		}

		/// <summary>
		/// Pauses an ongoing cutscene.
		/// </summary>
		public static void Pause()
		{
			_player?.Pause();
		}


		private static CutscenePlayer _player;

		/// <summary>
		/// Plays the specified audio cutscene.
		/// </summary>
		/// <param name="sender">An object or NPC which wants to play the cutscene</param>
		/// <param name="cutsceneName">Name of the soudn file to be played</param>
		public static void Play(object sender, string cutsceneName)
		{
			if (string.IsNullOrEmpty(cutsceneName))
				throw new ArgumentNullException(nameof(cutsceneName));

			_player.Play(cutsceneName, sender);
		}

		/// <summary>
		/// Stops an ongoing audio cutscene.
		/// </summary>
		/// <param name="sender">The object or NPC which wants to stop the cutscene</param>
		public static void Stop(object sender)
		{
			_player.Stop();
		}
	}
}
