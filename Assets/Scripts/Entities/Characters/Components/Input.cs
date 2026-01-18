using Game.Controls;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Entities.Characters.Chipotle;
using Game.Messaging.Events.Input;
using Game.Messaging.Events.Sound;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Allows the player to control an NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(ChipotleInput))]
	public abstract class Input : CharacterComponent
	{
		public override void Initialize()
		{
			base.Initialize();
			AddCommands();
		}

		/// <summary>
		/// Registered keyboard shortcuts and corresponding actions
		/// </summary>
		[ProtoIgnore]
		protected Dictionary<KeyboardInput, Action> _keyboardShortcuts;

		protected Dictionary<DualSenseInput, Action> _dualsenseShortcuts;

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case DualSenseKeyPressed m: OnDualSenseKeyPressed(m); break;
				case KeyPressed kp: OnKeyPressed(kp); break;
				case CutsceneEnded ce: OnCutsceneEnded(ce); break;
				case CutsceneBegan cb: OnCutsceneBegan(cb); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Processes the KeyDown message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected virtual void OnKeyPressed(KeyPressed message)
		{
			if (_keyboardShortcuts == null)
				AddCommands();

			if (_keyboardShortcuts != null && _keyboardShortcuts.TryGetValue(message.Shortcut, out Action action))
				action();
		}

		protected virtual void OnDualSenseKeyPressed(DualSenseKeyPressed message)
		{
			if (_dualsenseShortcuts== null)
				AddCommands();

			if (_dualsenseShortcuts!= null && _dualsenseShortcuts.TryGetValue(message.Shortcut, out Action action))
				action();
		}


		/// <summary>
		/// registers keyboard shotctus for the component.
		/// </summary>
		protected virtual void AddCommands()
		{
			_keyboardShortcuts ??= new();
			_dualsenseShortcuts = new();
		}

		protected void AddDualSenseShortcuts(Dictionary<DualSenseInput, Action> shortcuts)
		{
			_dualsenseShortcuts = _dualsenseShortcuts.Concat(shortcuts)
				.GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
		}

		/// <summary>
		/// Registers keyboard shortcuts and corresponding actions.
		/// </summary>
		/// <param name="shortcuts">Set of shortcuts to be registered</param>
		protected void AddKeyboardShortcuts(Dictionary<KeyboardInput, Action> shortcuts)
		{
			_keyboardShortcuts = _keyboardShortcuts.Concat(shortcuts).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
		}

		protected void AddShortcut(CommandId command1, Action command)
		{
			CommandBindings shortcut = InputConfig.GetBindings(command1);
			if (shortcut == null)
				return;

			if (shortcut.Keyboard != null)
				_keyboardShortcuts[shortcut.Keyboard.Value] = command;
			if (shortcut.DualSense != null)
				_dualsenseShortcuts[shortcut.DualSense.Value] = command;
		}
	}
}