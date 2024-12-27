using Game.Entities.Characters;

namespace Game.Models
{
	/// <summary>
	/// Model for ChipotlePhysicsComponent.GetNavigableCharacters method
	/// </summary>
	public class NavigableCharactersModel
	{
		/// <summary>
		/// Character descriptions
		/// </summary>
		public readonly string[] Descriptions;

		/// <summary>
		/// Detected characters
		/// </summary>
		public readonly Character[] Characters;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="descriptions">The descriptions of the characters.</param>
		/// <param name="characters">The characters.</param>
		public NavigableCharactersModel(string[] descriptions, Character[] characters)
		{
			Descriptions = descriptions;
			Characters = characters;
		}
	}
}