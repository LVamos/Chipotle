using System;

namespace Game.Controls
{

	/// <summary>
	/// Represents a single input from a DualSense controller.
	/// </summary>
	public struct DualSenseInput
	{
		public DualSenseButton? Button { get; }
		public DualSenseStick? Stick { get; }
		public DualSenseStickDirection? StickDirection { get; }
		public DualSenseTrigger? Trigger { get; }
		public float? TriggerThreshold { get; }

		/// <summary>
		/// Initializes an input from a string identifier (e.g. "Cross", "L2", "LeftStickUp").
		/// </summary>
		public DualSenseInput(string identifier)
		{
			Button = null;
			Stick = null;
			StickDirection = null;
			Trigger = null;
			TriggerThreshold = null;

			if (Enum.TryParse(identifier, true, out DualSenseButton btn))
			{
				Button = btn;
				return;
			}

			if (Enum.TryParse(identifier, true, out DualSenseTrigger trg))
			{
				Trigger = trg;
				TriggerThreshold = 1.0f; // full press by default
				return;
			}

			// Stick directions like "LeftStickUp", "RightStickLeft", etc.
			if (identifier.StartsWith("LeftStick", StringComparison.OrdinalIgnoreCase))
			{
				Stick = DualSenseStick.Left;
				StickDirection = ParseStickDirection(identifier.Substring("LeftStick".Length));
				return;
			}

			if (identifier.StartsWith("RightStick", StringComparison.OrdinalIgnoreCase))
			{
				Stick = DualSenseStick.Right;
				StickDirection = ParseStickDirection(identifier.Substring("RightStick".Length));
				return;
			}

			throw new ArgumentException($"Unknown DualSense input identifier: {identifier}");
		}

		private static DualSenseStickDirection ParseStickDirection(string suffix)
		{
			if (Enum.TryParse(suffix, true, out DualSenseStickDirection dir))
				return dir;
			throw new ArgumentException($"Invalid stick direction: {suffix}");
		}
	}

	/// <summary>
	/// All discrete buttons on the DualSense controller.
	/// </summary>
	public enum DualSenseButton
	{
		Cross,      // X
		Circle,
		Square,
		Triangle,
		DPadUp,
		DPadDown,
		DPadLeft,
		DPadRight,
		L1,
		R1,
		L3,
		R3,
		Options,
		Create,
		PS,
		TouchpadButton,
		Mute
	}

	/// <summary>
	/// The two analog sticks on the controller.
	/// </summary>
	public enum DualSenseStick
	{
		Left,
		Right
	}

	/// <summary>
	/// Possible stick movement directions.
	/// </summary>
	public enum DualSenseStickDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	/// <summary>
	/// Analog triggers.
	/// </summary>
	public enum DualSenseTrigger
	{
		L2,
		R2
	}
}