using Game.Terrain;

using System;
using System.Runtime.CompilerServices;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Relocates the Detective's car (detektivovo auto) to the specified zone.
	/// </summary>
	/// <remarks>Applies to the <see cref="Entities.Items.ChipotlesCar"/> class.</remarks>
	[Serializable]
	public class MoveChipotlesCar : Message
	{
		public readonly int Line;
		public readonly string Member;
		public readonly string File;


		/// <summary>
		/// The zone to which the car will move.
		/// </summary>
		public readonly Zone Destination;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="destination">The zone to which the car will move</param>
		public MoveChipotlesCar(
			object sender,
			Zone destination,
								[CallerLineNumber] int line = 0,
		[CallerMemberName] string member = "",
		[CallerFilePath] string file = ""
			) : base(sender)
		{
			Destination = destination;
			Line = line;
			Member = member;
			File = file;
		}
	}
}