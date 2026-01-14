using Game.Audio;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Messaging;

using System;
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
		protected Dictionary<KeyboardInput, Action> _keyboardShortcuts = new();

		protected Dictionary<DualSenseInput, Action> _dualsenseShortcuts = new();

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
		public virtual void OnKeyDown(KeyboardInput shortcut)
		{
			if (shortcut.Control || shortcut.Key is KeyCode.LeftControl or KeyCode.RightControl)
				Sounds.MuteSpeech();

			Action action = null;
			if (_keyboardShortcuts != null && _keyboardShortcuts.TryGetValue(shortcut, out action))
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
		public virtual void OnKeyUp(KeyboardInput shortcut)
		{ }

		public virtual void OnKeyUp(DualSenseInput input)
		{ }

		/// <summary>
		/// Registers Dual Sense shortcuts and corresponding actions.
		/// </summary>
		/// <remarks>If a shortcut is already registered it'll be overriden.</remarks>
		/// <param name="shortcuts">Set of shortcuts to be registered</param>
		protected void RegisterDualSenseShortcuts(params (DualSenseInput shortcut, Action action)[] shortcuts)
		{
			foreach ((DualSenseInput shortcut, Action action) shortcut in shortcuts)
				_dualsenseShortcuts[shortcut.shortcut] = shortcut.action;
		}

		/// <summary>
		/// Registers shotcuts and corresponding actions.
		/// </summary>
		/// <remarks>If a shortcut is already registered it'll be overriden.</remarks>
		/// <param name="shortcuts">Set of shortcuts to be registered</param>
		protected void RegisterKeyboardShortcuts(params (KeyboardInput shortcut, Action action)[] shortcuts)
		{
			foreach ((KeyboardInput shortcut, Action action) shortcut in shortcuts)
				_keyboardShortcuts[shortcut.shortcut] = shortcut.action;
		}

		protected const float _defaultVolume = 1;

		protected AudioSource Play(string soundName, float? volume = null)
		{
			if (string.IsNullOrEmpty(soundName))
				return null;

			float finalVolume = volume != null ? volume.Value : _defaultVolume;
			return Sounds.Play2d(soundName, finalVolume);
		}

		public virtual void OnKeyDown(DualSenseInput input)
		{
			Sounds.MuteSpeech();

			Action action = null;
			if (_dualsenseShortcuts!= null && _dualsenseShortcuts.TryGetValue(input, out action))
				action();
		}
	}
}