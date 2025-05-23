using Game;
using Game.Audio;
using Game.UI;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// Tato třída spustí zadané audio jakmile se objeví na scéně.
/// </summary>
public class AutoPlayBehaviour : MonoBehaviour
{
	private void OnActivated()
	{
		MainScript.DisableJAWSKeyHook();

		World.ResumeCutscene();
		Sounds.Unmute();
	}

	private void OnDeactivated()
	{
		MainScript.EnableJAWSKeyHook();
		World.PauseCutscene();
		Sounds.Mute();
	}

	private void CheckApplicationState()
	{
		if (!_hasFocus)
			OnDeactivated();
		else
			OnActivated();
	}

	private bool _hasFocus;

	public void Start()
	{
		Logger.LogInfo("Hra spuštěna");

		// camera settings
		if (Camera.main == null)
		{
			GameObject newCamera = new()
			{
				tag = "MainCamera"
			};
			newCamera.AddComponent<Camera>();
			Logger.LogInfo("Kamera vytvořena");
		}

		if (Camera.main.GetComponent<AudioListener>() == null)
		{
			Camera.main.gameObject.AddComponent<AudioListener>();
			Logger.LogInfo("Listener vytvořen");
		}
		if (Camera.main.GetComponent<ResonanceAudioListener>() == null)
		{
			Camera.main.gameObject.AddComponent<ResonanceAudioListener>();
			Logger.LogInfo("ResonanceAudioListener vytvořen");
		}

		Camera.main.clearFlags = CameraClearFlags.Nothing;
		Camera.main.cullingMask = 0;
		Application.runInBackground = true;
		Time.fixedDeltaTime = 1f / 30f;

		if (!Settings.MainMenuAtStartup)
			WindowHandler.StartGame();
		else
			WindowHandler.MainMenu();
	}

	public void Update()
	{
		HandleKeyDown();
		HandleKeyUp();
		World.UpdateGame();

		if (!Application.isPlaying)
			return;

		if (Application.isFocused)
		{
			if (!_hasFocus)
			{
				_hasFocus = true;
				CheckApplicationState();
			}
		}
		else
		{
			if (_hasFocus)
			{
				_hasFocus = false;
				CheckApplicationState();
			}
		}
	}

	private void HandleKeyDown()
	{
		IEnumerable<KeyCode> keyCodes = Enum.GetValues(typeof(KeyCode))
					.Cast<KeyCode>();

		// Detekuj modifier klávesy jako držené
		bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

		// Detekuj hlavní klávesu pouze při prvním stisknutí
		KeyCode key = keyCodes.FirstOrDefault(k => Input.GetKeyDown(k) &&
			k != KeyCode.LeftShift && k != KeyCode.RightShift &&
			k != KeyCode.LeftAlt && k != KeyCode.RightAlt);

		// Když nebyla stisknuta žádná klávesa, vrátí se
		if (key == KeyCode.None)
			return;

		KeyShortcut shortcut = new(shift: shift, alt: alt, control: ctrl, key: key);
		WindowHandler.OnKeyDown(shortcut);
	}

	private void HandleKeyUp()
	{
		IEnumerable<KeyCode> keyCodes = Enum.GetValues(typeof(KeyCode))
					.Cast<KeyCode>();

		IEnumerable<KeyCode> keys = keyCodes
			.Where(k => Input.GetKeyUp(k));

		bool alt = keys.Contains(KeyCode.LeftAlt) || keys.Contains(KeyCode.RightAlt);
		bool shift = keys.Contains(KeyCode.LeftShift) || keys.Contains(KeyCode.RightShift);
		bool ctrl = keys.Contains(KeyCode.LeftControl) || keys.Contains(KeyCode.RightControl);

		KeyCode key = keys.FirstOrDefault(k =>
			k is not KeyCode.LeftShift and not KeyCode.RightShift and
			not KeyCode.LeftControl and not KeyCode.RightControl and
			not KeyCode.LeftAlt and not KeyCode.RightAlt);

		// When no keys were pressed return null.
		if (key == KeyCode.None)
			return;

		KeyShortcut shortcut = new(shift: shift, alt: alt, control: ctrl, key: key);
		WindowHandler.OnKeyUp(shortcut);
	}
}
