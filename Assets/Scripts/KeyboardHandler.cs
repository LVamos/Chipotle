using Game.UI;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// A class that takes care of the detection and processing of keyboard inputs.
/// </summary>
public class KeyboardHandler : MonoBehaviour
{
	private void Update()
	{
		HandleKeyDown();
		HandleKeyUp();
	}

	private void HandleKeyDown()
	{
		IEnumerable<KeyCode> keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();

		bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

		KeyCode key = keyCodes.FirstOrDefault(k => Input.GetKeyDown(k) &&
			k != KeyCode.LeftShift && k != KeyCode.RightShift &&
			k != KeyCode.LeftAlt && k != KeyCode.RightAlt);

		if (key == KeyCode.None)
			return;

		KeyShortcut shortcut = new(shift: shift, alt: alt, control: ctrl, key: key);
		WindowHandler.OnKeyDown(shortcut);
	}

	private void HandleKeyUp()
	{
		IEnumerable<KeyCode> keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();

		IEnumerable<KeyCode> keys = keyCodes.Where(k => Input.GetKeyUp(k));

		bool alt = keys.Contains(KeyCode.LeftAlt) || keys.Contains(KeyCode.RightAlt);
		bool shift = keys.Contains(KeyCode.LeftShift) || keys.Contains(KeyCode.RightShift);
		bool ctrl = keys.Contains(KeyCode.LeftControl) || keys.Contains(KeyCode.RightControl);

		KeyCode key = keys.FirstOrDefault(k =>
			k != KeyCode.LeftShift && k != KeyCode.RightShift &&
			k != KeyCode.LeftControl && k != KeyCode.RightControl &&
			k != KeyCode.LeftAlt && k != KeyCode.RightAlt);

		if (key == KeyCode.None)
			return;

		KeyShortcut shortcut = new(shift: shift, alt: alt, control: ctrl, key: key);
		WindowHandler.OnKeyUp(shortcut);
	}
}
