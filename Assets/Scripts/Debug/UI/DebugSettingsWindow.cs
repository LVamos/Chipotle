using DavyKager;

using Game.Controls.Keyboard;
using Game.UI;

using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Debug.UI
{
	/// <summary>
	/// Debug window for quick toggling and editing of Settings values.
	/// </summary>
	public class DebugSettingsWindow : MenuWindow
	{
		public override void Close()
		{
			Tolk.Speak("Hra");
			base.Close();
		}

		// Holds metadata and accessors for a setting entry
		private class SettingEntry
		{
			public string Name;
			public Type DataType;
			public Func<object> Getter;
			public Action<object> Setter;
		}

		private List<SettingEntry> _entries = new List<SettingEntry>();

		public static DebugSettingsWindow CreateInstance()
		{
			GameObject obj = new();
			DebugSettingsWindow instance = obj.AddComponent<DebugSettingsWindow>();

			List<List<string>> items = new List<List<string>>();
			MenuParameters parameters = new(items, "Nastavení pro ladìní", " ");
			instance.Initialize(parameters);
			return instance;
		}

		// Hides base Initialize to add extra shortcuts, then defers to base
		public new void Initialize(MenuParameters parameters)
		{
			base.Initialize(parameters);

			// Register Space for toggling boolean values
			_keyboardShortcuts[new KeyboardInput(Key.Space)] = ToggleBoolean;
		}

		public override void OnActivate()
		{
			base.OnActivate();
			PopulateList();
		}

		/// <summary>
		/// Builds the menu list from Game.Settings via reflection and remembers types and accessors.
		/// </summary>
		private void PopulateList()
		{
			_entries = new List<SettingEntry>();
			List<List<string>> items = new List<List<string>>();
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
			Type settingsType = typeof(Settings);

			// Supported types
			HashSet<Type> supported = new HashSet<Type>
			{
				typeof(bool), typeof(string), typeof(float), typeof(int), typeof(Vector2), typeof(Vector2?)
			};

			// Fields
			FieldInfo[] fields = settingsType.GetFields(flags);
			foreach (FieldInfo f in fields)
			{
				Type fieldType = f.FieldType;
				if (!supported.Contains(fieldType))
					continue;

				SettingEntry entry = new()
				{
					Name = f.Name,
					DataType = fieldType,
					Getter = () => f.GetValue(null),
					Setter = v => f.SetValue(null, v)
				};
				_entries.Add(entry);
				items.Add(new List<string> { entry.Name, FormatValue(entry.Getter()) });
			}

			// Replace items and reset index
			_items = items;
			Index = _items.Count > 0 ? 0 : -1;
		}

		protected override void ActivateItem()
		{
			if (IndexOffEdge())
				return;

			SettingEntry entry = _entries[Index];
			// Edit non-boolean values via input box
			if (entry.DataType == typeof(bool))
				return;

			string title = "Zadej hodnotu";
			string defaultValue = FormatValue(entry.Getter());
			string input = Interaction.InputBox(string.Empty, title, defaultValue);
			if (string.IsNullOrEmpty(input))
			{
				SayItem();
				return;
			}

			try
			{
				object converted = ConvertInput(entry.DataType, input);
				entry.Setter(converted);
				Settings.SaveSettings();
				RefreshItem(Index);
				SayItem();
			}
			catch (Exception)
			{
				// Silently ignore invalid input to keep flow simple for debugging
			}
		}

		private void ToggleBoolean()
		{
			if (IndexOffEdge())
				return;

			SettingEntry entry = _entries[Index];
			if (entry.DataType != typeof(bool))
				return;

			bool current = (bool)entry.Getter();
			bool newValue = !current;
			entry.Setter(newValue);
			Tolk.Speak(newValue ? "zapnuto" : "vypnuto", true);
			Settings.SaveSettings();
			RefreshItem(Index);
		}

		private void RefreshItem(int index)
		{
			SettingEntry entry = _entries[index];
			_items[index] = new List<string> { entry.Name, FormatValue(entry.Getter()) };
		}

		private string FormatValue(object value)
		{
			if (value == null)
				return string.Empty;

			if (value is bool b)
				return b ? "zapnuto" : "vypnuto";
			else if (value is float f) return f.ToString(CultureInfo.InvariantCulture);
			else if (value is int i) return i.ToString(CultureInfo.InvariantCulture);
			else if (value is Vector2 v) return v.GetString();
			else return Convert.ToString(value, CultureInfo.InvariantCulture);
		}

		private object ConvertInput(Type targetType, string input)
		{
			if (targetType == typeof(string))
				return input;
			else if (targetType == typeof(float)) return float.Parse(input, CultureInfo.InvariantCulture);
			else if (targetType == typeof(int)) return int.Parse(input, NumberStyles.Integer, CultureInfo.InvariantCulture);
			else if (targetType == typeof(Vector2)) return input.ToVector2();
			else if (targetType == typeof(Vector2?))
			{
				if (string.IsNullOrWhiteSpace(input))
					return null;
				Vector2 vec = input.ToVector2();
				Vector2? result = new Vector2?(vec);
				return result;
			}

			throw new NotSupportedException($"Unsupported type {targetType}");
		}
	}
}
