using System;

namespace Game.Controls.DualSense
{
	public struct DualSenseInput : IEquatable<DualSenseInput>
	{
		public DualSenseButton? Button { get; }
		public DualSenseStick? Stick { get; }
		public DualSenseStickDirection? StickDirection { get; }
		public DualSenseTrigger? Trigger { get; }
		public DualSenseDPad? DPad { get; }
		public float? TriggerThreshold { get; }

		public DualSenseInput(string identifier)
		{
			Button = null;
			Stick = null;
			StickDirection = null;
			Trigger = null;
			DPad = null;
			TriggerThreshold = null;

			if (Enum.TryParse(identifier, true, out DualSenseButton button))
			{
				Button = button;
				return;
			}

			if (Enum.TryParse(identifier, true, out DualSenseTrigger trigger))
			{
				Trigger = trigger;
				TriggerThreshold = 1.0f;
				return;
			}

			if (Enum.TryParse(identifier, true, out DualSenseDPad dpad))
			{
				DPad = dpad;
				return;
			}

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

		// ======= Equals =======
		public bool Equals(DualSenseInput other)
		{
			return Button == other.Button &&
				   Stick == other.Stick &&
				   StickDirection == other.StickDirection &&
				   Trigger == other.Trigger &&
				   DPad == other.DPad &&
				   TriggerThreshold == other.TriggerThreshold;
		}

		public override bool Equals(object obj)
		{
			return obj is DualSenseInput other && Equals(other);
		}

		// ======= GetHashCode =======
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + Button.GetHashCode();
				hash = hash * 31 + Stick.GetHashCode();
				hash = hash * 31 + StickDirection.GetHashCode();
				hash = hash * 31 + Trigger.GetHashCode();
				hash = hash * 31 + DPad.GetHashCode();
				hash = hash * 31 + TriggerThreshold.GetHashCode();
				return hash;
			}
		}

		// ======= ToString =======
		public override string ToString()
		{
			if (Button.HasValue)
				return Button.Value.ToString();
			if (Trigger.HasValue)
				return Trigger.Value.ToString();
			if (DPad.HasValue)
				return $"DPad{DPad.Value}";
			if (Stick.HasValue && StickDirection.HasValue)
				return $"{Stick}Stick{StickDirection}";
			return "Unknown";
		}

		public static bool operator ==(DualSenseInput left, DualSenseInput right) => left.Equals(right);
		public static bool operator !=(DualSenseInput left, DualSenseInput right) => !left.Equals(right);
	}
	}
