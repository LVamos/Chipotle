using Game.Controls.Keyboard;
using Game.UI;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Handles keyboard input using Unity New Input System.
/// Tracks held keys reliably and reports correct modifier states.
/// </summary>
public class KeyboardHandler : MonoBehaviour
{
	private readonly HashSet<Key> heldKeys = new();

	/// <summary>
	/// Indicates whether any non-modifier key is currently held.
	/// </summary>
	public bool AnyKeyPressed => heldKeys.Count > 0;

	private void Update()
	{
		Keyboard keyboard = Keyboard.current;
		if (keyboard == null)
			return;

		HandleKeys(keyboard);
		HandleCharacterInput();
	}

	private void HandleKeys(Keyboard keyboard)
	{
		// Read modifier state directly in this frame
		bool shift = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
		bool ctrl = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
		bool alt = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;

		foreach (KeyControl keyControl in keyboard.allKeys)
		{
			if (keyControl == null)
				continue;

			Key key = keyControl.keyCode;

			if (IsModifier(key))
				continue;

			// Key down
			if (keyControl.wasPressedThisFrame)
			{
				if (heldKeys.Add(key))
				{
					KeyboardInput shortcut = new(ctrl, shift, alt, key);
					WindowHandler.OnKeyDown(shortcut);
				}
			}

			// Key up
			if (keyControl.wasReleasedThisFrame)
			{
				if (heldKeys.Remove(key))
				{
					KeyboardInput shortcut = new(shift, alt, ctrl, key);
					WindowHandler.OnKeyUp(shortcut);
				}
			}
		}
	}

	private void HandleCharacterInput()
	{
		if (string.IsNullOrEmpty(Input.inputString))
			return;

		// Input.inputString may contain multiple characters in one frame
		foreach (char c in Input.inputString)
		{
			if (char.IsLetterOrDigit(c))
				WindowHandler.OnKeyPress(c);
		}
	}

	private bool IsModifier(Key key)
	{
		switch (key)
		{
			case Key.LeftShift:
			case Key.RightShift:
			case Key.LeftCtrl:
			case Key.RightCtrl:
			case Key.LeftAlt:
			case Key.RightAlt:
				return true;
			default:
				return false;
		}
	}
}
