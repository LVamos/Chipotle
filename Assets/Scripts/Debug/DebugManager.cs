using Assets.Scripts.Models;

using DavyKager;

using game.debug;

using Game.Audio;
using Game.Controls;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Debug.integration;
using Game.Entities.Characters;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.Movement;
using Game.Serialization;
using Game.Terrain;
using Game.Testing.Characters.Chipotle;
using Game.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Game.Debug
{
    public class DebugManager : VirtualWindow
    {
        public override void OnKeyPress(char letter)
        {
            if (_macroRecorder != null && _macroRecorder.IsRecording)
                _macroRecorder.FeedKeyPress(letter);
        }

        Vector2? _lastPlayerPosition;

        private void Update()
        {
            if (!Settings.TestCommandsEnabled || Player == null || Player.Area == null)
                return;

            if (_lastPlayerPosition != Player.Center)
            {
                _lastPlayerPosition = Player.Center;
                if (DebugPointManager.GetPoints().TryGetValue(Player.Center, out string name))
                    Tolk.Speak(name, true);
            }
        }

        [DebugCommand(DebugCommand.SetWallMaterials)]
        private void SetWallMaterials()
        {
            string input = WindowHandler.InputBox("Zadej číslo materiálu", "Materiály zdí");
            int index = int.Parse(input);
            ZoneMaterial material = (ZoneMaterial)index;
            ZoneMaterials materials = new(material, material, material, material, material, material);
            Zone zone = World.Player.Zone;
            Sounds.RoomManager.SetRoomParameters
                (
                zone.transform.position,
                zone.transform.localScale,
                materials,
                zone.Type == ZoneType.Outdoor
                );

            string name = Enum.GetName(typeof(ZoneMaterial), material);
            Tolk.Speak(name);
        }

        [DebugCommand(DebugCommand.SayPlayerOrientation)]
        private void SayPlayerOrientation()
        {
            string message = $"Orientace hráče: {Player.Orientation.Angle.CartesianDegrees}; {Player.gameObject.transform.rotation.eulerAngles}";
            Tolk.Speak(message, true);
            GUIUtility.systemCopyBuffer = message;
        }

        [DebugCommand(DebugCommand.SayCameraInfo)]
        private void SayCameraInfo()
        {
            string message = $"Orientace kamery: {Camera.main.transform.rotation.eulerAngles}{Environment.NewLine}Pozice: {Camera.main.transform.position}";
            Tolk.Speak(message, true);
            GUIUtility.systemCopyBuffer = message;
        }

        [DebugCommand(DebugCommand.JumpToDebugPoint)]
        private void JumpToDebugPoint()
        {
            Dictionary<Vector2, string> points = DebugPointManager.GetPoints();
            if (points == null || points.Count == 0)
            {
                Tolk.Speak("Žádné uložené body", true);
                return;
            }

            List<List<string>> items = points.Values.Select(p => new List<string> { p }).ToList();

            MenuParameters parameters = new(
                items: items,
                introText: "Vyber bod",
                wrappingAllowed: false,
                menuClosed: (index) =>
                {
                    if (index == -1) return;
                    Vector2 selected = points.Keys.ElementAt(index);
                    GoToCoords(selected);
                }
            );
            WindowHandler.Menu(parameters);
        }

        [DebugCommand(DebugCommand.TestTuttleCollisions)]
        private void TestTuttleCollisions()
        {
            GameObject player = World.Player.gameObject;
            var tester = player.GetComponent<ChipotleTuttleCollisionTester>();
            if (tester != null)
            {
                Tolk.Speak("Vypnuto");
                Destroy(tester);
                return;
            }

            Tolk.Speak("Strkám do Tuttla");
            tester = player.AddComponent<ChipotleTuttleCollisionTester>();
            tester.Initialize();
            tester.Activate();
        }

        [DebugCommand(DebugCommand.OpenTuttleInEditor)]
        private void OpenTuttleInEditor()
        {
            _pipeServer.SendJumpToCoordinates(Tuttle.Center);
        }

        [DebugCommand(DebugCommand.MoveTuttleToClipboardCoords)]
        public void MoveTuttleToClipboardCoords()
        {
            try
            {
                MoveTuttleToCoords(GUIUtility.systemCopyBuffer.ToVector2());
            }
            catch (Exception) { }
        }

        public void MoveTuttleToCoords(Vector2 coords)
        {
            SetPosition message = new(this, coords);
            Tuttle.TakeMessage(message);
        }

        private void CreatePipeServer()
        {
            GameObject obj = new(nameof(MapEditorPipeServer));
            _pipeServer = obj.AddComponent<MapEditorPipeServer>();
            _pipeServer.StartServer();
        }

        MapEditorPipeServer _pipeServer;

        [DebugCommand(DebugCommand.OpenEditorOnPoint)]
        private void OpenEditorOnPoint() => _pipeServer.SendJumpToCoordinates(Player.Center);

        [DebugCommand(DebugCommand.PlaceDebugPoint)]
        public void PlaceDebugPoint() => DebugPointManager.PlaceDebugPoint();

        public static DebugManager CreateInstance()
        {
            GameObject obj = new(nameof(DebugManager));
            DebugManager manager = obj.AddComponent<DebugManager>();
            manager.Initialize();
            return manager;
        }

        /// <summary>
        /// Methods considered as macro-control (used to filter them out from recording)
        /// </summary>
        private HashSet<MethodInfo> _macroControlMethods;

        private MacroRecorder _macroRecorder;
        private MacroPlayer _macroPlayer;

        private void BuildMacroControlSet()
        {
            _macroControlMethods.Clear();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            TryAddMacroMethod(flags, nameof(StartMacroRecording));
            TryAddMacroMethod(flags, nameof(StopMacroRecording));
            TryAddMacroMethod(flags, nameof(PlayMacroPrompt));
            TryAddMacroMethod(flags, nameof(PlayMacroByName));
        }

        private void InitMacroSupport()
        {
            _macroRecorder = gameObject.AddComponent<MacroRecorder>();
            _macroPlayer = gameObject.AddComponent<MacroPlayer>();
            _macroPlayer.Sender = this;

            _macroControlMethods = new HashSet<MethodInfo>();
            BuildMacroControlSet();
        }

        private bool IsMacroControlShortcut(KeyboardInput shortcut)
        {
            if (_keyboardCommands.TryGetValue(shortcut, out Action action) && action != null)
            {
                MethodInfo mi = action.Method;
                if (_macroControlMethods.Contains(mi))
                    return true;
            }
            return false;
        }

        [DebugCommand(DebugCommand.PlayMacroPrompt)]
        private void PlayMacroPrompt()
        {
            if (_macroRecorder != null && _macroRecorder.IsRecording)
                return;

            // English: Gather macro files from configured macro folder
            string folder = MainScript.MacroPath;
            if (!Directory.Exists(folder))
            {
                Tolk.Speak("Složka s makry neexistuje", true);
                return;
            }

            // English: Get *.txt macro files and strip extensions
            string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.TopDirectoryOnly);
            List<string> names = files
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (names.Count == 0)
            {
                Tolk.Speak("Žádná makra nenalezena", true);
                return;
            }

            // English: Build menu items (single-column, macro names)
            List<List<string>> items = names.Select(n => new List<string> { n }).ToList();

            // English: Open menu and run selected macro in the callback
            MenuParameters parameters = new(
                items: items,
                introText: "Vyber makro",
                wrappingAllowed: false,
                menuClosed: (index) =>
                {
                    if (index == -1)
                        return;

                    string selected = items[index][0];
                    PlayMacroByName(selected);
                }
            );
            WindowHandler.Menu(parameters);
        }

        [DebugCommand(DebugCommand.StartMacroRecording)]
        private void StartMacroRecording()
        {
            if (_macroPlayer != null && _macroPlayer.IsPlaying)
                return;

            _macroRecorder?.StartRecording();
        }

        [DebugCommand(DebugCommand.StopMacroRecording)]
        private void StopMacroRecording()
        {
            _macroRecorder?.StopAndSave();
        }

        private void TryAddMacroMethod(BindingFlags flags, string name)
        {
            MethodInfo mi = GetType().GetMethod(name, flags);
            if (mi != null) _macroControlMethods.Add(mi);
        }

        private void PlayMacroByName(string name = null)
        {
            if (_macroRecorder != null && _macroRecorder.IsRecording)
                return;

            if (string.IsNullOrWhiteSpace(name))
                return;

            _macroPlayer?.Play(name);
        }

        [DebugCommand(DebugCommand.OpenSettings)]
        public void OpenSettings() => WindowHandler.OpenDebugSettings();

        [DebugCommand(DebugCommand.OpenLog)]
        public void OpenLog() => Logger.OpenLog();

        public override void OnKeyDown(KeyboardInput shortcut)
        {
            base.OnKeyDown(shortcut);

            if (_macroRecorder != null && _macroRecorder.IsRecording && !IsMacroControlShortcut(shortcut))
                _macroRecorder.FeedKeyDown(shortcut);

            Action action = null;
            if (_keyboardCommands.TryGetValue(shortcut, out action))
                action();
        }

        /// <summary>
        /// KeyUp event handler. Forwards the shortcut to MacroRecorder when recording and not a macro-control shortcut.
        /// </summary>
        public override void OnKeyUp(KeyboardInput shortcut)
        {
            base.OnKeyUp(shortcut);

            if (_macroRecorder != null && _macroRecorder.IsRecording && !IsMacroControlShortcut(shortcut))
                _macroRecorder.FeedKeyUp(shortcut);
        }

        private Dictionary<KeyboardInput, Action> _keyboardCommands = new();
        private Dictionary<DualSenseInput, Action> _gamepadCommands = new();

        private Character Tuttle => World.GetCharacter("tuttle");
        private Character Player => World.Player;

        /// <summary>
        /// A test method that saves current position as start position.
        /// </summary>
        [DebugCommand(DebugCommand.SaveStartPosition)]
        private void SaveStartPosition()
        {
            Settings.TestChipotleStartPosition = Player.Center;
            Settings.SaveSettings();
            Tolk.Speak("Startovní pozice uložena", true);
        }

        /// <summary>
        /// Test function to announce Tuttle's position
        /// </summary>
        [DebugCommand(DebugCommand.SayTuttlesPosition)]
        private void SayTuttlesPosition()
        {
            string distance = World.GetDistance(Tuttle, Player).ToString();
            string position = Tuttle.Center.ToString();
            string zone = Tuttle.Zone.Name.Inner;
            Tolk.Speak(distance + Environment.NewLine + zone + " " + position, true);
        }

        /// <summary>
        /// Reports current position of the player in relative coordinates.
        /// </summary>
        [DebugCommand(DebugCommand.SayRelativeCoordinates)]
        private void SayRelativeCoordinates()
        {
            SayCoordinates message = new(this);
            Player.TakeMessage(message);
        }

        [DebugCommand(DebugCommand.SayItemSize)]
        private void SayItemSize()
        {
            SayItemSize message = new(this);
            Player.TakeMessage(message);
        }

        [DebugCommand(DebugCommand.ResetGame)]
        private void ResetGame() => WindowHandler.ResetGame();

        [DebugCommand(DebugCommand.RestoreStartPosition)]
        private void RestoreStartPosition()
        {
            if (!Settings.TestCommandsEnabled)
                return;

            Settings.TestChipotleStartPosition = null;
            Settings.SaveSettings();
            Tolk.Speak("Startovní pozice obnovena", true);
        }


        /// <summary>
        /// Test method that moves Chipotle to coords taken from clipboard
        /// </summary>
        [DebugCommand(DebugCommand.GoToClipboardCoords)]
        private void GoToClipboardCoords()
        {
            try
            {
                string coords = GUIUtility.systemCopyBuffer;
                Vector2 target = coords.ToVector2();
                GoToCoords(target);
            }
            catch (Exception) { }
        }

        public void GoToCoords(Vector2 coords)
        {
            SetPosition message = new(this, coords);
            Player.TakeMessage(message);
        }

        private const string _walkablePointsPath = "WalkablePoints.yaml";
        private const string _commandMapPath = "DebugCommands.yaml";

        private Dictionary<string, List<Vector2>> _walkablePoints;

        public override void Initialize()
        {
            base.Initialize();
            LoadWalkablePoints();
            LoadCommands();
            InitMacroSupport();
            CreatePipeServer();
        }

        private void LoadCommands()
        {
            string path = Path.Combine(MainScript.DebugPath, _commandMapPath);
            try
            {
                Dictionary<string, YamlCommandBindings> rawMap = null;
                YamlHelper.LoadFromFile(path, out rawMap);

                // Get all DebugManager methods with DebugCommand attribute
                MethodInfo[] methods = typeof(DebugManager).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Dictionary<DebugCommand, Action> commandMethods = methods
                    .Select(m => new
                    {
                        Method = m,
                        Attr = m.GetCustomAttribute<DebugCommandAttribute>()
                    })
                    .Where(x => x.Attr != null)
                    .ToDictionary(x => x.Attr.Command, x => (Action)Delegate.CreateDelegate(typeof(Action), this, x.Method));

                // Clear old dictionaries
                _keyboardCommands.Clear();
                _gamepadCommands.Clear();

                foreach (KeyValuePair<string, YamlCommandBindings> pair in rawMap)
                {
                    // Convert string key to DebugCommand enum
                    if (!Enum.TryParse<DebugCommand>(pair.Key, out DebugCommand command))
                        continue;

                    // Skip if there is no method for this DebugCommand
                    if (!commandMethods.TryGetValue(command, out Action action))
                        continue;

                    // Add keyboard input to dictionary if present
                    if (!string.IsNullOrEmpty(pair.Value.Keyboard))
                    {
                        KeyboardInput keyboardInput = new KeyboardInput(pair.Value.Keyboard);
                        _keyboardCommands[keyboardInput] = action;
                    }

                    // Add DualSense input to dictionary if present
                    if (!string.IsNullOrEmpty(pair.Value.DualSense))
                    {
                        DualSenseInput gamepadInput = new DualSenseInput(pair.Value.DualSense);
                        _gamepadCommands[gamepadInput] = action;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Chyba při načítánídefinice testovacích příkazů", e.ToString());
            }
        }

        private void LoadWalkablePoints()
        {
            try
            {
                string path = Path.Combine(MainScript.DebugPath, _walkablePointsPath);
                Dictionary<string, List<float[]>> raw = null;
                YamlHelper.LoadFromFile(path, out raw);
                _walkablePoints = raw.ToDictionary(k => k.Key, v => v.Value.Select(p => new Vector2(p[0], p[1])).ToList());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Opens a menu with all zones and jumps to the nearest walkable position in the selected zone.
        /// </summary>
        [DebugCommand(DebugCommand.JumpToZoneMenu)]
        private void JumpToZoneMenu()
        {
            IEnumerable<Zone> zones = World.GetZones();
            List<List<string>> items =
            (
                from z in zones
                orderby z.Name.Inner
                select (new List<string> { z.Name.Inner })
            ).ToList();

            MenuParameters parameters = new
                (
                items,
                "Vyber lokaci",
                menuClosed: (int item) => JumpToZone(item, items)
                );
            WindowHandler.Menu(parameters);
        }

        private void JumpToZone(int item, List<List<string>> items)
        {
            if (item == -1)
                return;

            string zone = items[item][0];
            Vector2 pointForPlayer = _walkablePoints[zone][0];
            SetPosition message1 = new(this, pointForPlayer);
            Player.TakeMessage(message1);

            // Move Tuttle
            if (!Settings.LetTuttleFollowChipotle)
                return;
            Vector2 pointForTuttle = _walkablePoints[zone][1];
            SetPosition message2 = new(null, pointForTuttle);
            Tuttle.TakeMessage(message2);
        }
    }
}