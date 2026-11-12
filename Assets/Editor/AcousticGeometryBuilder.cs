using Game;
using Game.Audio;

using SteamAudio;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

using Scene = UnityEngine.SceneManagement.Scene;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Builds the acoustic geometry of the whole map for Steam Audio from chipotle.xml and exports it.
/// </summary>
public static class AcousticGeometryBuilder
{
	private const string _mapPath = "Assets/Resources/Data/Map/chipotle.xml";
	private const string _dataFolder = "Assets/SteamAudioData";
	private const string _materialFolder = _dataFolder + "/Materials";
	private const string _doorFolder = _dataFolder + "/Doors";
	private const string _sceneDataPath = _dataFolder + "/SampleScene.asset";
	private const string _pathingDataPath = _dataFolder + "/Pathing.asset";

	/// <summary>
	/// Spacing of the pathing probes in rooms and in halls; Steam Audio keeps a path between every two probes, so halls get fewer.
	/// </summary>
	private const float _probeSpacing = 1;
	private const float _hallProbeSpacing = 2;
	private const float _hallArea = 200;

	private const float _probeHeight = 1.5f;

	/// <summary>
	/// Radius of the probes that the listener and the sources take paths from, in multiples of their spacing. Steam Audio weighs
	/// them by distance, so a probe coming within reach of a walking listener turns the path only slightly; it takes at most 8 of them.
	/// </summary>
	private const float _probeReach = 1.5f;

	/// <summary>
	/// Distance of the probe grid from the walls of a room.
	/// </summary>
	private const float _probeInset = .3f;

	/// <summary>
	/// Distance of the probes in front of a passage from its middle.
	/// </summary>
	private const float _passageProbeDistance = .5f;

	/// <summary>
	/// Longest straight segment of a path between two probes and longest path, in meters.
	/// </summary>
	private const float _probeVisibilityRange = 20;
	private const float _pathRange = 50;

	/// <summary>
	/// Size of one map tile; map coordinates are tile centers.
	/// </summary>
	private const float _tile = .1f;

	/// <summary>
	/// Thickness of floors and ceilings.
	/// </summary>
	private const float _slab = .2f;

	private const float _gardenWallHeight = 2.5f;
	private const float _facadeHeight = 5;
	private const float _doorHeight = 2.05f;
	private const float _gateHeight = 2.3f;
	private const float _openingHeight = 2.1f;

	/// <summary>
	/// Ceilings that differ from the map; the map says 4 m for the hall, a usual entrance hall is lower.
	/// </summary>
	private static readonly Dictionary<string, float> _ceilingOverrides = new() { ["hala w1"] = 2.6f };

	/// <summary>
	/// Localities marked as indoor in the map that are open to the sky.
	/// </summary>
	private static readonly HashSet<string> _openAirLocalities = new() { "balkon p1" };

	/// <summary>
	/// A rectangle on the ground; x and z are Unity axes, the map's y axis is Unity's z axis.
	/// </summary>
	private readonly struct Area
	{
		public readonly float MinX, MaxX, MinZ, MaxZ;

		public Area(float minX, float maxX, float minZ, float maxZ)
		{
			MinX = minX;
			MaxX = maxX;
			MinZ = minZ;
			MaxZ = maxZ;
		}

		public float Width => MaxX - MinX;
		public float Depth => MaxZ - MinZ;

		public Area Expand(float margin)
			=> new(MinX - margin, MaxX + margin, MinZ - margin, MaxZ + margin);

		public bool Overlaps(Area other)
			=> MinX < other.MaxX - .001f && MaxX > other.MinX + .001f && MinZ < other.MaxZ - .001f && MaxZ > other.MinZ + .001f;

		/// <summary>
		/// Returns a part of the area between two coordinates along x or z.
		/// </summary>
		public Area Slice(bool alongX, float from, float to)
			=> alongX ? new(from, to, MinZ, MaxZ) : new(MinX, MaxX, from, to);

		/// <summary>
		/// Returns a square area of the given size around the center of this area.
		/// </summary>
		public Area Square(float size)
		{
			float x = (MinX + MaxX) / 2, z = (MinZ + MaxZ) / 2;
			return new(x - size / 2, x + size / 2, z - size / 2, z + size / 2);
		}
	}

	private enum ShapeKind
	{
		Skip,
		Box,

		/// <summary>
		/// Windows, doors and pictures sitting in or on a wall; they stick out of it by 1 cm so the rays hit them and not the wall.
		/// </summary>
		OnWall,

		/// <summary>
		/// A trunk with a crown of foliage.
		/// </summary>
		Tree
	}

	/// <summary>
	/// Acoustic shape of objects whose type contains one of the keywords.
	/// </summary>
	private class ObjectShape
	{
		public readonly ShapeKind Kind;
		public readonly float Bottom, Top;
		public readonly string Material;
		public readonly string[] Keywords;

		public ObjectShape(ShapeKind kind, float bottom, float top, string material, params string[] keywords)
		{
			Kind = kind;
			Bottom = bottom;
			Top = top;
			Material = material;
			Keywords = keywords;
		}
	}

