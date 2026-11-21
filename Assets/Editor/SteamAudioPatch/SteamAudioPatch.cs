using System.IO;
using System.Linq;
using System.Text;

using UnityEditor;

using UnityEngine;

/// <summary>
/// Applies Chipotle's changes to the Steam Audio 4.8.1 scripts, which are not in git: after a clean clone with Steam Audio imported
/// from its package, and after Steam Audio is imported again. It is a separate assembly so it runs while the game does not compile yet.
/// </summary>
[InitializeOnLoad]
public class SteamAudioPatch : AssetPostprocessor
{
	private const string ManagerPath = "Assets/Plugins/SteamAudio/Scripts/Runtime/SteamAudioManager.cs";
	private const string AppliedMarker = "SimulationsStarted++; // Chipotle";

	static SteamAudioPatch() => EditorApplication.delayCall += Apply;

	private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
	{
		if (imported.Contains(ManagerPath))
			Apply();
	}

	private static void Apply()
	{
		if (!File.Exists(ManagerPath))
		{
			Debug.LogWarning("Steam Audio chybí: naimportuj balíček Steam Audio 4.8.1, úpravy Chipotle se pak doplní samy.");
			return;
		}

		byte[] bytes = File.ReadAllBytes(ManagerPath);
		bool bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
		string original = new UTF8Encoding(false).GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0));
		if (original.Contains(AppliedMarker))
			return;

		string text = original.Replace("\r\n", "\n");
		foreach (var (find, replace) in Edits)
		{
			string anchor = find.Replace("\r\n", "\n");
			int first = text.IndexOf(anchor);
			if (first < 0 || text.IndexOf(anchor, first + 1) >= 0)
			{
				Debug.LogError("Úpravy Chipotle do Steam Audia nejdou použít, " + ManagerPath + " se změnil (jiná verze než 4.8.1?). Soubor zůstal beze změny.");
				return;
			}
			text = text.Replace(anchor, replace.Replace("\r\n", "\n"));
		}

		if (original.Contains("\r\n"))
			text = text.Replace("\n", "\r\n");
		File.WriteAllText(ManagerPath, text, new UTF8Encoding(bom));
		AssetDatabase.ImportAsset(ManagerPath);
		Debug.Log("Úpravy Chipotle doplněny do " + ManagerPath);
	}

	private static readonly (string Find, string Replace)[] Edits =
	{
			(@"                    {
                        listener.UpdateOutputs(SimulationFlags.Reflections | SimulationFlags.Pathing);
                    }
",
			 @"                    {
                        listener.UpdateOutputs(SimulationFlags.Reflections | SimulationFlags.Pathing);
                    }

                    // Chipotle: lets the game start sounds right after their reflections were simulated and freeze them
                    // before the next simulation reads the inputs.
                    SimulationsApplied++;
                    SimulationApplied?.Invoke();
                }

                // Chipotle: the simulation thread may have finished only after the commit at the top of this method;
                // a door opened or closed in this frame must be part of the simulation that starts now.
                if (mSceneCommitRequired)
                {
                    mCurrentScene.Commit();
                    mSceneCommitRequired = false;
                    mSimulator.SetScene(mCurrentScene);
                    mSimulator.Commit();
"),
			(@"                {
                    mSimulationThreadWaitHandle.Set();
                }
",
			 @"                {
                    mSimulationThreadWaitHandle.Set();
                }

                SimulationsStarted++; // Chipotle
"),
			(@"            }
        }
#endif
",
			 @"            }
        }
#endif

        // Chipotle: number of reflection simulations started and of those whose results were applied to the sources.
        public static int SimulationsStarted { get; private set; }
        public static int SimulationsApplied { get; private set; }

        // Chipotle: raised on the main thread when results of a simulation were applied, before the next one reads its inputs.
        public static event Action SimulationApplied;
"),
	};
}
