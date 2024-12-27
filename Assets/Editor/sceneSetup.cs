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

public class sceneSetup
{
	[MenuItem("Tools/Nastav na��t�n� audia na streamov�n�")]
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

				// Nastaven� Load Type na Streaming
				settings.loadType = AudioClipLoadType.Streaming;

				// Aktualizace nastaven�
				audioImporter.defaultSampleSettings = settings;

				// Vynucen� op�tovn�ho importu
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
		}
		Log("Na��t�n� zm�n�no na streamov�n�");
	}


	[MenuItem("Tools/Zkop�ruj obsah konzole %#E")]
	private static void CopyConsoleToClipboard()
	{
		var assembly = Assembly.GetAssembly(typeof(EditorWindow));
		var consoleWindowType = assembly.GetType("UnityEditor.ConsoleWindow");
		var field = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
		var consoleWindowInstance = field.GetValue(null);

		if (consoleWindowInstance == null || consoleWindowType != EditorWindow.focusedWindow.GetType())
		{
			Log("Nejsi v okn� konzole.");
			return;
		}

		var listViewField = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
		var listView = listViewField.GetValue(consoleWindowInstance);

		var totalRowsField = listView.GetType().GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);
		int totalRows = (int)totalRowsField.GetValue(listView);

		var logEntriesType = assembly.GetType("UnityEditor.LogEntries");
		var getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
		var getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);

		var entryType = assembly.GetType("UnityEditor.LogEntry");
		var entryInstance = System.Activator.CreateInstance(entryType);

		StringBuilder sb = new StringBuilder();
		int count = (int)getCountMethod.Invoke(null, null);

		for (int i = 0; i < count; i++)
		{
			getEntryMethod.Invoke(null, new object[] { i, entryInstance });
			var messageField = entryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
			string message = messageField.GetValue(entryInstance) as string;
			sb.AppendLine(message);
		}

		EditorGUIUtility.systemCopyBuffer = sb.ToString();
		Log("Text z konzole zkop�rov�n!");
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
			Log("Nepoda�ilo se na��st mapu.");
			return;
		}

		try
		{
			LoadLocalitiesAndItems(document.Root);
			Log("Lokace a objekty na�teny");
		}
		catch (Exception)
		{
			Log("Chyba p�i na��t�n� lokac� a objekt�");
			return;
		}

		try
		{
			LoadPassages(document.Root);
			Log("Pr�chody na�teny");
		}
		catch (Exception)
		{
			Log("Chyba p�i na��t�n� pr�chod�");
		}
	}


	private static void LoadPassages(XElement root)
	{
		PassageFactory.LoadPassages();
		XElement passagesNode = root.Element("passages");
		var passages = passagesNode.Elements("passage").ToList();
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
		var localities = root.Element("localities").Elements("locality").ToList();
		foreach (XElement localityNode in localities)
		{
			GameObject locality = CreateObject(Attribute(localityNode, "indexedname"), "Locality");
			locality.AddComponent<Locality>();
			LoadItems(localityNode);
		}
	}


	private static void LoadItems(XElement localityNode)
	{
		var items = localityNode.Elements("object").ToList();
		foreach (XElement item in items)
		{
			string type = Attribute(item, "type");
			GameObject obj = CreateObject(Attribute(item, "indexedname"), "Item");
			ItemFactory.AddComponent(obj, type);
		}
	}

	private static GameObject CreateObject(string name, string tag)
	{
		GameObject obj = new(name);
		obj.tag = tag;
		return obj;
	}

	private static string Attribute(XElement element, string attribute, bool prepareForIndexing = true)
	=> prepareForIndexing ? element.Attribute(attribute).Value.PrepareForIndexing() : element.Attribute(attribute).Value;



	private static bool _eventHandlerAssigned;





	private static void Log(string message)
	{
		TolkTolk.Speak(message);
		FileLogger.LogMessage(message);
	}

	static void HandleLog(string logString, string stackTrace, LogType type)
	{
		Log(logString);
	}


	/// <summary>
	/// Static constructor that is automatically called after the editor loads.
	/// </summary>
	[MenuItem("Tools/P�iprav sc�nu %#Q")]
	private static void Preparescene()
	{
		InitializeVoiceOutput();
		ValidateScene();
		DeleteObjects();
		LoadMap();
	}

	private static void ValidateScene()
	{
		var scene = SceneManager.GetActiveScene();
		if (!scene.IsValid())
			Log("Sc�na nebyla na�tena");
		else Log("Sc�na na�tena");
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




	private static Type FindMetaType(string typeName, bool inEditor = false)
	{
		string assemblyName = inEditor ? "Meta.XR.Audio.Editor" : "Meta.XR.Audio";
		Assembly assembly = Assembly.Load(assemblyName);

		if (assembly == null)
		{
			Log($"Assembly '{assemblyName}' nelze na��st.");
			return null;
		}

		// Z�sk�me v�echny typy v assembly, v�etn� neve�ejn�ch
		Type[] types = assembly.GetTypes();
		foreach (Type t in types)
		{
			if (t.Name == typeName)
				return t;
		}

		Log($"Typ {typeName} nebyl nalezen.");
		return null;
	}

	[MenuItem("Tools/Sma� v�echny objekty")]
	private static void DeleteObjects()
	{
		GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (GameObject obj in objects)
			UnityEngine.Object.DestroyImmediate(obj);  // Use DestroyImmediate because we are in the editor

		Log("Objekty smaz�ny");
	}


}