	/// <summary>
	/// Shapes of acoustic obstacles by their type in the map; the first matching rule wins. Things sound passes through are skipped.
	/// </summary>
	private static readonly ObjectShape[] _shapes =
	{
		new(ShapeKind.Skip, 0, 0, null, "plot drátěný", "houpačka", "zábradlí", "dálnice", "none"),

		new(ShapeKind.Box, -_slab, .005f, "Water", "bazén u"),

		new(ShapeKind.OnWall, 1.4f, 1.7f, "Wood", "věšák na klíče"),
		new(ShapeKind.Box, .6f, 1.8f, "Fabric", "věšák", "němý sluha"),

		new(ShapeKind.OnWall, .9f, 2.1f, "Glass", "okno"),
		new(ShapeKind.OnWall, .3f, 2.6f, "Glass", "výloha"),
		new(ShapeKind.OnWall, 1.3f, 1.8f, "Wood", "obraz"),
		new(ShapeKind.OnWall, 1.4f, 1.8f, "Wood", "fotka"),
		new(ShapeKind.OnWall, 1, 1.9f, "Glass", "zrcadlo"),
		new(ShapeKind.OnWall, 1.1f, 1.9f, "Wood", "nástěnka"),
		new(ShapeKind.OnWall, 1.6f, 2.1f, "Wood", "hodiny"),
		new(ShapeKind.OnWall, 1.2f, 1.5f, "Wood", "polička"),
		new(ShapeKind.OnWall, .6f, .75f, "Fabric", "toaleťák"),
		new(ShapeKind.OnWall, 1, 1.3f, "Metal", "sušák"),
		new(ShapeKind.OnWall, 0, 5, "Metal", "okap"),
		new(ShapeKind.OnWall, .5f, 1.1f, "Ceramic", "mušle"),
		new(ShapeKind.OnWall, 0, _doorHeight, "Wood", "dveře", "dvířka"),
		new(ShapeKind.OnWall, 0, _gateHeight, "Metal", "vrata"),
		new(ShapeKind.OnWall, 0, 1.8f, "Metal", "brána", "branka"),

		new(ShapeKind.Box, 0, 1.8f, "Foliage", "plot živý"),
		new(ShapeKind.Box, 0, 2, "Metal", "plot plechový"),
		new(ShapeKind.Box, 0, 1.6f, "Wood", "plot"),

		new(ShapeKind.Tree, 0, 0, null,
			"cedr", "smrk", "javor", "dub", "lípa", "buk", "kaštan", "borovice", "švestka", "bříza", "modřín", "jedle", "tis",
			"cypřiš", "tůje", "jalovec", "topol", "líska", "třešeň", "ořešák"),
		new(ShapeKind.Box, 0, 1, "Foliage", "fíkus", "fialka", "dracena", "datlovník", "agáve", "kopřivy", "růže", "truhlík"),

		new(ShapeKind.Box, .2f, .55f, "Upholstery", "postel"),
		new(ShapeKind.Box, .15f, .85f, "Upholstery", "pohovka", "sedačka", "křeslo"),
		new(ShapeKind.Box, .2f, .45f, "Fabric", "lehátko"),

		new(ShapeKind.Box, .65f, .85f, "Wood", "kulečník"),
		new(ShapeKind.Box, .7f, 1.2f, "Metal", "pinball"),
		new(ShapeKind.Box, 0, 1.1f, "Wood", "pult"),
		new(ShapeKind.Box, 0, .9f, "Wood", "ponk"),
		new(ShapeKind.Box, 0, .55f, "Wood", "stolek noční"),
		new(ShapeKind.Box, 0, 1.2f, "Foliage", "květinový stolek"),
		new(ShapeKind.Box, .4f, .45f, "Wood", "stolek"),
		new(ShapeKind.Box, .42f, .47f, "Wood", "stolička", "židle", "lavice", "lavička"),
		new(ShapeKind.Box, .72f, .77f, "Wood", "stůl"),

		new(ShapeKind.Box, 1.2f, 1.8f, "Wood", "skříňka"),
		new(ShapeKind.Box, 0, 2, "Wood", "skříň", "stěna obývací"),
		new(ShapeKind.Box, 0, 1.8f, "Glass", "vitrýna"),
		new(ShapeKind.Box, 0, 2, "Books", "regál"),
		new(ShapeKind.Box, 0, 1.8f, "Books", "police"),
		new(ShapeKind.Box, 0, 1, "Wood", "botník"),
		new(ShapeKind.Box, 0, .9f, "Wood", "linka"),
		new(ShapeKind.Box, 0, .9f, "Metal", "dřez"),
		new(ShapeKind.Box, 0, 1, "Metal", "trezor"),
		new(ShapeKind.Box, 1, 1.8f, "Metal", "rozvodna"),
		new(ShapeKind.Box, 0, .85f, "Metal", "minibar", "myčka", "pračka", "sporák"),
		new(ShapeKind.Box, 0, .9f, "Metal", "mrazák"),
		new(ShapeKind.Box, 0, 1.8f, "Metal", "lednice", "automat"),
		new(ShapeKind.Box, 0, 1.3f, "Wood", "klavír"),
		new(ShapeKind.Box, 0, 1.3f, "Glass", "akvárko"),
		new(ShapeKind.Box, 0, 1.2f, "Brick", "krb"),
		new(ShapeKind.Box, 0, 2.2f, "Wood", "kadibudka"),
		new(ShapeKind.Box, 0, 2.2f, "Metal", "plechová bouda"),
		new(ShapeKind.Box, 0, .9f, "Wood", "bouda"),
		new(ShapeKind.Box, 0, .6f, "Wood", "bedna"),
		new(ShapeKind.Box, 0, 1, "Wood", "sud"),
		new(ShapeKind.Box, 0, 1, "Metal", "gril"),
		new(ShapeKind.Box, 0, 1.1f, "Metal", "popelnice"),
		new(ShapeKind.Box, 0, .6f, "Fabric", "koš na prádlo"),
		new(ShapeKind.Box, 0, .5f, "Metal", "koš"),
		new(ShapeKind.Box, .5f, 1.1f, "Metal", "televize"),
		new(ShapeKind.Box, .3f, .9f, "Metal", "hasičák"),
		new(ShapeKind.Box, 0, 1.4f, "Wood", "koště"),
		new(ShapeKind.Box, 0, .6f, "Ceramic", "váza"),

		new(ShapeKind.Box, 0, .55f, "Ceramic", "vana", "bazének"),
		new(ShapeKind.Box, .7f, .9f, "Ceramic", "umyvadlo"),
		new(ShapeKind.Box, 0, .4f, "Ceramic", "záchod", "mísa", "bidet"),
		new(ShapeKind.Box, 0, 2, "Glass", "sprchový kout", "sprcháč"),

		new(ShapeKind.Box, .4f, 2.2f, "Car", "náklaďák"),
		new(ShapeKind.Box, .4f, 1, "Wood", "valník"),
		new(ShapeKind.Box, .3f, 1.1f, "Metal", "motorka"),
		new(ShapeKind.Box, .3f, 1.45f, "Car", "auto", "volkswagen"),

		new(ShapeKind.Box, 0, 4, "Concrete", "sloup"),
		new(ShapeKind.Box, 0, 5, "Metal", "lampa"),
		new(ShapeKind.Box, 0, 2.5f, "Metal", "značka"),
		new(ShapeKind.Box, .9f, 1.3f, "Metal", "schránka"),
		new(ShapeKind.Box, 0, 1.5f, "Metal", "klouzačka"),
		new(ShapeKind.Box, 0, .6f, "Fabric", "pytel"),
		new(ShapeKind.Box, 0, .4f, "Fabric", "kufr"),
		new(ShapeKind.Box, 0, .4f, "Wood", "krabice", "krabiice"),
		new(ShapeKind.Box, 0, .3f, "Concrete", "tvárnice"),
		new(ShapeKind.Box, 0, .5f, "Wood", "kláda"),
		new(ShapeKind.Box, .3f, .9f, "Metal", "vozík"),
		new(ShapeKind.Box, .3f, .9f, "Fabric", "kočárek"),
	};

