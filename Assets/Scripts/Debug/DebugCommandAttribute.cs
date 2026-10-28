using Game.Debug;

using System;
namespace game.debug
{
	[AttributeUsage(AttributeTargets.Method)]
	public class DebugCommandAttribute : Attribute
	{
		public DebugCommand Command { get; }

		/// <summary>
		/// Text shown in the debug menu
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="command">A debug command assigned to a method</param>
		/// <param name="description">Text shown in the debug menu</param>
		public DebugCommandAttribute(DebugCommand command, string description)
		{
			Command = command;
			Description = description;
		}
	}
}
