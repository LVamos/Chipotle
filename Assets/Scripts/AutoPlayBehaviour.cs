using Game;
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
	public void Start()
	{
		// camera settings
		if (Camera.main == null)
		{
			GameObject newCamera = new();
			newCamera.tag = "MainCamera";
			newCamera.AddComponent<Camera>();
		}

		AudioListener listener = null;
		if (!Camera.main.gameObject.TryGetComponent<AudioListener>(out listener))
			listener = Camera.main.gameObject.AddComponent<AudioListener>();

		Camera.main.clearFlags = CameraClearFlags.Nothing;
		Camera.main.cullingMask = 0;
		Application.targetFrameRate = 30;

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

		// Stop the game when debugging in VS is stopped.
#if UNITY_EDITOR
		if (!System.Diagnostics.Debugger.IsAttached)
			UnityEditor.EditorApplication.isPlaying = false;
#endif
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
			k != KeyCode.LeftControl && k != KeyCode.RightControl &&
			k != KeyCode.LeftAlt && k != KeyCode.RightAlt);

		// Když nebyla stisknuta žádná klávesa, vrátí se
		if (key == KeyCode.None)
			return;

		KeyShortcut shortcut = new KeyShortcut(shift: shift, alt: alt, control: ctrl, key: key);
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
			k != KeyCode.LeftShift && k != KeyCode.RightShift &&
			k != KeyCode.LeftControl && k != KeyCode.RightControl &&
			k != KeyCode.LeftAlt && k != KeyCode.RightAlt);

		// When no keys were pressed return null.
		if (key == KeyCode.None)
			return;

		KeyShortcut shortcut = new KeyShortcut(shift: shift, alt: alt, control: ctrl, key: key);
		WindowHandler.OnKeyUp(shortcut);
	}
}