	/// <summary>
	/// Shape of obstacles that no rule describes.
	/// </summary>
	private static readonly ObjectShape _defaultShape = new(ShapeKind.Box, 0, .6f, "Wood");

	private static Dictionary<string, SteamAudioMaterial> _materials;

	/// <summary>
	/// Object types that fell back to the default shape.
	/// </summary>
	private static SortedSet<string> _unknownTypes;

	private static int _boxCount;

	private static void CreateMaterials()
	{
		// Absorption low/mid/high, scattering, transmission low/mid/high.
		// Indoors scattering is at least 0.4: the boxes lack frames, mouldings and clutter, and Steam Audio renders scattered energy
		// as a smooth tail, while mostly specular surfaces give a sparse, crackling one.
		_materials = new()
		{
			// Bare plaster absorbs .12/.06/.04; the higher values stand for what the boxes lack in a lived-in room
			// (door frames, skirting, pictures, radiators, furniture, leaking doors), otherwise a tiled corridor reverberates almost 3 s.
			["Plaster"] = GetMaterial("Plaster", .14f, .15f, .12f, .5f, .056f, .056f, .004f),
			["Brick"] = GetMaterial("Brick", .03f, .04f, .07f, .5f, .015f, .015f, .015f),
			["Concrete"] = GetMaterial("Concrete", .05f, .07f, .08f, .4f, .015f, .002f, .001f),
			["Ceramic"] = GetMaterial("Ceramic", .01f, .01f, .02f, .4f, .06f, .044f, .011f),
			["Glass"] = GetMaterial("Glass", .06f, .03f, .02f, .2f, .06f, .044f, .011f),
			["Wood"] = GetMaterial("Wood", .11f, .07f, .06f, .5f, .07f, .014f, .005f),
			// An interior door with its gaps insulates about 15/25/30 dB; Steam Audio uses the transmission as an amplitude gain.
			["Door"] = GetMaterial("Door", .14f, .1f, .08f, .5f, .178f, .056f, .032f),
			["Metal"] = GetMaterial("Metal", .2f, .07f, .06f, .3f, .2f, .025f, .01f),
			// Thin boards on a frame resonate and leak; the values also stand for the bed and other furniture the map leaves out.
			["Boards"] = GetMaterial("Boards", .3f, .22f, .18f, .5f, .2f, .06f, .03f),
			// Walls hidden behind heaps of bags, boxes and broken furniture.
			["Clutter"] = GetMaterial("Clutter", .22f, .3f, .3f, .7f, .056f, .056f, .004f),
			// Wood wool insulation boards, usual under the ceiling of a large garage.
			["CeilingPanels"] = GetMaterial("CeilingPanels", .25f, .55f, .65f, .6f, 0, 0, 0),
			// Thin resonating panels, glass, wheel arches and the gap under the body.
			["Car"] = GetMaterial("Car", .3f, .2f, .15f, .6f, .2f, .025f, .01f),
			["Carpet"] = GetMaterial("Carpet", .24f, .69f, .73f, .6f, .02f, .005f, .003f),
			["Linoleum"] = GetMaterial("Linoleum", .02f, .03f, .03f, .4f, .02f, .005f, .003f),
			["Parquet"] = GetMaterial("Parquet", .12f, .07f, .07f, .45f, .07f, .014f, .005f),
			["ConcreteFloor"] = GetMaterial("ConcreteFloor", .01f, .02f, .03f, .4f, 0, 0, 0),
			// The open ground has no tail to smooth, and scattered energy returns from all of it as a hiss after every step.
			["Paving"] = GetMaterial("Paving", .01f, .02f, .02f, .1f, 0, 0, 0),
			["ConcreteGround"] = GetMaterial("ConcreteGround", .01f, .02f, .03f, .1f, 0, 0, 0),
			["Asphalt"] = GetMaterial("Asphalt", .02f, .04f, .07f, .15f, 0, 0, 0),
			["Cobblestones"] = GetMaterial("Cobblestones", .02f, .04f, .06f, .3f, 0, 0, 0),
			["Decking"] = GetMaterial("Decking", .12f, .08f, .07f, .3f, 0, 0, 0),
			["Grass"] = GetMaterial("Grass", .2f, .6f, .8f, .6f, 0, 0, 0),
			["Soil"] = GetMaterial("Soil", .15f, .4f, .55f, .4f, 0, 0, 0),
			["Gravel"] = GetMaterial("Gravel", .25f, .6f, .65f, .7f, 0, 0, 0),
			["Water"] = GetMaterial("Water", .01f, .01f, .02f, .05f, 0, 0, 0),
			["Upholstery"] = GetMaterial("Upholstery", .35f, .6f, .65f, .6f, .1f, .05f, .02f),
			["Fabric"] = GetMaterial("Fabric", .15f, .45f, .6f, .7f, .3f, .2f, .1f),
			["Foliage"] = GetMaterial("Foliage", .1f, .25f, .4f, .9f, .8f, .6f, .4f),
			["Books"] = GetMaterial("Books", .2f, .25f, .3f, .7f, .02f, .01f, .005f),
		};
	}

