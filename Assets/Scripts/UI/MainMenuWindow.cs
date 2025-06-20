using DavyKager;

using Game.Audio;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace Game.UI
{
	/// <summary>
	/// Represents a virtual window opened during the main menu.
	/// </summary>
	public class MainMenuWindow : VirtualWindow
	{
		public static MainMenuWindow CreateInstance()
		{
			GameObject obj = new("Main menu window");
			MainMenuWindow window = obj.AddComponent<MainMenuWindow>();
			window.Initialize();
			return window;
		}

		protected override void SetUpAudio()
		{
			base.SetUpAudio();
			SetUpAudioSource(_menuLoopAudio, _menuLoopSound, true);
			SetUpAudioSource(_speakerTestAudio, _speakerTestSound);
		}

		/// <summary>
		/// Default volume for the menu loop
		/// </summary>
		private const float _loopVolume = .5f;

		/// <summary>
		/// Actions performed when the window is activated.
		/// </summary>
		public override void OnActivate()
		{
			base.OnActivate();

			//if (_voicingEnabled)
			//	Tolk.Speak("Hlavní menu", true);

			RunMainMenu();
		}

		/// <summary>
		/// Name of the background sound loop
		/// </summary>
		protected const string _menuLoopSound = "MainMenuLoop";

		/// <summary>
		/// A handle of the background sound loop
		/// </summary>
		private AudioSource _menuLoopAudio;

		/// <summary>
		/// Default menu items
		/// </summary>
		private List<List<string>> _items = new()
		{
			new() { "Nová hra" },
			new() { "Test sluchátek" },
			new() { "Návod" },
			new() { "Konec" }
		};

		/// <summary>
		/// Enables or disables voicing in the main menu.
		/// </summary>
		private bool _voicingEnabled = true;
		private AudioSource _speakerTestAudio;
		private List<List<string>> _usedItems;
		private int _lastChoice;
		private AudioSource _endJingleSource;

		/// <summary>
		/// Items including the Load game command
		/// </summary>
		private readonly List<List<string>> _itemsWithLoadGame = new()
		{
			new() {"Nová hra" },
			new() {"Pokračovat ve hře" },
			new() {"Test sluchátek" },
			new() {"Návod" },
			new() {"Konec" }
		};

		/// <summary>
		/// Name of the sound with the speaker test.
		/// </summary>
		private const string _speakerTestSound = "SpeakerTest";

		/// <summary>
		/// Name of the sound played when the application is being closed
		/// </summary>
		private const string _endSound = "MainMenuLongEnd";

		/// <summary>
		/// Name of the menu loop end sound
		/// </summary>
		private const string _shortEndSound = "MainMenuLongEnd";

		/// <summary>
		/// Runs the main menu.
		/// </summary>
		public void RunMainMenu()
		{
			string intro = !_menuLoopAudio.IsPlaying() ? "Hlavní menu" : String.Empty;
			PlayLoop();

			_usedItems = GameStateSaved() ? _itemsWithLoadGame : _items;
			MenuParametersDTO parameters = new(_usedItems, intro, menuClosed: MenuClosed, defaultIndex: _lastChoice);
			int choice = WindowHandler.Menu(parameters);
		}

		private void MenuClosed(int choice)
		{
			choice = choice == -1 ? _usedItems.Count - 1 : choice;
			_lastChoice = choice;
			switch (_usedItems[choice][0])
			{
				case "Nová hra": StartCoroutine(StartGame()); break;
				case "Pokračovat ve hře": LoadGame(); break;
				case "Test sluchátek": SpeakerTest(); break;
				case "Návod": Help(); RunMainMenu(); break;
				default: StartCoroutine(nameof(ExitGame)); break;
			}
		}

		/// <summary>
		/// Checks if a game state was previously saved on disc.
		/// </summary>
		/// <returns>True if a saved game state was found on disc.</returns>
		private bool GameStateSaved()
			=> File.Exists(Path.Combine(MainScript.DataPath, "game.sav"));

		/// <summary>
		/// Loads a saved game from file.
		/// </summary>
		private void LoadGame()
		{
			Tolk.Speak("To musim dodělat... píči už");
			//todo dodělat
			return;

			StopLoop();
			World.LoadGame();
		}

		/// <summary>
		/// Starts a new game.
		/// </summary>
		private IEnumerator StartGame()
		{
			yield return StopLoop();
			Settings.MainMenuAtStartup = false;
			WindowHandler.StartGame();
		}

		/// <summary>
		/// Fades the menu loop sound and plays a closing jingle.
		/// </summary>
		/// <param name="shortEnd">Specifies if the closing jingle should be short or long.</param>
		private IEnumerator StopLoop()
		{
			_voicingEnabled = false;

			if (!Settings.PlayMenuLoop)
				yield break;

			Sounds.SlideVolume(_menuLoopAudio, .2f, 0, true);
			_endJingleSource = Play(_endSound);
			yield return WaitForSound(_endJingleSource);
		}

		/// <summary>
		/// Starts the menu loop sound if the menu is newly created.
		/// </summary>
		private void PlayLoop()
		{
			if (!Settings.PlayMenuLoop)
				return;

			if (!_menuLoopAudio.IsPlaying())
				_menuLoopAudio = Sounds.Play2d("MainMenuLoop", 1, true);
		}

		/// <summary>
		/// Plays the speaker test.
		/// </summary>
		public void SpeakerTest()
		{
			if (_speakerTestAudio.IsPlaying())
				Sounds.SlideVolume(_speakerTestAudio, 0.5f, 0, true);
			_speakerTestAudio = Sounds.Play2d("SpeakerTest");
			RunMainMenu();
		}

		/// <summary>
		/// closes the application.
		/// </summary>
		private IEnumerator ExitGame()
		{
			if (_speakerTestAudio.IsPlaying())
				Sounds.SlideVolume(_speakerTestAudio, .5f, 0, true);
			MainScript.EnableJAWSKeyHook();
			yield return StopLoop();
			Quit();
		}

		private IEnumerator WaitForSound(AudioSource source)
		{
			while (source.IsPlaying())
				yield return new WaitForSeconds(0.1f);
		}

		void Quit()
		{
			Application.Quit();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		/// <summary>
		/// Opens help.
		/// </summary>
		private void Help()
		{
			Tolk.Speak("Jo, ještě musim vlastně napsat návod... ty vole fakt... mrdat želvu do ucha");
			//todo dodělat
			return;

			Tolk.Speak("Otvírám nápovědu", true);
			System.Diagnostics.Process.Start("help.html");
		}

		/// <summary>
		/// Handles the KeyDown message.
		/// </summary>
		/// <param name="shortcut">The message</param>
		public override void OnKeyDown(KeyShortcut shortcut)
		{
			if (_speakerTestAudio.isPlaying)
				_speakerTestAudio.Stop();

			base.OnKeyDown(shortcut);
		}

	}
}