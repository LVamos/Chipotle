using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Controls.DualSense
{
	public struct DualSenseInput : IEquatable<DualSenseInput>
	{
		public List<string> Keys { get; }
		public float? TriggerThreshold { get; }

		public DualSenseInput(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
				throw new ArgumentException("DualSense input identifier cannot be null or empty.", nameof(identifier));

			List<string> keys = new ();
			bool triggerDetected = false;
			float? triggerThreshold = null;

			string[] tokens = identifier.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string rawToken in tokens)
			{
				string token = rawToken.Trim();
				if (token.Length == 0)
					continue;

				DualSenseStick parsedStick;
				DualSenseStickDirection parsedDirection;
				if (TryParseStickToken(token, out parsedStick, out parsedDirection))
				{
					keys.Add($"{parsedStick}Stick{parsedDirection}");
					continue;
				}

				if (Enum.TryParse(token, true, out DualSenseTrigger parsedTrigger))
				{
					triggerDetected = true;
					if (triggerThreshold == null)
						triggerThreshold = 1.0f;
					keys.Add(parsedTrigger.ToString());
					continue;
				}

				if (Enum.TryParse(token, true, out DualSenseDPad parsedDPad))
				{
					keys.Add(parsedDPad.ToString());
					continue;
				}

				if (Enum.TryParse(token, true, out DualSenseButton parsedButton))
				{
					keys.Add(parsedButton.ToString());
					continue;
				}

				throw new ArgumentException($"Unknown DualSense input identifier: {token}", nameof(identifier));
			}

			Keys = keys;
			TriggerThreshold = triggerThreshold;

			if (triggerDetected && TriggerThreshold == null)
				TriggerThreshold = 1.0f;
		}

		private static DualSenseStickDirection ParseStickDirection(string suffix)
		{
			if (Enum.TryParse(suffix, true, out DualSenseStickDirection dir))
				return dir;
			throw new ArgumentException($"Invalid stick direction: {suffix}");
		}

		private static bool TryParseStickToken(string token, out DualSenseStick stick, out DualSenseStickDirection direction)
		{
			if (token.StartsWith("LeftStick", StringComparison.OrdinalIgnoreCase))
			{
				stick = DualSenseStick.Left;
				direction = ParseStickDirection(token.Substring("LeftStick".Length));
				return true;
			}

			if (token.StartsWith("RightStick", StringComparison.OrdinalIgnoreCase))
			{
				stick = DualSenseStick.Right;
				direction = ParseStickDirection(token.Substring("RightStick".Length));
				return true;
			}

			stick = default;
			direction = default;
			return false;
		}

		// ======= Equals =======
		public bool Equals(DualSenseInput other)
		{
			bool keysEqual = Keys != null && other.Keys != null && Keys.SequenceEqual(other.Keys);
			return keysEqual && TriggerThreshold == other.TriggerThreshold;
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
				int keysHash = 0;
				if (Keys != null)
				{
					foreach (string key in Keys)
						keysHash ^= key.ToLowerInvariant().GetHashCode();
				}

				int hash = 17;
				hash = hash * 31 + keysHash;
				hash = hash * 31 + TriggerThreshold.GetHashCode();
				return hash;
			}
		}

		// ======= ToString =======
		public override string ToString()
		{
			List<string> parts = Keys != null ? new(Keys) : new List<string>();
			parts.Sort(StringComparer.OrdinalIgnoreCase);
			return parts.Count > 0 ? string.Join(", ", parts) : "Unknown";
		}

		public static bool operator ==(DualSenseInput left, DualSenseInput right) => left.Equals(right);
		public static bool operator !=(DualSenseInput left, DualSenseInput right) => !left.Equals(right);
	}
}
