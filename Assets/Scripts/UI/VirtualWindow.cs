using Game.Audio;
using Game.Messaging;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.UI
{

	/// <summary>
	/// Base class for all virtual windows
	/// </summary>
	[Serializable]
	public abstract class VirtualWindow : MessagingObject
	{

		public virtual void Initialize()
		{
		}


		protected virtual void Avake()
			=> SetUpAudio();

		protected void SetUpAudioSource(AudioSource audioSource, string soundName = null, bool loop = false)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.loop = loop;

			if (!string.IsNullOrEmpty(soundName))
				audioSource.clip = Sounds.GetClip(soundName);
		}

		protected virtual void SetUpAudio()
		{ }

		/// <summary>
		/// Reference to parent window
		/// </summary>
		public VirtualWindow ParentWindow { get; set; }

		/// <summary>
		/// Key commands and their handlers
		/// </summary>
		protected Dictionary<KeyShortcut, Action> _shortcuts = new();

		/// <summary>
		/// Indicates if the window is closed
		/// </summary>
		public bool Closed { get; private set; }

		/// <summary>
		/// Closes an opened window.
		/// </summary>
		public virtual void Close()
		{
			Closed = true;
			Destroy(gameObject);

			// Try to switch to parent window.
			if (ParentWindow != null)
				WindowHandler.Switch(ParentWindow);
		}

		/// <summary>
		/// OnActivate event handler
		/// </summary>
		public virtual void OnActivate() => Closed = false;

		/// <summary>
		/// OnClose event handler
		/// </summary>
		public virtual void OnDeactivate() => Closed = true;

		/// <summary>
		/// KeyDown event handler
		/// </summary>
		/// <param name="e">Event parameters</param>
		public virtual void OnKeyDown(KeyShortcut shortcut)
		{
			if (shortcut.Control || shortcut.Key is KeyCode.LeftControl or KeyCode.RightControl)
				Sounds.MuteSpeech();

			Action action = null;
			if (_shortcuts != null && _shortcuts.TryGetValue(shortcut, out action))
				action();
		}

		/// <summary>
		/// Handles the KeyPress message.
		/// </summary>
		/// <param name="letter">The key that was pressed</param>
		public virtual void OnKeyPress(char letter) { }

		/// <summary>
		/// KeyUp event handler
		/// </summary>
		/// <param name="e">Event parameters</param>
		/// <remarks>Must be implemented in descendants.</remarks>
		public virtual void OnKeyUp(KeyShortcut shortcut)
		{ }

		/// <summary>
		/// Registers shotcuts and corresponding actions.
		/// </summary>
		/// <remarks>If a shortcut is already registered it'll be overriden.</remarks>
		/// <param name="shortcuts">Set of shortcuts to be registered</param>
		protected void RegisterShortcuts(params (KeyShortcut shortcut, Action action)[] shortcuts)
		{
			foreach ((KeyShortcut shortcut, Action action) shortcut in shortcuts)
				_shortcuts[shortcut.shortcut] = shortcut.action;
		}

		protected const float _defaultVolume = 1;

		protected void Play(string soundName, float? volume = null)
		{
			if (string.IsNullOrEmpty(soundName))
				return;

			GameObject obj = new();
			AudioSource source = obj.AddComponent<AudioSource>();
			source.clip = Sounds.GetClip(soundName);
			source.spatialBlend = 0;
			source.volume = volume != null ? volume.Value : _defaultVolume;
			source.Play();
			Destroy(obj, Sounds.GetClip(soundName).length); // Destroy the object after the sound is played.
		}

		protected void AdjustVolume(AudioSource sound, float duration, float targetVolume)
		{
			StartCoroutine(Adjust());

			IEnumerator Adjust()
			{
				float startVolume = sound.volume;

				for (float t = 0; t < duration; t += Time.deltaTime)
				{
					sound.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
					yield return null;
				}
				sound.volume = targetVolume;

				if (targetVolume <= 0)
					sound.Stop();
			}
		}
	}
}