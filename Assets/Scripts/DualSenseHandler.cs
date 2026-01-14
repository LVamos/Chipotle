using Game.Controls.DualSense;
using Game.UI;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Handles input from a DualSense controller using the New Input System.
/// </summary>
public class DualSenseHandler : MonoBehaviour
{
	private static void CheckDPad(DualSenseDPad dpad, ButtonControl control)
	{
		if (control.wasPressedThisFrame)
			WindowHandler.OnKeyDown(new DualSenseInput(dpad.ToString()));

		if (control.wasReleasedThisFrame)
			WindowHandler.OnKeyUp(new DualSenseInput(dpad.ToString()));
	}

	private readonly Dictionary<DualSenseStick, DualSenseStickDirection?> stickState = new();
	private readonly Dictionary<DualSenseTrigger, bool> triggerState = new();

	private Gamepad gamepad;

	private void Update()
	{
		gamepad = Gamepad.current;
		if (gamepad == null)
			return;

		HandleButtons();
		HandleTriggers();
		HandleSticks();
	}

	private void HandleButtons()
	{
		ButtonControl touchpad = null;

		foreach (InputControl control in gamepad.allControls)
		{
			if (control.name.Equals("touchpadButton", StringComparison.OrdinalIgnoreCase) && control is ButtonControl btn)
			{
				touchpad = "Button:/DualSenseGamepadHID/touchpadButton"Cbtn;
				break;
			}
		}

		if (touchpad != null)
		{
			if (touchpad.wasPressedThisFrame)
				WindowHandler.OnKeyDown(new DualSenseInput("TouchpadButton"));

			if (touchpad.wasReleasedThisFrame)
				WindowHandler.OnKeyUp(new DualSenseInput("TouchpadButton"));
		}

		// D-pad
		CheckDPad(DualSenseDPad.Up, gamepad.dpad.up);
		CheckDPad(DualSenseDPad.Down, gamepad.dpad.down);
		CheckDPad(DualSenseDPad.Left, gamepad.dpad.left);
		CheckDPad(DualSenseDPad.Right, gamepad.dpad.right);

		CheckButton(DualSenseButton.Cross, gamepad.buttonSouth);
		CheckButton(DualSenseButton.Circle, gamepad.buttonEast);
		CheckButton(DualSenseButton.Square, gamepad.buttonWest);
		CheckButton(DualSenseButton.Triangle, gamepad.buttonNorth);

		CheckButton(DualSenseButton.L1, gamepad.leftShoulder);
		CheckButton(DualSenseButton.R1, gamepad.rightShoulder);

		CheckButton(DualSenseButton.Options, gamepad.startButton);
		CheckButton(DualSenseButton.Create, gamepad.selectButton);

		CheckButton(DualSenseButton.L3, gamepad.leftStickButton);
		CheckButton(DualSenseButton.R3, gamepad.rightStickButton);
	}

	private static void CheckButton(DualSenseButton button, ButtonControl control)
	{
		if (control.wasPressedThisFrame)
			WindowHandler.OnKeyDown(new DualSenseInput(button.ToString()));

		if (control.wasReleasedThisFrame)
			WindowHandler.OnKeyUp(new DualSenseInput(button.ToString()));
	}

	private void HandleTriggers()
	{
		CheckTrigger(DualSenseTrigger.L2, gamepad.leftTrigger.ReadValue());
		CheckTrigger(DualSenseTrigger.R2, gamepad.rightTrigger.ReadValue());
	}

	private void CheckTrigger(DualSenseTrigger trigger, float value)
	{
		bool pressed = value > 0.1f;

		if (!triggerState.TryGetValue(trigger, out bool wasPressed))
			wasPressed = false;

		if (pressed && !wasPressed)
			WindowHandler.OnKeyDown(new DualSenseInput(trigger.ToString()));
		else if (!pressed && wasPressed)
			WindowHandler.OnKeyUp(new DualSenseInput(trigger.ToString()));

		triggerState[trigger] = pressed;
	}

	private void HandleSticks()
	{
		CheckStick(DualSenseStick.Left, gamepad.leftStick.ReadValue());
		CheckStick(DualSenseStick.Right, gamepad.rightStick.ReadValue());
	}

	private void CheckStick(DualSenseStick stick, Vector2 input)
	{
		stickState.TryGetValue(stick, out DualSenseStickDirection? prevDirection);

		DualSenseStickDirection? newDirection = null;

		if (input.magnitude >= 0.1f)
		{
			if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
				newDirection = input.x > 0 ? DualSenseStickDirection.Right : DualSenseStickDirection.Left;
			else
				newDirection = input.y > 0 ? DualSenseStickDirection.Up : DualSenseStickDirection.Down;
		}

		if (prevDirection != null && prevDirection != newDirection)
			WindowHandler.OnKeyUp(new DualSenseInput($"{stick}Stick{prevDirection}"));

		if (newDirection != null && prevDirection != newDirection)
			WindowHandler.OnKeyDown(new DualSenseInput($"{stick}Stick{newDirection}"));

		stickState[stick] = newDirection;
	}
}
