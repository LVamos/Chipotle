using Game;
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

	private bool _controlHeld;
	private bool _shiftHeld;
	private bool _ctrlHeld;
	private bool _altHeld;

	/// <summary>
	/// Indicates whether any non-modifier key is currently held.
	/// </summary>
	public bool GetAnyKeyPressed()
	{
		return heldKeys.Count > 0;
	}

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
		bool shift = IsModifierDown(keyboard.leftShiftKey) || IsModifierDown(keyboard.rightShiftKey);
		bool ctrl = IsModifierDown(keyboard.leftCtrlKey) || IsModifierDown(keyboard.rightCtrlKey);
		bool alt = IsModifierDown(keyboard.leftAltKey) || IsModifierDown(keyboard.rightAltKey);

		// Modifiers held in the previous frame, needed for a whole combination tapped between two frames
		bool shiftInFrame = shift || _shiftHeld;
		bool ctrlInFrame = ctrl || _ctrlHeld;
		bool altInFrame = alt || _altHeld;
		_shiftHeld = IsHeld(keyboard.leftShiftKey) || IsHeld(keyboard.rightShiftKey);
		_ctrlHeld = IsHeld(keyboard.leftCtrlKey) || IsHeld(keyboard.rightCtrlKey);
		_altHeld = IsHeld(keyboard.leftAltKey) || IsHeld(keyboard.rightAltKey);

		if (ctrl && !_controlHeld)
			ScreenReader.Silence();
		_controlHeld = ctrl;

		foreach (KeyControl keyControl in keyboard.allKeys)
		{
			if (keyControl == null)
				continue;

			Key key = keyControl.keyCode;

			if (IsModifier(key))
				continue;

			bool isPressed = keyControl.isPressed;

			if (IsDown(keyControl) && !heldKeys.Contains(key))
			{
				heldKeys.Add(key);
				bool tappedInFrame = !isPressed;
				KeyboardInput shortcut = tappedInFrame
					? new(ctrlInFrame, shiftInFrame, altInFrame, key)
					: new(ctrl, shift, alt, key);
				WindowHandler.OnKeyDown(shortcut);
			}

			if (!isPressed && heldKeys.Contains(key))
			{
				heldKeys.Remove(key);
				KeyboardInput shortcut = new(ctrl, shift, alt, key);
				WindowHandler.OnKeyUp(shortcut);
			}
		}
	}

	/// <summary>
	/// A key pressed and released between two frames isn't pressed anymore, but it still has to count as a press.
	/// </summary>
	private static bool IsDown(KeyControl key) => key.isPressed || key.wasPressedThisFrame;

	/// <summary>
	/// A modifier counts as down when it's held or when it was tapped between two frames.
	/// </summary>
	private static bool IsModifierDown(KeyControl key)
		=> IsHeld(key) || key.wasPressedThisFrame && key.wasReleasedThisFrame;

	/// <summary>
	/// Reads the real key value. The cached isPressed stays set for a modifier held while the game lost focus.
	/// </summary>
	private static bool IsHeld(KeyControl key) => key.IsValueConsideredPressed(key.ReadValue());

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
