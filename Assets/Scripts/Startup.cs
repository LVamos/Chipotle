using Game;
using Game.Audio;
using Game.UI;

using UnityEngine;

/// <summary>
/// This class takes care of the initialization of the game.
/// </summary>
public class Startup : MonoBehaviour
{
	private bool _hasFocus;

	public void Start()
	{
		Logger.LogInfo("Hra spuštěna");
		SetCamera(); Application.runInBackground = true;
		Time.fixedDeltaTime = 1f / 30f;
		Sounds.Initialize();
		MainMenu();
	}

	private static void MainMenu()
	{
		if (!Settings.MainMenuAtStartup)
			WindowHandler.StartGame();
		else
			WindowHandler.MainMenu();
	}

	private static void SetCamera()
	{
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
	}

	public void Update()
	{
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

	private void CheckApplicationState()
	{
		if (!_hasFocus)
			OnDeactivated();
		else
			OnActivated();
	}

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
}
