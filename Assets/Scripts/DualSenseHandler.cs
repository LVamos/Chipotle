using Game;
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
	private readonly HashSet<string> _activeKeys = new(StringComparer.OrdinalIgnoreCase);
	private readonly List<string> _pressedOrder = new();
	private Gamepad _gamepad;
	private bool _previousLeftTriggerPressed;
	private bool _previousRightTriggerPressed;
	private string _previousLeftStickDirection;
	private string _previousRightStickDirection;

	// Touchpad swipe tracking
	private Vector2? _previousTouchPosition = null;
	private readonly float _swipeThreshold = 0.3f;
	private DualSenseStickDirection? _currentSwipe = null;
	private readonly float _triggerPressThreshold = 0.1f;

	private void Update()
	{
		_gamepad = Gamepad.current;
		if (_gamepad == null)
			return;

		ProcessInputs();
	}

	private void ProcessInputs()
	{
		UpdateButton(DualSenseButton.Cross, _gamepad.buttonSouth);
		UpdateButton(DualSenseButton.Circle, _gamepad.buttonEast);
		UpdateButton(DualSenseButton.Square, _gamepad.buttonWest);
		UpdateButton(DualSenseButton.Triangle, _gamepad.buttonNorth);
		UpdateButton(DualSenseButton.L1, _gamepad.leftShoulder);
		UpdateButton(DualSenseButton.R1, _gamepad.rightShoulder);
		UpdateButton(DualSenseButton.Options, _gamepad.startButton);
		UpdateButton(DualSenseButton.Create, _gamepad.selectButton);
		UpdateButton(DualSenseButton.L3, _gamepad.leftStickButton);
		UpdateButton(DualSenseButton.R3, _gamepad.rightStickButton);

		ButtonControl dpadUp = _gamepad.dpad.up;
		ButtonControl dpadDown = _gamepad.dpad.down;
		ButtonControl dpadLeft = _gamepad.dpad.left;
		ButtonControl dpadRight = _gamepad.dpad.right;
		UpdateDPad(DualSenseDPad.DPadUp, dpadUp);
		UpdateDPad(DualSenseDPad.DPadDown, dpadDown);
		UpdateDPad(DualSenseDPad.DPadLeft, dpadLeft);
		UpdateDPad(DualSenseDPad.DPadRight, dpadRight);

		StickControl touchpadStick = _gamepad.allControls.FirstOrDefault(c => c.name.Equals("touchpad", StringComparison.OrdinalIgnoreCase)) as StickControl;
		ButtonControl touchpadButton = _gamepad.allControls.FirstOrDefault(c => c.name.Equals("touchpadButton", StringComparison.OrdinalIgnoreCase)) as ButtonControl;
		UpdateButton(DualSenseButton.TouchpadButton, touchpadButton);

		UpdateTrigger(DualSenseTrigger.L2, _gamepad.leftTrigger.ReadValue(), ref _previousLeftTriggerPressed);
		UpdateTrigger(DualSenseTrigger.R2, _gamepad.rightTrigger.ReadValue(), ref _previousRightTriggerPressed);

		UpdateStick(DualSenseStick.Left, _gamepad.leftStick.ReadValue(), ref _previousLeftStickDirection);
		UpdateStick(DualSenseStick.Right, _gamepad.rightStick.ReadValue(), ref _previousRightStickDirection);

		UpdateTouchpadSwipe(touchpadStick, touchpadButton);
	}

	public bool AnyKeyPressed()
	{
		Gamepad gamepad = Gamepad.current;
		if (gamepad == null)
			return false;

		return _activeKeys.Count > 0;
	}

	private void UpdateButton(DualSenseButton button, ButtonControl control)
	{
		if (control == null)
			return;

		string identifier = button.ToString();
		if (control.wasPressedThisFrame)
			AddActive(identifier);

		if (control.wasReleasedThisFrame)
			RemoveActive(identifier);
	}

	private void UpdateDPad(DualSenseDPad dpad, ButtonControl control)
	{
		if (control == null)
			return;

		string identifier = dpad.ToString();
		if (control.wasPressedThisFrame)
			AddActive(identifier);

		if (control.wasReleasedThisFrame)
			RemoveActive(identifier);
	}

	private void UpdateTrigger(DualSenseTrigger trigger, float value, ref bool previousPressed)
	{
		bool pressed = value > _triggerPressThreshold;
		if (pressed && !previousPressed)
			AddActive(trigger.ToString());
		else if (!pressed && previousPressed)
			RemoveActive(trigger.ToString());

		previousPressed = pressed;
	}

	private void UpdateStick(DualSenseStick stick, Vector2 input, ref string previousDirection)
	{
		string direction = GetStickDirectionIdentifier(stick, input);
		if (!string.Equals(previousDirection, direction, StringComparison.OrdinalIgnoreCase))
		{
			if (!string.IsNullOrEmpty(previousDirection))
				RemoveActive(previousDirection);
			if (!string.IsNullOrEmpty(direction))
				AddActive(direction);
			previousDirection = direction;
		}
	}

	private static string GetStickDirectionIdentifier(DualSenseStick stick, Vector2 input)
	{
		if (input.magnitude < 0.1f)
			return null;

		DualSenseStickDirection direction;
		if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
			direction = input.x > 0 ? DualSenseStickDirection.Right : DualSenseStickDirection.Left;
		else
			direction = input.y > 0 ? DualSenseStickDirection.Up : DualSenseStickDirection.Down;

		return $"{stick}Stick{direction}";
	}

	private void UpdateTouchpadSwipe(StickControl touchpadStick, ButtonControl touchpadButton)
	{
		if (touchpadStick == null || touchpadButton == null)
		{
			ClearTouchpadSwipe();
			return;
		}

		if (!touchpadButton.isPressed)
		{
			ClearTouchpadSwipe();
			return;
		}

		Vector2 position = touchpadStick.ReadValue();
		if (_previousTouchPosition.HasValue)
		{
			Vector2 delta = position - _previousTouchPosition.Value;
			if (delta.magnitude >= _swipeThreshold)
			{
				DualSenseStickDirection direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
					? (delta.x > 0 ? DualSenseStickDirection.Right : DualSenseStickDirection.Left)
					: (delta.y > 0 ? DualSenseStickDirection.Up : DualSenseStickDirection.Down);
				_currentSwipe = direction;
			}
		}

		_previousTouchPosition = position;
		if (_currentSwipe.HasValue)
			AddActive($"Touchpad{_currentSwipe}");
	}

	private void ClearTouchpadSwipe()
	{
		if (_currentSwipe.HasValue)
			RemoveActive($"Touchpad{_currentSwipe}");
		_currentSwipe = null;
		_previousTouchPosition = null;
	}

	private void AddActive(string identifier)
	{
		if (string.IsNullOrEmpty(identifier))
			return;

		if (_activeKeys.Add(identifier))
		{
			_pressedOrder.Add(identifier);
			WindowHandler.OnKeyDown(new DualSenseInput(BuildIdentifier(_pressedOrder)));
		}
	}

	private void RemoveActive(string identifier)
	{
		if (string.IsNullOrEmpty(identifier))
			return;

		if (_activeKeys.Remove(identifier))
		{
			_pressedOrder.RemoveAll(item => item.Equals(identifier, StringComparison.OrdinalIgnoreCase));
			WindowHandler.OnKeyUp(new DualSenseInput(identifier));
		}
	}

	private static string BuildIdentifier(IList<string> keys)
	{
		return string.Join(", ", keys);
	}
}
