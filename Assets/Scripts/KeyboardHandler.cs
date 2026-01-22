using Game.Controls.Keyboard;
using Game.UI;

using System;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// A class that takes care of the detection and processing of keyboard inputs.
/// </summary>
public class KeyboardHandler : MonoBehaviour
{
	private void Start()
	{
		_keyCodes = Enum.GetValues(typeof(KeyCode))
				.Cast<KeyCode>()
				.ToArray();
	}

	private void HandleCharacterInput()
	{
		if (string.IsNullOrEmpty(Input.inputString))
			return;

		// Input.inputString can contain multiple characters in a single frame.
		foreach (char c in Input.inputString)
		{
			if (char.IsLetterOrDigit(c))
				WindowHandler.OnKeyPress(c);
		}
	}

	private void Update()
	{
		HandleKeyDown();
		HandleKeyUp();
		HandleCharacterInput();
	}

	private KeyCode[] _keyCodes;

	private bool IsModifier(KeyControl key, Keyboard keyboard)
	{
		return
			key == keyboard.leftCtrlKey
			|| key == keyboard.rightCtrlKey
			|| key == keyboard.leftAltKey
			|| key == keyboard.rightAltKey
			|| key == keyboard.leftShiftKey
			|| key == keyboard.rightShiftKey;
	}

	private void HandleKeyDown()
	{
		Keyboard keyboard = Keyboard.current;
		if (keyboard == null)
			return;

		if (!keyboard.anyKey.wasPressedThisFrame)
			return;

		bool ctrl =
			keyboard.leftCtrlKey.isPressed ||
			keyboard.rightCtrlKey.isPressed;

		bool alt =
			keyboard.leftAltKey.isPressed ||
			keyboard.rightAltKey.isPressed;

		bool shift =
			keyboard.leftShiftKey.isPressed ||
			keyboard.rightShiftKey.isPressed;

		KeyControl pressedKey = keyboard.allKeys.FirstOrDefault((KeyControl key) =>
			key != null && key.wasPressedThisFrame &&
			!IsModifier(key, keyboard)
		);

		if (pressedKey == null)
			return;

		KeyboardInput shortcut = new(
			shift: shift,
			alt: alt,
			control: ctrl,
			key: pressedKey.keyCode
		);

		WindowHandler.OnKeyDown(shortcut);
	}

	private void HandleKeyUp()
	{
		Keyboard keyboard = Keyboard.current;
		if (keyboard == null)
			return;

		if (!keyboard.anyKey.wasReleasedThisFrame)
			return;

		KeyControl[] releasedKeys = keyboard.allKeys
			.Where((KeyControl key) => key != null && key.wasReleasedThisFrame)
			.ToArray();

		if (releasedKeys.Length == 0)
			return;

		bool ctrl =
			keyboard.leftCtrlKey.isPressed ||
			keyboard.rightCtrlKey.isPressed;
		bool alt =
			keyboard.leftAltKey.isPressed ||
			keyboard.rightAltKey.isPressed;
		bool shift =
			keyboard.leftShiftKey.isPressed ||
			keyboard.rightShiftKey.isPressed;

		KeyControl releasedKey = releasedKeys.FirstOrDefault((KeyControl key) =>
			!IsModifier(key, keyboard)
		);

		if (releasedKey == null)
			return;

		KeyboardInput shortcut = new(
			shift: shift,
			alt: alt,
			control: ctrl,
			key: releasedKey.keyCode
		);

		WindowHandler.OnKeyUp(shortcut);
	}

}