	private static SteamAudioMaterial GetFloorMaterial(string terrain, bool indoor) => terrain switch
	{
		"asfalt" or "asphalt" => _materials["Asphalt"],
		"tiles" => _materials[indoor ? "Ceramic" : "Paving"],
		"carpet" => _materials["Carpet"],
		"linoleum" => _materials["Linoleum"],
		"wood" => _materials[indoor ? "Parquet" : "Decking"],
		"grass" or "bush" => _materials["Grass"],
		"gravel" => _materials["Gravel"],
		"clay" or "mud" => _materials["Soil"],
		"puddle" => _materials["Water"],
		"cobblestones" => _materials["Cobblestones"],
		_ => _materials[indoor ? "ConcreteFloor" : "ConcreteGround"]
	};

	/// <summary>
	/// Localities with a concrete floor whose walls and ceiling are plastered.
	/// </summary>
	private static readonly HashSet<string> _plasteredLocalities = new() { "garáž v1", "garáž w1", "garáž p1", "sklep w1" };

	/// <summary>
	/// Localities full of junk that the map does not list as obstacles.
	/// </summary>
	private static readonly HashSet<string> _clutteredLocalities = new() { "garáž s1", "hala s1" };

	/// <summary>
	/// Localities with sound absorbing boards under the ceiling.
	/// </summary>
	private static readonly HashSet<string> _paneledCeilings = new() { "garáž v1" };

	/// <summary>
	/// Bathrooms and toilets are tiled, garages and the cellar are bare concrete, the cottage is wooden.
	/// </summary>
	private static SteamAudioMaterial GetWallMaterial(string locality, string terrain)
	{
		if (_clutteredLocalities.Contains(locality))
			return _materials["Clutter"];
		if (_plasteredLocalities.Contains(locality))
			return _materials["Plaster"];
		if (locality.Contains("koupelna") || locality.Contains("záchod"))
			return _materials["Ceramic"];
		if (terrain == "concrete")
			return _materials["Concrete"];
		if (terrain == "wood")
			return _materials["Boards"];
		return _materials["Plaster"];
	}

	[MenuItem("Tools/Postav akustickou geometrii")]
	public static void Build()
	{
		if (EditorApplication.isPlaying)
		{
			Log("Akustickou geometrii nejde postavit, když běží hra");
			return;
		}

		CreateMaterials();
		_unknownTypes = new();
		_boxCount = 0;
		_probeRooms = new();
		RemovePreviousGeometry();

		XDocument document = XDocument.Load(_mapPath);
		List<XElement> passages = document.Root.Element("passages").Elements("passage").ToList();

		Transform map = new GameObject(_mapObjectName).transform;
		foreach (XElement locality in document.Root.Element("localities").Elements("locality"))
			CreateLocality(locality, passages, map);

		CreateDoors(passages);

		// With the data asset assigned in advance Steam Audio exports without asking for a file.
		SteamAudioStaticMesh staticMesh = new GameObject(_staticMeshObjectName).AddComponent<SteamAudioStaticMesh>();
		staticMesh.asset = GetSerializedData(_sceneDataPath);
		staticMesh.sceneNameWhenExported = SceneManager.GetActiveScene().name;
		AssetDatabase.SaveAssets();

		Export();
		if (_unknownTypes.Count > 0)
			Debug.Log("Typy překážek bez pravidla: " + string.Join(", ", _unknownTypes));
		Log($"Akustická geometrie postavena, {_boxCount} kvádrů");

		BakePathing(staticMesh.asset, passages);
	}

