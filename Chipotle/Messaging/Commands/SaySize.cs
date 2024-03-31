
using Game.Terrain;

namespace Game.Messaging.Commands
{
	/// <summary>
	/// A command that instructs a sound component to announce size of a rectangle.
	/// </summary>
	public class SaySize : GameMessage
	{
		/// <summary>
		/// A Rectangle area whose size should be announced
		/// </summary>
		public readonly Rectangle Area;

		public SaySize(object sender, Rectangle area) : base(sender)
		{
			Area = area;
		}
	}
}
