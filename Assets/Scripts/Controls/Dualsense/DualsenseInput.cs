using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Controls.DualSense
{
	public struct DualSenseInput : IEquatable<DualSenseInput>
	{
		public HashSet<string> Keys { get; }
		public IReadOnlyList<string> OrderedKeys { get; }
		public float? TriggerThreshold { get; }

		public DualSenseInput(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
				throw new ArgumentException("DualSense input identifier cannot be null or empty.", nameof(identifier));

			HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
			List<string> orderedKeys = new();
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
				string normalizedToken;
				if (TryParseStickToken(token, out parsedStick, out parsedDirection))
				{
					normalizedToken = $"{parsedStick}Stick{parsedDirection}";
					AddUniqueKey(keys, orderedKeys, normalizedToken);
					continue;
				}

				if (Enum.TryParse(token, true, out DualSenseTrigger parsedTrigger))
				{
					normalizedToken = parsedTrigger.ToString();
					triggerDetected = true;
					if (triggerThreshold == null)
						triggerThreshold = 1.0f;
					AddUniqueKey(keys, orderedKeys, normalizedToken);
					continue;
				}

				if (Enum.TryParse(token, true, out DualSenseDPad parsedDPad))
				{
					normalizedToken = parsedDPad.ToString();
					AddUniqueKey(keys, orderedKeys, normalizedToken);
					continue;
				}

				if (Enum.TryParse(token, true, out DualSenseButton parsedButton))
				{
					normalizedToken = parsedButton.ToString();
					AddUniqueKey(keys, orderedKeys, normalizedToken);
					continue;
				}

				throw new ArgumentException($"Unknown DualSense input identifier: {token}", nameof(identifier));
			}

			Keys = keys;
			OrderedKeys = orderedKeys;
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
			bool keysEqual = OrderedKeys != null && other.OrderedKeys != null && ListsEqual(OrderedKeys, other.OrderedKeys);
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
				if (OrderedKeys != null)
				{
					foreach (string key in OrderedKeys)
						keysHash = (keysHash * 31) ^ key.ToLowerInvariant().GetHashCode();
				}

				int hash = 17;
				hash = hash * 31 + keysHash;
				hash = hash * 31 + TriggerThreshold.GetHashCode();
				return hash;
			}
		}

		public string GetCzechLocalization()
		{
			List<string> parts;
			if (OrderedKeys != null)
				parts = new(OrderedKeys);
			else parts = new();

			if (parts.Count <= 0)
				return "Neznámý";

			List<string> translatedParts = parts
				.Select(GetTranslation)
				.ToList();

			return string.Join(", ", translatedParts);

			string GetTranslation(string input)
			{
				Dictionary<string, string> localizationStrings = new(StringComparer.OrdinalIgnoreCase)
				{
					{"TouchpadButton","Tlačítko touchpadu" },
					{"Cross","Křížek" },
					{"Square", "Čtvereček" },
					{"Circle", "Kolečko" },
					{"Triangle", "Trojúhelník" },
					{"DPadUp", "D-Pad Nahoru" },
					{"DPadDown", "D-Pad Dolů" },
					{"DPadLeft", "D-Pad Doleva" },
					{"DPadRight", "D-Pad Doprava" },
					{"LeftStickUp", "Levá páčka nahoru" },
					{"LeftStickDown", "Levá páčka dolů" },
					{"LeftStickLeft", "Levá páčka doleva" },
					{"LeftStickRight", "Levá páčka doprava" },
					{"RightStickUp", "Pravá páčka nahoru" },
					{"RightStickDown", "Pravá páčka dolů" },
					{"RightStickLeft", "Pravá páčka doleva" },
					{"RightStickRight", "Pravá páčka doprava" }
				};

				string translation = null;
				if (localizationStrings.TryGetValue(input, out translation))
					return translation;
				return input;
			}
		}

		public override string ToString()
		{
			List<string> parts;
			if (OrderedKeys != null)
				parts = new(OrderedKeys);
			else parts = new();

			if (parts.Count > 0)
				return string.Join(", ", parts);
			else return "Unknown";
		}

		public static bool operator ==(DualSenseInput left, DualSenseInput right) => left.Equals(right);
		public static bool operator !=(DualSenseInput left, DualSenseInput right) => !left.Equals(right);

		private static void AddUniqueKey(HashSet<string> keys, List<string> orderedKeys, string key)
		{
			if (keys.Add(key))
				orderedKeys.Add(key);
		}

		private static bool ListsEqual(IReadOnlyList<string> first, IReadOnlyList<string> second)
		{
			if (ReferenceEquals(first, second))
				return true;
			if (first == null || second == null)
				return false;
			if (first.Count != second.Count)
				return false;
			for (int i = 0; i < first.Count; i++)
				if (!first[i].Equals(second[i], StringComparison.OrdinalIgnoreCase))
					return false;
			return true;
		}
	}
}
