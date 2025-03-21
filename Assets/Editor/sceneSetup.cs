using Assets.Scripts;

using Game;
using Game.Entities.Items;
using Game.Terrain;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class sceneSetup
{
	static sceneSetup()
	{
		InitializeVoiceOutput();
	}

	[MenuItem("Tools/Nastav naèítání audia na streamování")]
	public static void SetLoadTypeToStreaming()
	{
		string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip");

		foreach (string guid in audioGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;

			if (audioImporter != null)
			{
				AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;

				// Nastavení Load Type na Streaming
				settings.loadType = AudioClipLoadType.Streaming;

				// Aktualizace nastavení
				audioImporter.defaultSampleSettings = settings;

				// Vynucení opìtovného importu
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
		}
		Log("Naèítání zmìnìno na streamování");
	}

	[MenuItem("Tools/Zkopíruj obsah konzole %#E")]
	private static void CopyConsoleToClipboard()
	{
		Assembly assembly = Assembly.GetAssembly(typeof(EditorWindow));
		Type consoleWindowType = assembly.GetType("UnityEditor.ConsoleWindow");
		FieldInfo field = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
		object consoleWindowInstance = field.GetValue(null);

		if (consoleWindowInstance == null || consoleWindowType != EditorWindow.focusedWindow.GetType())
		{
			Log("Nejsi v oknì konzole.");
			return;
		}

		FieldInfo listViewField = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
		object listView = listViewField.GetValue(consoleWindowInstance);

		FieldInfo totalRowsField = listView.GetType().GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);
		int totalRows = (int)totalRowsField.GetValue(listView);

		Type logEntriesType = assembly.GetType("UnityEditor.LogEntries");
		MethodInfo getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
		MethodInfo getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);

		Type entryType = assembly.GetType("UnityEditor.LogEntry");
		object entryInstance = System.Activator.CreateInstance(entryType);

		StringBuilder sb = new();
		int count = (int)getCountMethod.Invoke(null, null);

		for (int i = 0; i < count; i++)
		{
			getEntryMethod.Invoke(null, new object[] { i, entryInstance });
			FieldInfo messageField = entryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
			string message = messageField.GetValue(entryInstance) as string;
			sb.AppendLine(message);
		}

		EditorGUIUtility.systemCopyBuffer = sb.ToString();
		Log("Text z konzole zkopírován!");
	}

	public static void LoadMap()
	{
		XDocument document = null;
		try
		{
			string mapPath = Path.Combine(MainScript.MapPath, "chipotle.xml");
			document = XDocument.Load(mapPath);
		}
		catch (FileNotFoundException)
		{
			Log($"Soubor {MainScript.MapPath} nebyl nalezen.");
			return;
		}
		catch (Exception)
		{
			Log("Nepodaøilo se naèíst mapu.");
			return;
		}

		try
		{
			LoadLocalitiesAndItems(document.Root);
			Log("Lokace a objekty naèteny");
		}
		catch (Exception)
		{
			Log("Chyba pøi naèítání lokací a objektù");
			return;
		}

		try
		{
			LoadPassages(document.Root);
			Log("Prùchody naèteny");
		}
		catch (Exception)
		{
			Log("Chyba pøi naèítání prùchodù");
		}
	}

	private static void LoadPassages(XElement root)
	{
		PassageFactory.LoadPassages();
		XElement passagesNode = root.Element("passages");
		System.Collections.Generic.List<XElement> passages = passagesNode.Elements("passage").ToList();
		foreach (XElement passage in passages)
		{
			string name = Attribute(passage, "indexedname");
			GameObject obj = CreateObject(name, "Passage");
			bool isDoor = Attribute(passage, "door").ToBool();
			PassageFactory.AddComponent(obj, name, isDoor);
		}
	}

	private static void LoadLocalitiesAndItems(XElement root)
	{
		System.Collections.Generic.List<XElement> localities = root.Element("localities").Elements("locality").ToList();
		foreach (XElement localityNode in localities)
		{
			GameObject locality = CreateObject(Attribute(localityNode, "indexedname"), "Locality");
			locality.AddComponent<Locality>();
			LoadItems(localityNode);
		}
	}

	private static void LoadItems(XElement localityNode)
	{
		System.Collections.Generic.List<XElement> items = localityNode.Elements("object").ToList();
		foreach (XElement item in items)
		{
			string type = Attribute(item, "type");
			GameObject obj = CreateObject(Attribute(item, "indexedname"), "Item");
			ItemFactory.AddComponent(obj, type);
		}
	}

	private static GameObject CreateObject(string name, string tag)
	{
		GameObject obj = new(name)
		{
			tag = tag
		};
		return obj;
	}

	private static string Attribute(XElement element, string attribute, bool prepareForIndexing = true)
	{
		return prepareForIndexing ? element.Attribute(attribute).Value.PrepareForIndexing() : element.Attribute(attribute).Value;
	}

	private static bool _eventHandlerAssigned;

	private static void Log(string message)
	{
		TolkTolk.Speak(message);
		FileLogger.LogMessage(message);
	}

	private static void HandleLog(string logString, string stackTrace, LogType type)
	{
		// Prevent logging useless warnings related to Resonance Audio.
		const string resonanceWarning = "Make sure AudioSource is routed to a mixer that ResonanceAudioRenderer is attached to.";
		if (!string.Equals(resonanceWarning, logString, StringComparison.OrdinalIgnoreCase))
			Log(logString);
	}

	/// <summary>
	/// Static constructor that is automatically called after the editor loads.
	/// </summary>
	[MenuItem("Tools/Pøiprav scénu %#Q")]
	private static void Preparescene()
	{
		ValidateScene();
		DeleteObjects();
		LoadMap();
	}

	private static void ValidateScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		if (!scene.IsValid())
			Log("Scéna nebyla naètena");
		else
			Log("Scéna naètena");
	}

	private static void InitializeVoiceOutput()
	{
		TolkTolk.Load();

		if (!_eventHandlerAssigned)
		{
			Application.logMessageReceived += HandleLog;
			_eventHandlerAssigned = true;
		}
	}

	/// <summary>

	[MenuItem("Tools/Najdi objekty")]
	private static void GetObjects()
	{
		GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
		Log("Hotovo");
	}

	[MenuItem("Tools/Smaž všechny objekty")]
	private static void DeleteObjects()
	{
		GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (GameObject obj in objects)
			UnityEngine.Object.DestroyImmediate(obj);  // Use DestroyImmediate because we are in the editor

		Log("Objekty smazány");
	}
}
