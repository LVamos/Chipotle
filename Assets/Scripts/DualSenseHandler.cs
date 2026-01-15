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
		HashSet<string> pressedKeys = GetPressedKeys();
		List<string> newOrder = BuildNewOrder(pressedKeys);
		bool setChanged = !AreSetsEqual(_activeKeys, pressedKeys);
		bool orderChanged = !_pressedOrder.SequenceEqual(newOrder, StringComparer.OrdinalIgnoreCase);

		if (setChanged || orderChanged)
		{
			if (_activeKeys.Count > 0)
				WindowHandler.OnKeyUp(new DualSenseInput(BuildIdentifier(_pressedOrder)));

			_activeKeys.Clear();
			foreach (string key in pressedKeys)
				_activeKeys.Add(key);

			_pressedOrder.Clear();
			_pressedOrder.AddRange(newOrder);

			if (_activeKeys.Count > 0)
				WindowHandler.OnKeyDown(new DualSenseInput(BuildIdentifier(_pressedOrder)));
		}
	}

	private HashSet<string> GetPressedKeys()
	{
		HashSet<string> pressed = new(StringComparer.OrdinalIgnoreCase);

		AddButton(pressed, DualSenseButton.Cross, _gamepad.buttonSouth);
		AddButton(pressed, DualSenseButton.Circle, _gamepad.buttonEast);
		AddButton(pressed, DualSenseButton.Square, _gamepad.buttonWest);
		AddButton(pressed, DualSenseButton.Triangle, _gamepad.buttonNorth);
		AddButton(pressed, DualSenseButton.L1, _gamepad.leftShoulder);
		AddButton(pressed, DualSenseButton.R1, _gamepad.rightShoulder);
		AddButton(pressed, DualSenseButton.Options, _gamepad.startButton);
		AddButton(pressed, DualSenseButton.Create, _gamepad.selectButton);
		AddButton(pressed, DualSenseButton.L3, _gamepad.leftStickButton);
		AddButton(pressed, DualSenseButton.R3, _gamepad.rightStickButton);

		StickControl touchpadStick = _gamepad.allControls.FirstOrDefault(c => c.name.Equals("touchpad", StringComparison.OrdinalIgnoreCase)) as StickControl;
		ButtonControl touchpadButton = _gamepad.allControls.FirstOrDefault(c => c.name.Equals("touchpadButton", StringComparison.OrdinalIgnoreCase)) as ButtonControl;
		AddButton(pressed, DualSenseButton.TouchpadButton, touchpadButton);

		AddDPad(pressed, DualSenseDPad.DPadUp, _gamepad.dpad.up);
		AddDPad(pressed, DualSenseDPad.DPadDown, _gamepad.dpad.down);
		AddDPad(pressed, DualSenseDPad.DPadLeft, _gamepad.dpad.left);
		AddDPad(pressed, DualSenseDPad.DPadRight, _gamepad.dpad.right);

		AddTrigger(pressed, DualSenseTrigger.L2, _gamepad.leftTrigger.ReadValue());
		AddTrigger(pressed, DualSenseTrigger.R2, _gamepad.rightTrigger.ReadValue());

		AddStick(pressed, DualSenseStick.Left, _gamepad.leftStick.ReadValue());
		AddStick(pressed, DualSenseStick.Right, _gamepad.rightStick.ReadValue());

		AddTouchpadSwipe(pressed, touchpadStick, touchpadButton);

		return pressed;
	}

	private static void AddButton(HashSet<string> pressed, DualSenseButton button, ButtonControl control)
	{
		if (control != null && control.isPressed)
			pressed.Add(button.ToString());
	}

	private static void AddDPad(HashSet<string> pressed, DualSenseDPad dpad, ButtonControl control)
	{
		if (control != null && control.isPressed)
			pressed.Add(dpad.ToString());
	}

	private void AddTrigger(HashSet<string> pressed, DualSenseTrigger trigger, float value)
	{
		if (value > _triggerPressThreshold)
			pressed.Add(trigger.ToString());
	}

	private static void AddStick(HashSet<string> pressed, DualSenseStick stick, Vector2 input)
	{
		if (input.magnitude < 0.1f)
			return;

		DualSenseStickDirection direction;
		if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
			direction = input.x > 0 ? DualSenseStickDirection.Right : DualSenseStickDirection.Left;
		else
			direction = input.y > 0 ? DualSenseStickDirection.Up : DualSenseStickDirection.Down;

		pressed.Add($"{stick}Stick{direction}");
	}

	private void AddTouchpadSwipe(HashSet<string> pressed, StickControl touchpadStick, ButtonControl touchpadButton)
	{
		if (touchpadStick == null || touchpadButton == null)
		{
			_currentSwipe = null;
			_previousTouchPosition = null;
			return;
		}

		if (!touchpadButton.isPressed)
		{
			_currentSwipe = null;
			_previousTouchPosition = null;
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
			pressed.Add($"Touchpad{_currentSwipe}");
	}

	private List<string> BuildNewOrder(HashSet<string> pressedKeys)
	{
		List<string> newOrder = new();
		foreach (string key in _pressedOrder)
			if (pressedKeys.Contains(key))
				newOrder.Add(key);

		foreach (string key in pressedKeys)
			if (!ContainsIgnoreCase(newOrder, key))
				newOrder.Add(key);

		return newOrder;
	}

	private static bool AreSetsEqual(HashSet<string> first, HashSet<string> second)
	{
		if (first == null && second == null)
			return true;
		if (first == null || second == null)
			return false;
		return first.SetEquals(second);
	}

	private static bool ContainsIgnoreCase(List<string> list, string value)
	{
		foreach (string item in list)
			if (item.Equals(value, StringComparison.OrdinalIgnoreCase))
				return true;
		return false;
	}

	private static string BuildIdentifier(IList<string> keys)
	{
		return string.Join(", ", keys);
	}
}
