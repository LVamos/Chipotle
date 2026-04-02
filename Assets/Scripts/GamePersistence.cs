using DavyKager;

using Game;
using Game.Audio;
using Game.Serialization.Protobuf;
using Game.Terrain;
using Game.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using UnityEngine;

namespace Assets.Scripts
{
	public static class GamePersistence
	{
		public static XDocument OpenMap()
		{
			string mapPath = Path.Combine(MainScript.MapPath, Settings.MapName).Replace("\\", "/");
			TextAsset mapAsset = Resources.Load<TextAsset>(mapPath);
			return XDocument.Parse(mapAsset.text);
		}

		/// <summary>
		/// Starts the menu for selecting a predefined save and loads the selected save into memory.
		/// </summary>
		/// <returns>True if a save was selected.</returns>
		/// <remarks>for testing purposes only.</remarks>
		public static bool LoadNamedSave()
		{
			string[] saves = null;

			if (Directory.Exists(MainScript.PredefinedSavesPath))
			{
				saves =
					Directory.GetDirectories(MainScript.PredefinedSavesPath);
			}

			if (saves.IsNullOrEmpty())
			{
				Tolk.Speak("Žádný sejvy tady nevidim.");
				return false;
			}

			List<List<string>> items =
				saves.Select(s => new List<string> { Path.GetFileName(s) })
					.ToList();

			int i = WindowHandler.Menu(new(items, "Kterej sejv chceš načíst?"));

			if (i == -1)
			{
				Tolk.Speak("Tak nic");
				return false;
			}

			Sounds.StopAllSounds();
			string path = Path.Combine(saves[i], "game.sav");
			LoadGame(path);
			Tolk.Speak("Načteno.");
			return true;
		}


		/// <summary>
		/// Saves the game state to the default file.
		/// </summary>
		public static void SaveGame()
			=> SaveGame(MainScript.SerializationPath);

		/// <summary>
		/// Saves sttate of the game into a specified binary file.
		/// </summary>
		/// <param name="path">Location of the save</param>
		/// <remarks>Used for testing purposes only. Allows creation of predefined saves.</remarks>
		public static void SaveGame(string path)
		{
			GameSave save = World.CreateSave();
			FileStream stream = null;
			using (stream = File.Create(path))
			{
				ProtoBuf.Serializer.Serialize(stream, save);
			}
			stream?.Close();
		}

		public static bool CreateNamedSave()
		{
			string name = null;
			//Interaction.InputBox(String.Empty, "Zadej název sejvu");
			if (string.IsNullOrEmpty(name))
			{
				Tolk.Speak("Tak nic");
				return false;
			}

			if (!Directory.Exists(MainScript.PredefinedSavesPath))
				Directory.CreateDirectory(MainScript.PredefinedSavesPath);
			string path = Path.Combine(MainScript.PredefinedSavesPath, name);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = Path.Combine(path, "game.sav");
			SaveGame(path);
			Tolk.Speak("uloženo");
			return true;
		}

		/// <summary>
		/// Loads a saved game state from default path.
		/// </summary>
		public static void LoadGame()
			=> LoadGame(MainScript.SerializationPath);

		/// <summary>
		/// Loads a saved game state from the specified binary file.
		/// </summary>
		/// <param name="path">Path to the saved game state</param>
		/// <remarks>Used just for testing purposes. Allows opening predefined saves.</remarks>
		public static void LoadGame(string path)
		{
			GameManager.LoadGame();
			GameSave save = null;
			TileMap map = null;

			try
			{
				if (!File.Exists(path))
					MainScript.Terminate($"Nevidím soubor {MainScript.SerializationPath}. Že ty ses v tom hrabal?");

				XDocument document = OpenMap();
				List<XElement> zoneNodes = document.Root.Element("localities").Elements("locality").ToList();
				map = TileMap.Create(zoneNodes);
				FileStream stream = null;

				using (stream = File.OpenRead(path))
				{
					save = ProtoBuf.Serializer.Deserialize<GameSave>(stream);
				}
				stream?.Close();
			}
			catch (ProtoBuf.ProtoException)
			{
				MainScript.Terminate($"Nepodařilo se načíst hru. Soubor {MainScript.SerializationPath} je v nesprávném formátu.");
			}
			catch (Exception e)
			{
				MainScript.OnError(e);
			}

			World.ApplySave(save, map);
		}

	}
}