	private const string _mapObjectName = "Map acoustics";
	private const string _staticMeshObjectName = "Steam Audio Static Mesh";

	private static void RemovePreviousGeometry()
	{
		foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
		{
			if (obj.name is _mapObjectName or _staticMeshObjectName or AcousticDoors.ObjectName or SpatialAudio.ProbeBatchName)
				UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	/// <summary>
	/// Indoor localities with their height; the pathing probes cover the buildings.
	/// </summary>
	private static List<(Area area, float height)> _probeRooms;

	/// <summary>
	/// Places probes in the buildings and bakes the shortest paths of sound between them, so Steam Audio lets the sound
	/// of a hidden source bend around door frames and corners. Doors are dynamic objects missing in the bake;
	/// Steam Audio checks the paths against the closed ones while the game runs and looks for others.
	/// </summary>
	private static void BakePathing(SerializedData sceneData, List<XElement> passages)
	{
		SetPathingSettings();

		SteamAudioProbeBatch batch = new GameObject(SpatialAudio.ProbeBatchName).AddComponent<SteamAudioProbeBatch>();
		batch.asset = GetSerializedData(_pathingDataPath);
		batch.heightAboveFloor = _probeHeight;
		int count = GenerateProbes(batch, sceneData, GetPassageProbes(passages));
		SaveScene();

		Log($"{count} sond, peču cesty zvuku");
		SteamAudioProbeBatch.BeginBake(new[] { batch });
		EditorApplication.update += WaitForPathing;
	}

	/// <summary>
	/// The settings lie in the plugin folder, which is not in the repository. A path between two probes needs a clear line
	/// between their centers, so it cannot cut through the edge of a wall.
	/// </summary>
	private static void SetPathingSettings()
	{
		SteamAudioSettings settings = SteamAudioSettings.Singleton;
		settings.bakingVisibilityRadius = 0;
		settings.bakingVisibilityRange = _probeVisibilityRange;
		settings.bakingPathRange = _pathRange;
		EditorUtility.SetDirty(settings);
		AssetDatabase.SaveAssets();
	}

	/// <summary>
	/// A probe in the middle of each passage of a building and one on each side of it, so every doorway joins the rooms it connects.
	/// </summary>
	private static List<Vector3> GetPassageProbes(List<XElement> passages)
	{
		List<Vector3> probes = new();
		foreach (XElement passage in passages)
		{
			Area area = ToArea(ParseNumbers(Attr(passage, "coordinates")));
			if (!_probeRooms.Any(r => r.area.Overlaps(area)))
				continue;

			Vector3 center = new((area.MinX + area.MaxX) / 2, Mathf.Min(_probeHeight, GetPassageHeight(passage) - .3f), (area.MinZ + area.MaxZ) / 2);
			Vector3 normal = area.Width < area.Depth ? Vector3.right : Vector3.forward;
			probes.Add(center);
			probes.Add(center + _passageProbeDistance * normal);
			probes.Add(center - _passageProbeDistance * normal);
		}
		return probes;
	}

	/// <summary>
	/// Places a grid of probes above the floor of each room and the given single probes, and saves them to the probe batch.
	/// </summary>
	/// <returns>Number of probes</returns>
	private static int GenerateProbes(SteamAudioProbeBatch batch, SerializedData sceneData, List<Vector3> singleProbes)
	{
		SteamAudioManager.Initialize(ManagerInitReason.GeneratingProbes);
		SteamAudioManager.LoadScene(SceneManager.GetActiveScene(), SteamAudioManager.Context, false);
		SteamAudio.Scene scene = SteamAudioManager.CurrentScene;
		StaticMesh staticMesh = new(SteamAudioManager.Context, scene, sceneData);
		ProbeBatch probes = new(SteamAudioManager.Context);
		Transform box = new GameObject("Probe box").transform;
		try
		{
			staticMesh.AddToScene(scene);
			scene.Commit();

			List<Sphere> spheres = new();
			foreach (var (area, height) in _probeRooms)
			{
				Area grid = area.Expand(-_probeInset);
				float top = height - .05f;
				box.position = new Vector3((grid.MinX + grid.MaxX) / 2, top / 2, (grid.MinZ + grid.MaxZ) / 2);
				box.localScale = new Vector3(grid.Width, top, grid.Depth);

				ProbeArray array = new(SteamAudioManager.Context);
				array.GenerateProbes(scene, new ProbeGenerationParams
				{
					type = ProbeGenerationType.UniformFloor,
					spacing = area.Width * area.Depth > _hallArea ? _hallProbeSpacing : _probeSpacing,
					height = _probeHeight,
					transform = Common.ConvertTransform(box)
				});
				for (int i = 0; i < array.GetNumProbes(); i++)
				{
					Sphere sphere = array.GetProbe(i);
					sphere.radius *= _probeReach;
					probes.AddProbe(sphere);
					spheres.Add(sphere);
				}
				array.Release();
			}

			foreach (Vector3 point in singleProbes)
			{
				Sphere sphere = new() { center = Common.ConvertVector(point), radius = _probeSpacing * _probeReach };
				probes.AddProbe(sphere);
				spheres.Add(sphere);
			}

			batch.probeDataSize = probes.Save(batch.asset);
			typeof(SteamAudioProbeBatch).GetField("mProbeSpheres", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(batch, spheres.ToArray());
			batch.ResetLayers();
			EditorUtility.SetDirty(batch);
			return spheres.Count;
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(box.gameObject);
			probes.Release();
			staticMesh.Release();
			SteamAudioManager.ShutDown();
			UnityEngine.Object.DestroyImmediate(SteamAudioManager.Singleton.gameObject);
		}
	}

	private static void WaitForPathing()
	{
		if (Baker.IsBakeActive())
			return;

		EditorApplication.update -= WaitForPathing;
		SaveScene();
		SteamAudioProbeBatch batch = UnityEngine.Object.FindAnyObjectByType<SteamAudioProbeBatch>();
		Log(batch != null && batch.GetNumLayers() > 0 ? "Cesty zvuku upečeny" : "Cesty zvuku se nepodařilo upéct");
	}

	/// <summary>
	/// Indoor localities get a floor, a ceiling and walls with openings for passages; outdoor ones only the ground, so the sound escapes to the sky.
	/// </summary>
	private static void CreateLocality(XElement locality, List<XElement> passages, Transform map)
	{
		string name = Attr(locality, "indexedname");
		string terrain = Attr(locality, "defaultTerrain").ToLowerInvariant();
		float[] coordinates = ParseNumbers(Attr(locality, "coordinates"));
		Vector2 upperLeft = new(coordinates[0], coordinates[1]);
		Area area = ToArea(coordinates);
		bool indoor = Attr(locality, "type").Equals("indoor", StringComparison.OrdinalIgnoreCase) && !_openAirLocalities.Contains(name);
		float height = _ceilingOverrides.TryGetValue(name, out float ceiling) ? ceiling : float.Parse(Attr(locality, "height"), CultureInfo.InvariantCulture);
		SteamAudioMaterial wallMaterial = GetWallMaterial(name, terrain);

		Transform parent = new GameObject(name).transform;
		parent.SetParent(map, false);
		if (Attr(locality, "type").Equals("indoor", StringComparison.OrdinalIgnoreCase))
			_probeRooms.Add((area, height));

		CreateBox("Floor", area, -_slab, 0, GetFloorMaterial(terrain, indoor), parent);
		CreatePanels(locality.Elements("panel"), upperLeft, indoor, height, parent);

		if (indoor)
		{
			SteamAudioMaterial ceilingMaterial = wallMaterial == _materials["Ceramic"] ? _materials["Plaster"] : wallMaterial;
			if (_paneledCeilings.Contains(name))
				ceilingMaterial = _materials["CeilingPanels"];
			CreateBox("Ceiling", area, height, height + _slab, ceilingMaterial, parent);
			CreateWalls(area, height, wallMaterial, GetOpenings(area, passages), parent);
		}

		foreach (XElement obj in locality.Elements("object").Where(o => IsTrue(Attr(o, "acousticObstacle"))))
			CreateObject(obj, upperLeft, indoor ? height : float.PositiveInfinity, indoor ? wallMaterial : _materials["Brick"], parent);
	}

	/// <summary>
	/// Builds the terrain panels as the game sees them: it draws them on a grid of tiles in the order of the map,
	/// so a later panel replaces the tiles of an earlier one, e.g. a walkable path across a wall.
	/// </summary>
	private static void CreatePanels(IEnumerable<XElement> panels, Vector2 upperLeft, bool indoor, float height, Transform parent)
	{
		Dictionary<(int x, int z), (string terrain, bool walkable)> tiles = new();
		foreach (XElement panel in panels)
		{
			float[] c = ParseNumbers(Attr(panel, "coordinates"));
			var terrain = (Attr(panel, "terrain").ToLowerInvariant(), IsTrue(Attr(panel, "canBeOccupied")));
			int minX = ToTile(upperLeft.x + Mathf.Min(c[0], c[2])), maxX = ToTile(upperLeft.x + Mathf.Max(c[0], c[2]));
			int minZ = ToTile(upperLeft.y - Mathf.Max(c[1], c[3])), maxZ = ToTile(upperLeft.y - Mathf.Min(c[1], c[3]));
			for (int x = minX; x <= maxX; x++)
			{
				for (int z = minZ; z <= maxZ; z++)
					tiles[(x, z)] = terrain;
			}
		}

		foreach (var (area, terrain) in MergeTiles(tiles))
			CreatePanel(terrain.terrain, terrain.walkable, area, indoor, height, parent);
	}

	private static int ToTile(float coordinate) => Mathf.RoundToInt(coordinate / _tile);

	/// <summary>
	/// Joins neighbouring tiles of the same kind into as few rectangles as possible.
	/// </summary>
	private static IEnumerable<(Area area, T value)> MergeTiles<T>(Dictionary<(int x, int z), T> tiles)
	{
		HashSet<(int x, int z)> used = new();
		foreach ((int x, int z) tile in tiles.Keys.OrderBy(t => t.z).ThenBy(t => t.x).ToList())
		{
			if (used.Contains(tile))
				continue;

			T value = tiles[tile];
			bool Same((int x, int z) t) => !used.Contains(t) && tiles.TryGetValue(t, out T other) && EqualityComparer<T>.Default.Equals(other, value);

			int maxX = tile.x;
			while (Same((maxX + 1, tile.z)))
				maxX++;

			int maxZ = tile.z;
			while (Enumerable.Range(tile.x, maxX - tile.x + 1).All(x => Same((x, maxZ + 1))))
				maxZ++;

			for (int x = tile.x; x <= maxX; x++)
			{
				for (int z = tile.z; z <= maxZ; z++)
					used.Add((x, z));
			}

			yield return (new Area((tile.x - .5f) * _tile, (maxX + .5f) * _tile, (tile.z - .5f) * _tile, (maxZ + .5f) * _tile), value);
		}
	}

	/// <summary>
	/// Creates a terrain panel: a different floor, a wall, a hedge or a low concrete block.
	/// </summary>
	private static void CreatePanel(string terrain, bool walkable, Area area, bool indoor, float height, Transform parent)
	{
		if (terrain == "wall")
			CreateBox("Wall", area, 0, indoor ? height : walkable ? .5f : _facadeHeight, indoor ? _materials["Plaster"] : _materials["Brick"], parent);
		else if (terrain == "bush")
			CreateBox("Bush", area, 0, walkable ? .6f : 1.8f, _materials["Foliage"], parent);
		else if (terrain == "concrete" && !walkable)
			CreateBox("Concrete block", area, 0, 1, _materials["Concrete"], parent);
		else // A floor 5 mm above the floor of the locality, so the rays hit it first.
			CreateBox("Terrain " + terrain, area, -_slab, .005f, GetFloorMaterial(terrain, indoor), parent);
	}

	/// <summary>
	/// Returns the passages touching the locality with the height of their opening.
	/// </summary>
	private static List<(Area area, float top)> GetOpenings(Area locality, List<XElement> passages)
	{
		List<(Area, float)> openings = new();
		foreach (XElement passage in passages)
		{
			Area area = ToArea(ParseNumbers(Attr(passage, "coordinates")));
			if (area.Overlaps(locality))
				openings.Add((area, GetPassageHeight(passage)));
		}
		return openings;
	}

	private static float GetPassageHeight(XElement passage)
	{
		if (!IsTrue(Attr(passage, "door")))
			return _openingHeight;
		return IsGate(passage) ? _gateHeight : _doorHeight;
	}

	private static bool IsGate(XElement passage)
		=> Attr(passage, "type").Equals("gate", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Builds the walls on the edge tiles of the locality, where the map has its walls.
	/// </summary>
	private static void CreateWalls(Area room, float height, SteamAudioMaterial material, List<(Area area, float top)> openings, Transform parent)
	{
		CreateWall("North wall", new(room.MinX, room.MaxX, room.MaxZ - _tile, room.MaxZ), true, height, material, openings, parent);
		CreateWall("South wall", new(room.MinX, room.MaxX, room.MinZ, room.MinZ + _tile), true, height, material, openings, parent);
		CreateWall("West wall", new(room.MinX, room.MinX + _tile, room.MinZ, room.MaxZ), false, height, material, openings, parent);
		CreateWall("East wall", new(room.MaxX - _tile, room.MaxX, room.MinZ, room.MaxZ), false, height, material, openings, parent);
	}

	/// <summary>
	/// Splits a wall into solid parts and lintels above the openings.
	/// </summary>
	private static void CreateWall(string name, Area strip, bool alongX, float height, SteamAudioMaterial material, List<(Area area, float top)> openings, Transform parent)
	{
		float start = alongX ? strip.MinX : strip.MinZ;
		float end = alongX ? strip.MaxX : strip.MaxZ;
		var gaps = openings
			.Where(o => o.area.Overlaps(strip))
			.Select(o => (from: Mathf.Max(alongX ? o.area.MinX : o.area.MinZ, start), to: Mathf.Min(alongX ? o.area.MaxX : o.area.MaxZ, end), o.top))
			.OrderBy(g => g.from);

		float position = start;
		foreach (var gap in gaps)
		{
			if (gap.from > position)
				CreateBox(name, strip.Slice(alongX, position, gap.from), 0, height, material, parent);
			if (gap.top < height)
				CreateBox(name + " lintel", strip.Slice(alongX, gap.from, gap.to), gap.top, height, material, parent);
			position = Mathf.Max(position, gap.to);
		}

		if (position < end)
			CreateBox(name, strip.Slice(alongX, position, end), 0, height, material, parent);
	}

	private static void CreateObject(XElement obj, Vector2 upperLeft, float ceiling, SteamAudioMaterial wallMaterial, Transform parent)
	{
		string type = Attr(obj, "type").Trim().ToLowerInvariant();
		string name = Attr(obj, "indexedname");
		Area area = ToAbsoluteArea(ParseNumbers(Attr(obj, "coordinates")), upperLeft);

		if (type == "zeď")
		{
			CreateBox(name, area, 0, float.IsInfinity(ceiling) ? _gardenWallHeight : ceiling, wallMaterial, parent);
			return;
		}

		ObjectShape shape = _shapes.FirstOrDefault(s => s.Keywords.Any(type.Contains));
		if (shape == null)
		{
			_unknownTypes.Add(type);
			shape = _defaultShape;
		}

		if (shape.Kind == ShapeKind.Skip)
			return;

		// A solid box standing on the ground, higher than the listener, so the steps and other sounds clearly reflect off it.
		float top = float.IsInfinity(ceiling) ? _outdoorObstacleHeight : Mathf.Min(Settings.ListenerHeight + _indoorObstacleClearance, ceiling - .01f);
		CreateBox(name, area, 0, top, _materials[shape.Material ?? _defaultShape.Material], parent);
	}

	/// <summary>
	/// Height of the acoustic obstacles outdoors and how far they reach above the listener indoors, in meters.
	/// </summary>
	private const float _outdoorObstacleHeight = 7, _indoorObstacleClearance = .5f;

	/// <summary>
	/// Doors are dynamic objects, so the game can remove them when they open.
	/// </summary>
	private static void CreateDoors(List<XElement> passages)
	{
		EnsureFolder(_doorFolder);

		GameObject root = new(AcousticDoors.ObjectName);
		root.AddComponent<AcousticDoors>();

		int index = 0;
		foreach (XElement passage in passages.Where(p => IsTrue(Attr(p, "door"))))
		{
			bool gate = IsGate(passage);
			Area area = ToArea(ParseNumbers(Attr(passage, "coordinates")));
			string name = Attr(passage, "indexedname").Sanitize();
			GameObject door = CreateBox(name, area, 0, gate ? _gateHeight : _doorHeight, gate ? _materials["Metal"] : _materials["Door"], root.transform);
			door.AddComponent<SteamAudioDynamicObject>().asset = GetSerializedData($"{_doorFolder}/Door{index++}.asset");
		}
	}

	private static void Export()
	{
		SteamAudioManager.ExportActiveScene();
		foreach (SteamAudioDynamicObject door in UnityEngine.Object.FindObjectsByType<SteamAudioDynamicObject>(FindObjectsSortMode.None))
			SteamAudioManager.ExportDynamicObject(door, false);
		SaveScene();
	}

	private static void SaveScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		EditorSceneManager.MarkSceneDirty(scene);
		EditorSceneManager.SaveScene(scene);
	}

	private static SerializedData GetSerializedData(string path)
	{
		SerializedData data = AssetDatabase.LoadAssetAtPath<SerializedData>(path);
		if (data == null)
		{
			data = ScriptableObject.CreateInstance<SerializedData>();
			AssetDatabase.CreateAsset(data, path);
		}
		return data;
	}

	/// <summary>
	/// Map coordinates are tile centers, so a rectangle reaches half a tile beyond them.
	/// </summary>
	private static Area ToArea(float[] c)
		=> new(Mathf.Min(c[0], c[2]) - _tile / 2, Mathf.Max(c[0], c[2]) + _tile / 2, Mathf.Min(c[1], c[3]) - _tile / 2, Mathf.Max(c[1], c[3]) + _tile / 2);

	/// <summary>
	/// Converts coordinates relative to a locality (x to the east, y to the south from its upper left corner) like the game does.
	/// </summary>
	private static Area ToAbsoluteArea(float[] c, Vector2 upperLeft)
		=> ToArea(new[] { upperLeft.x + c[0], upperLeft.y - c[1], upperLeft.x + c[2], upperLeft.y - c[3] });

	private static float[] ParseNumbers(string text)
		=> text.Split(',').Select(n => float.Parse(n.Trim(), CultureInfo.InvariantCulture)).ToArray();

	private static string Attr(XElement element, string name)
		=> element.Attribute(name)?.Value.Trim() ?? string.Empty;

	private static bool IsTrue(string value)
		=> value.Equals("true", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Creates a box with Steam Audio geometry.
	/// </summary>
	/// <param name="name">Inner name of the object</param>
	/// <param name="area">Ground plan of the box</param>
	/// <param name="bottom">Height of the bottom in meters</param>
	/// <param name="top">Height of the top in meters</param>
	/// <param name="material">Acoustic material of the box</param>
	/// <param name="parent">Parent object</param>
	/// <remarks>The box keeps its collider, so the game can keep sound sources out of walls and doors.</remarks>
	private static GameObject CreateBox(string name, Area area, float bottom, float top, SteamAudioMaterial material, Transform parent)
	{
		GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
		box.layer = SpatialAudio.GeometryLayer;
		box.name = name;
		box.transform.SetParent(parent, false);
		box.transform.position = new Vector3((area.MinX + area.MaxX) / 2, (bottom + top) / 2, (area.MinZ + area.MaxZ) / 2);
		box.transform.localScale = new Vector3(area.Width, top - bottom, area.Depth);
		box.AddComponent<SteamAudioGeometry>().material = material;
		_boxCount++;
		return box;
	}

	/// <summary>
	/// Loads a Steam Audio material from the project or creates it, and sets its acoustic properties.
	/// </summary>
	/// <remarks>The material is stored as an asset so the scene keeps the reference after reloading.</remarks>
	private static SteamAudioMaterial GetMaterial(string name, float lowFreqAbsorption, float midFreqAbsorption, float highFreqAbsorption, float scattering, float lowFreqTransmission, float midFreqTransmission, float highFreqTransmission)
	{
		EnsureFolder(_materialFolder);

		string path = $"{_materialFolder}/{name}.asset";
		SteamAudioMaterial material = AssetDatabase.LoadAssetAtPath<SteamAudioMaterial>(path);
		if (material == null)
		{
			material = ScriptableObject.CreateInstance<SteamAudioMaterial>();
			AssetDatabase.CreateAsset(material, path);
		}

		material.lowFreqAbsorption = lowFreqAbsorption;
		material.midFreqAbsorption = midFreqAbsorption;
		material.highFreqAbsorption = highFreqAbsorption;
		material.lowFreqTransmission = lowFreqTransmission;
		material.midFreqTransmission = midFreqTransmission;
		material.highFreqTransmission = highFreqTransmission;
		material.scattering = scattering;
		EditorUtility.SetDirty(material);
		return material;
	}

	private static void EnsureFolder(string path)
	{
		if (AssetDatabase.IsValidFolder(path))
			return;

		int slash = path.LastIndexOf('/');
		string parent = path.Substring(0, slash);
		EnsureFolder(parent);
		AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
	}

	private static void Log(string message)
	{
		ScreenReader.Speak(message);
		FileLogger.LogMessage(message);
	}
}
