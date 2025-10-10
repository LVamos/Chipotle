using Game.Debug;

using System;
namespace game.debug
{
	[AttributeUsage(AttributeTargets.Method)]
	public class DebugCommandAttribute : Attribute
	{
		public DebugCommand Command { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="command">A debug command assigned to a method</param>
		public DebugCommandAttribute(DebugCommand command) => Command = command;
	}
}
