using Game.Messaging.Events.Sound;

using System;

using UnityEngine;

namespace Game.Audio
{
	public static class CutScene
	{

		public static void Init()
		{
			_cutSceneMessage = null;
			if (_cutScenePlayer == null)
				CreateCutsceneplayer();
		}

		private static void CreateCutsceneplayer()
		{
			GameObject obj = new("CutScene player");
			_cutScenePlayer = obj.AddComponent<CutScenePlayer>();
		}

		/// <summary>
		/// Resumes a paused cutscene.
		/// </summary>
		public static void Resume()
		{
			if (_cutScenePlayer == null || _cutSceneMessage == null)
				return;

			_cutScenePlayer.Resume();
		}


		/// <summary>
		/// Pauses an ongoing cutscene.
		/// </summary>
		public static void Pause()
		{
			_cutScenePlayer?.Pause();
		}


		private static CutsceneBegan _cutSceneMessage;

		private static CutScenePlayer _cutScenePlayer;

		/// <summary>
		/// Plays the specified audio cutscene.
		/// </summary>
		/// <param name="sender">An object or NPC which wants to play the cutscene</param>
		/// <param name="cutsceneName">Name of the soudn file to be played</param>
		public static void Play(object sender, string cutsceneName)
		{
			if (string.IsNullOrEmpty(cutsceneName))
				throw new ArgumentNullException(nameof(cutsceneName));

			_cutScenePlayer.Play(cutsceneName, sender);
			_cutSceneMessage = new(null, cutsceneName);
			World.TakeMessage(_cutSceneMessage);

			// Stop it if cutscenes are forbidden for debugging purposes.
			if (!Settings.PlayCutscenes)
				Stop(null);
		}

		/// <summary>
		/// Stops an ongoing audio cutscene.
		/// </summary>
		/// <param name="sender">The object or NPC which wants to stop the cutscene</param>
		public static void Stop(object sender)
		{
			if (_cutSceneMessage == null)
				return;

			World.TakeMessage(new CutsceneEnded(_cutSceneMessage.Sender, _cutSceneMessage.CutsceneName));
			_cutScenePlayer.Stop();
			_cutSceneMessage = null;
		}
	}
}
