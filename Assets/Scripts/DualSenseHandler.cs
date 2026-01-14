using Game.Controls.DualSense;
using Game.UI;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Handles input from a DualSense controller using the New Input System, including DPad, sticks, triggers, and touchpad.
/// </summary>
public class DualSenseHandler : MonoBehaviour
{
	private readonly Dictionary<DualSenseStick, DualSenseStickDirection?> stickState = new();
	private readonly Dictionary<DualSenseTrigger, bool> triggerState = new();

	private Gamepad gamepad;

	// Touchpad swipe tracking
	private Vector2? prevTouchPos = null;
	private readonly float swipeThreshold = 0.3f;
	private DualSenseStickDirection? currentSwipe = null;

	private void Update()
	{
		gamepad = Gamepad.current;
		if (gamepad == null)
			return;

		HandleButtons();
		HandleTriggers();
		HandleSticks();
		HandleTouchpad();
		HandleDPad();
	}

	// ===== DPad =====
	private void HandleDPad()
	{
		CheckDPad(DualSenseDPad.Up, gamepad.dpad.up);
		CheckDPad(DualSenseDPad.Down, gamepad.dpad.down);
		CheckDPad(DualSenseDPad.Left, gamepad.dpad.left);
		CheckDPad(DualSenseDPad.Right, gamepad.dpad.right);
	}

	private static void CheckDPad(DualSenseDPad dpad, ButtonControl control)
	{
		if (control.wasPressedThisFrame)
			WindowHandler.OnKeyDown(new DualSenseInput(dpad.ToString()));

		if (control.wasReleasedThisFrame)
			WindowHandler.OnKeyUp(new DualSenseInput(dpad.ToString()));
	}

	// ===== Buttons =====
	private void HandleButtons()
	{
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

		// Touchpad press
		var touchButton = gamepad.allControls.FirstOrDefault(c => c.name.Equals("touchpadButton", StringComparison.OrdinalIgnoreCase)) as ButtonControl;
		if (touchButton != null)
		{
			CheckButton(DualSenseButton.TouchpadButton, touchButton);
		}
	}

	private static void CheckButton(DualSenseButton button, ButtonControl control)
	{
		if (control.wasPressedThisFrame)
			WindowHandler.OnKeyDown(new DualSenseInput(button.ToString()));

		if (control.wasReleasedThisFrame)
			WindowHandler.OnKeyUp(new DualSenseInput(button.ToString()));
	}

	// ===== Triggers =====
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

	// ===== Sticks =====
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

	// ===== Touchpad swipe =====
	private void HandleTouchpad()
	{
		var touchpadStick = gamepad.allControls.FirstOrDefault(c => c.name.Equals("touchpad", StringComparison.OrdinalIgnoreCase)) as StickControl;
		var touchpadButton = gamepad.allControls.FirstOrDefault(c => c.name.Equals("touchpadButton", StringComparison.OrdinalIgnoreCase)) as ButtonControl;

		if (touchpadStick == null || touchpadButton == null)
			return;

		CheckButton(DualSenseButton.TouchpadButton, touchpadButton);

		if (!touchpadButton.isPressed)
		{
			if (currentSwipe != null)
				WindowHandler.OnKeyUp(new DualSenseInput($"Touchpad{currentSwipe}"));

			currentSwipe = null;
			prevTouchPos = null;
			return;
		}

		// swipe
		Vector2 pos = touchpadStick.ReadValue();
		if (prevTouchPos.HasValue)
		{
			Vector2 delta = pos - prevTouchPos.Value;
			if (delta.magnitude >= swipeThreshold)
			{
				DualSenseStickDirection dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
					? (delta.x > 0 ? DualSenseStickDirection.Right : DualSenseStickDirection.Left)
					: (delta.y > 0 ? DualSenseStickDirection.Up : DualSenseStickDirection.Down);

				if (currentSwipe != dir)
				{
					if (currentSwipe != null)
						WindowHandler.OnKeyUp(new DualSenseInput($"Touchpad{currentSwipe}"));

					currentSwipe = dir;
					WindowHandler.OnKeyDown(new DualSenseInput($"Touchpad{currentSwipe}"));
				}
			}
		}

		prevTouchPos = pos;
	}
}
