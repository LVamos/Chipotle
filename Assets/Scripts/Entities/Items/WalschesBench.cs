using ProtoBuf;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents a bench in bazén w1 zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class WalshesBench : Item
	{
		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, bool passable = false, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
		{
			base.Initialize(name, area, type, decorative, pickable, usable, passable, cutscene: "cs1");
		}



	}
}