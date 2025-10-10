using System;

namespace Game.Controls.DualSense
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

			if (Enum.TryParse(identifier, true, out DualSenseButton button))
			{
				Button = button;
				return;
			}

			if (Enum.TryParse(identifier, true, out DualSenseTrigger trigger))
			{
				Trigger = trigger;
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
}