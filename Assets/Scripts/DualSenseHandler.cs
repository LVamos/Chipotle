using Game.Controls.DualSense;
using Game.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Handles input from a DualSense controller, including buttons, sticks, and triggers.
/// </summary>
public class DualSenseHandler : MonoBehaviour
{
	private readonly Dictionary<DualSenseStick, DualSenseStickDirection?> stickState = new();
	private readonly Dictionary<DualSenseTrigger, bool> triggerState = new();

	private void Update()
	{
		HandleButtons();
		HandleTriggers();
		HandleSticks();
	}

	private void HandleButtons()
	{
		foreach (DualSenseButton button in Enum.GetValues(typeof(DualSenseButton)))
		{
			string unityName = button.ToString();
			if (Input.GetButtonDown(unityName))
				WindowHandler.OnKeyDown(new DualSenseInput(unityName));
			if (Input.GetButtonUp(unityName))
				WindowHandler.OnKeyUp(new DualSenseInput(unityName));
		}
	}

	private void HandleTriggers()
	{
		foreach (DualSenseTrigger trigger in Enum.GetValues(typeof(DualSenseTrigger)))
		{
			float value = Input.GetAxis(trigger.ToString());
			bool pressed = value > 0.1f;

			if (!triggerState.TryGetValue(trigger, out bool wasPressed))
				wasPressed = false;

			if (pressed && !wasPressed)
				WindowHandler.OnKeyDown(new DualSenseInput(trigger.ToString()));
			else if (!pressed && wasPressed)
				WindowHandler.OnKeyUp(new DualSenseInput(trigger.ToString()));

			triggerState[trigger] = pressed;
		}
	}

	private void HandleSticks()
	{
		CheckStick(DualSenseStick.Left, new Vector2(Input.GetAxis("LeftStickHorizontal"), Input.GetAxis("LeftStickVertical")));
		CheckStick(DualSenseStick.Right, new Vector2(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical")));
	}

	private void CheckStick(DualSenseStick stick, Vector2 input)
	{
		DualSenseStickDirection? prevDirection = stickState.ContainsKey(stick) ? stickState[stick] : null;

		DualSenseStickDirection? newDirection = null;
		if (input.magnitude >= 0.1f)
		{
			if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
				newDirection = input.x > 0 ? DualSenseStickDirection.Right : DualSenseStickDirection.Left;
			else
				newDirection = input.y > 0 ? DualSenseStickDirection.Up : DualSenseStickDirection.Down;
		}

		if (prevDirection != null && prevDirection != newDirection)
			WindowHandler.OnKeyUp(new DualSenseInput($"{stick}{prevDirection}"));

		if (newDirection != null && prevDirection != newDirection)
			WindowHandler.OnKeyDown(new DualSenseInput($"{stick}{newDirection}"));

		stickState[stick] = newDirection;
	}
}
