using Game.Audio;
using Game.Controls;
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
		protected virtual void AddShortcuts() { }

		protected virtual void AddShortcut(Command command1, Action command)
		{
			CommandBindings shortcut = InputConfig.GetBindings(command1);
			if (shortcut == null)
				return;

			if (shortcut.Keyboard != null)
				_keyboardShortcuts[shortcut.Keyboard.Value] = command;
			if (shortcut.DualSense != null)
				_dualsenseShortcuts[shortcut.DualSense.Value] = command;
		}

		public virtual void Initialize()
		{
			AddShortcuts();
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
			Action action = null;
			if (_dualsenseShortcuts != null && _dualsenseShortcuts.TryGetValue(input, out action))
				action();
		}
	}
}