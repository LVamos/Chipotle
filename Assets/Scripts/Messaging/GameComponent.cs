using Game.Terrain;

namespace Game.Messaging
{
	public abstract class GameComponent<TOwner> : MessagingObject
		where TOwner : MapElement
	{
		public virtual TOwner Owner { get; }

		/// <summary>
		/// Indexed name of the owner fo this component.
		/// </summary>
		protected string _ownerName;
	}
}
