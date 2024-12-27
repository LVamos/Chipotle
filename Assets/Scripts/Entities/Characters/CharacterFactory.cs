using Game.Entities.Characters.Bartender;
using Game.Entities.Characters.Carson;
using Game.Entities.Characters.Chipotle;
using Game.Entities.Characters.Christine;
using Game.Entities.Characters.Mariotti;
using Game.Entities.Characters.Sweeney;
using Game.Entities.Characters.Tuttle;

using UnityEngine;

using Physics = Game.Entities.Characters.Components.Physics;
using Sound = Game.Entities.Characters.Components.Sound;

namespace Game.Entities.Characters
{
	public static class CharacterFactory
	{
		/// <summary>
		/// Creates new instance of the Carson NPC.
		/// </summary>
		/// <returns>New instance of the NPC</returns>
		public static Character CreateCarson()
		{
			GameObject obj = new("Carson");
			Character carson = obj.AddComponent<Character>() as Character;

			CarsonAI ai = obj.AddComponent<CarsonAI>();
			Physics physics = obj.AddComponent<Physics>();
			Sound sound = obj.AddComponent<Sound>();

			carson.Initialize(
				new("Carson", "David Carson"),
				"Carson",
				ai,
				null,
				physics,
				sound
				);
			return carson;
		}

		/// <summary>
		/// Creates new instance of the Detective Chipotle NPC.
		/// </summary>
		/// <returns>New instance of the NPC</returns>
		public static Character CreateChipotle()
		{
			GameObject obj = new("Chipotle");
			Character chipotle = obj.AddComponent<Character>() as Character;

			ChipotlePhysics physics = obj.AddComponent<ChipotlePhysics>();
			ChipotleSound sound = obj.AddComponent<ChipotleSound>();
			ChipotleInput input = obj.AddComponent<ChipotleInput>();
			chipotle.Initialize(
				new("Chipotle", "detektiv Chipotle"),
				"Chipotle",
				null,
				input,
				physics,
				sound
				);
			return chipotle;
		}

		/// <summary>
		/// Creates new instance of the Christine NPC.
		/// </summary>
		/// <returns>New instance of the NPC</returns>
		public static Character CreateChristine()
		{
			GameObject obj = new("Christine");
			Character christine = obj.AddComponent<Character>() as Character;

			ChristineAI ai = obj.AddComponent<ChristineAI>();
			Physics physics = obj.AddComponent<Physics>();
			Sound sound = obj.AddComponent<Sound>();

			christine.Initialize(
				new("Christine", "Christine Piercová"),
				"Christine",
				ai,
				null,
				physics,
				sound
				);
			return christine;
		}

		/// <summary>
		/// Creates new instance of the Mariotti NPC.
		/// </summary>
		/// <returns>New instance of the NPC</returns>
		public static Character CreateMariotti()
		{
			GameObject obj = new("Mariotti");
			Character mariotti = obj.AddComponent<Character>() as Character;

			MariottiAI ai = obj.AddComponent<MariottiAI>();
			Physics physics = obj.AddComponent<Physics>();
			Sound sound = obj.AddComponent<Sound>();

			mariotti.Initialize(
				new("Mariotti", "Paolo Mariotti"),
				"Mariotti",
				ai,
				null,
				physics,
				sound
				);
			return mariotti;
		}

		/// <summary>
		/// Creates new instance of the Sweeney NPC.
		/// </summary>
		/// <returns>New instance of the NPC</returns>
		public static Character CreateSweeney()
		{
			GameObject obj = new("Sweeney");
			Character sweeney = obj.AddComponent<Character>() as Character;

			SweeneyAI ai = obj.AddComponent<SweeneyAI>();
			Physics physics = obj.AddComponent<Physics>();
			Sound sound = obj.AddComponent<Sound>();
			sweeney.Initialize(
				new("Sweeney", "Derreck Sweeney"),
				"Sweeney",
				ai,
				null,
				physics,
				sound
				);
			return sweeney;
		}

		/// <summary>
		/// Creates new instance of the Tuttle NPC.
		/// </summary>
		/// <returns>New instance of the NPC</returns>
		public static Character CreateTuttle()
		{
			GameObject obj = new("Tuttle");
			Character tuttle = obj.AddComponent<Character>() as Character;

			TuttleAI ai = obj.AddComponent<TuttleAI>();
			TuttlePhysics physics = obj.AddComponent<TuttlePhysics>();
			TuttleSound sound = obj.AddComponent<TuttleSound>();
			tuttle.Initialize(
				new("Tuttle", "parťák"),
				"Tuttle",
				ai,
				null,
				physics,
				sound
				);
			return tuttle;
		}

		/// <summary>
		/// Creates new instance of the Bartender NPC.
		/// </summary>
		/// <returns>New instance of the NPC</returns>
		public static Character CreateBartender()
		{
			GameObject obj = new("Bartender");
			Character bartender = obj.AddComponent<Character>() as Character;
			BartenderAI ai = new BartenderAI();
			Physics physics = obj.AddComponent<Physics>();
			Sound sound = obj.AddComponent<Sound>();
			bartender.Initialize(
				new("Bartender", "pingl"),
				"Bartender",
				ai,
				null,
				physics,
				sound
				);
			return bartender;
		}



	}
}
