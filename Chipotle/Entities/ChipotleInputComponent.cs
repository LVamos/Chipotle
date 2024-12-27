using DavyKager;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

using OpenTK;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Game.Entities
{
    /// <summary>
    /// Allows the player to scroll the entity using the keyboard.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class ChipotleInputComponent : InputComponent
    {
        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case KeyReleased kr: OnKeyUp(kr); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Reports current position of the player in relative coordinates.
        /// </summary>
        private void SayRelativeCoordinates()
            => InnerMessage(new SayCoordinates(this));

        /// <summary>
        /// Determines how quickly the game reacts to movement commands. The speed is in milliseconds.
        /// </summary>
        private const int _keyboardSpeed = 10;

        /// <summary>
        /// Constructor
        /// </summary>
        protected override void RegisterShortcuts()
        {
            base.RegisterShortcuts();

            AddShortcuts(
            new Dictionary<KeyShortcut, Action>()
            {
                // Test commands

                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.F5)] = LoadPredefinedSave,
                [new KeyShortcut(Keys.F5)] = CreatePredefinedSave,
                [new KeyShortcut(Keys.C)] = SayRelativeCoordinates,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.T)] = SayTuttlesPosition,
                [new KeyShortcut(Keys.F10)] = JumpToLocality,
                [new KeyShortcut(Keys.F11)] = SaveStartPosition,
                [new KeyShortcut(false, true, false, Keys.C)] = SayAbsoluteCoordinates,
                [new KeyShortcut(Keys.F12)] = GoToClipboardCoords,

                // Other commands
                [new KeyShortcut(Keys.P)] = ResearchObject,
                [new KeyShortcut(Keys.R)] = SayLocalityDescription,
                [new KeyShortcut(Keys.I)] = RunInventoryMenu,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Return)] = PickUpObject,
                [new KeyShortcut(Keys.Tab)] = GameMenu,
                [new KeyShortcut(Keys.L)] = SayLocalitySize,
                [new KeyShortcut(false, true, false, Keys.V)] = ListExits,
                [new KeyShortcut(false, true, false, Keys.O)] = ListObjects,
                [new KeyShortcut(Keys.S)] = SayOrientation,
                [new KeyShortcut(Keys.V)] = SayExits,
                [new KeyShortcut(Keys.Space)] = StopCutscene,
                [new KeyShortcut(Keys.T)] = TerrainInfo,
                [new KeyShortcut(Keys.B)] = SayVisitedRegion,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Left)] = GoLeft,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Right)] = GoRight,
                [new KeyShortcut(Keys.O)] = SayObjects,
                [new KeyShortcut(Keys.K)] = SayLocalityName,
                [new KeyShortcut(Keys.Up)] = GoForward,
                [new KeyShortcut(Keys.Down)] = GoBack,
                [new KeyShortcut(Keys.Left)] = TurnLeft,
                [new KeyShortcut(Keys.Right)] = TurnRight,
                [new KeyShortcut(KeyShortcut.Modifiers.Control, Keys.Left)] = TurnSharplyLeft,
                [new KeyShortcut(KeyShortcut.Modifiers.Control, Keys.Right)] = TurnSharplyRight,
                [new KeyShortcut(KeyShortcut.Modifiers.Control, Keys.Down)] = TurnAround,
                [new KeyShortcut(Keys.Return)] = Interact,
            }
            );

        }

        /// <summary>
        /// Instruucts the sound component to read description of the current locality.
        /// </summary>
        private void ResearchObject() => InnerMessage(new ResearchObject(this));

        private void SayLocalityDescription() => InnerMessage(new SayLocalityDescription(this));

        /// <summary>
        /// Performs the command to pick up an object off the ground.
        /// </summary>
        private void PickUpObject() => InnerMessage(new PickUpObject(this));

        /// <summary>
        /// Creates a predefined save.
        /// </summary>
        private void LoadPredefinedSave()
        {
            if (Program.Settings.AllowPredefinedSaves)
                InnerMessage(new LoadPredefinedSave(this));
        }

        /// <summary>
        /// Creates a predefined save.
        /// </summary>
        private void CreatePredefinedSave()
        {
            if (Program.Settings.AllowPredefinedSaves)
                InnerMessage(new CreatePredefinedSave(this));
        }

        /// <summary>
        /// Lists navigable objects.
        /// </summary>
        protected void ListObjects() => InnerMessage(new ListObjects(this));

        /// <summary>
        /// Runs the game menu
        /// </summary>
        private void GameMenu()
        {
            InnerMessage(new StopWalk(this)); // Stop Chipotle if he's going somewhere.

            // Prepare the menu
            (string name, Action command)[] commands =
            {
                ("Prozkoumej objekt: pé", ResearchObject),
                ("Rozhlédni se: r", SayLocalityDescription),
                                ("inventář: I", RunInventoryMenu),
                                ("použij objekt nebo dveře: entr", Interact),
                                ("Vezmi objekt: šift entr", PickUpObject),
                ("Jdi dopředu: horní šipka", StepForward),
                ("Jdi dozadu: dolní šipka", StepBack),
                ("Jdi doleva: šift levá šipka", StepLeft),
                ("Jdi doprava: šift pravá šipka", StepRight),
                ("Otoč se trochu doleva: levá šipka", TurnLeft),
                ("Otoč se trochu doprava: pravá šipka", TurnRight),
                ("Otoč se ostře doleva: kontrol levá šipka", TurnSharplyLeft),
                ("Otoč se ostře doprava: kontrol pravá šipka", TurnSharplyRight),
                ("Otoč se čelem vzad: kontrol dolní šipka", TurnAround),
                ("okolní objekty: O", SayObjects),
                ("Naveď mě k objektu: šift O", ListObjects),
                ("východy z lokace: Vé", SayExits),
                                ("Naveď mě k východu: šift vé", ListExits),
                                                ("kde jsem: ká", SayLocalityName),
                                                                ("Byl jsem tu: bé", SayVisitedRegion),
                                                                                ("Rozměry lokace: el", SayLocalitySize),
                                                                                                ("kompas: Es", SayOrientation),
                                                                                                                ("souřadnice: Cé", SayAbsoluteCoordinates),
                                                                                                                                ("Poslat zprávu autorovi: Kontrol zet", Program.SendFeedback),
                                                                                                                                ("hlavní menu: Iskejp", World.QuitGame),
            };

            // Run the menu
            List<List<string>> items = commands.Select(c => new List<string>() { c.name }).ToList();
            Program.MainWindow.GameInProgress = false;
            int item = WindowHandler.Menu(items, "Menu");
            Program.MainWindow.GameInProgress = true;
            if (item > 0)
                commands[item].command();
        }

        /// <summary>
        /// Runs the inventory menu.
        /// </summary>
        protected void RunInventoryMenu()
            => InnerMessage(new RunInventoryMenu(this));

        /// <summary>
        /// Moves the NPC one step to the right.
        /// </summary>
        private void StepRight()
        {
            GoRight();
            StopWalk();
        }

        /// <summary>
        /// Moves the NPC one step to the left.
        /// </summary>
        private void StepLeft()
        {
            GoLeft();
            StopWalk();
        }

        /// <summary>
        /// Moves the NPC one step back.
        /// </summary>
        private void StepBack()
        {
            GoBack();
            StopWalk();
        }

        /// <summary>
        /// Moves the NPC one step forth.
        /// </summary>
        private void StepForward()
        {
            GoForward();
            StopWalk();
        }

        /// <summary>
        /// Reports size of the locality in which the Chipotle NPC is currently located.
        /// </summary>
        private void SayLocalitySize()
            => InnerMessage(new SayLocalitySize(this));

        /// <summary>
        /// Test function to announce Tuttle's position
        /// </summary>
        private void SayTuttlesPosition()
        {
            if (!Program.Settings.TestCommandsEnabled)
                return;

            Character tuttle = World.GetCharacter("tuttle");
            string distance = World.GetDistance(tuttle.Area.Center, Owner.Area.Center).ToString();
            string position = tuttle.Area.Center.ToString();
            string locality = tuttle.Locality.Name.Indexed;
            Tolk.Speak(distance + Environment.NewLine + locality + " " + position, true);
        }

        /// <summary>
        /// Opens a menu with all localities and jumps to the nearest walkable position in the selected locality.
        /// </summary>
        private void JumpToLocality()
        {
            if (!Program.Settings.TestCommandsEnabled)
                return;

            Vector2 me = Owner.Area.Center;
			List<List<string>> items =
                (
                from l in World.GetLocalities()
                orderby l.Name.Indexed
                select (new List<string> { l.Name.Indexed })
                ).ToList();

            int item = WindowHandler.Menu(items, "Vyber lokaci");
            if (item == -1)
                return;

            Locality locality = World.GetLocality(items[item][0]);
            Vector2 point = locality.Area.GetWalkableTiles().First().position;
            InnerMessage(new SetPosition(this, new Rectangle(point)));

            // Move Tuttle
            point = locality.Area.GetWalkableTiles().First(t => t.position != point).position;
            World.GetCharacter("tuttle").TakeMessage(new SetPosition(null, new Rectangle(point)));
        }

        /// <summary>
        /// A test method that saves current position as start position.
        /// </summary>
        private void SaveStartPosition()
        {
            if (!Program.Settings.TestCommandsEnabled)
                return;

            string pos = Owner.Area.Center.X.ToString() + ", " + Owner.Area.Center.Y.ToString();
            File.WriteAllText("initpos.txt", pos);
            Tolk.Speak("Startovní pozice uložena", true);
        }

        private void SayAbsoluteCoordinates()
        {
            if (!Program.Settings.TestCommandsEnabled)
                return;

            Vector2 coords = Owner.Area.Center;
            string result = Math.Round(coords.X).ToString() + ", " + Math.Round(coords.Y);
            Clipboard.SetText(result);
            InnerMessage(new SayCoordinates(this, false));
        }


        /// <summary>
        /// Test method that moves Chipotle to coords taken from clipboard
        /// </summary>
        private void GoToClipboardCoords()
        {
            if (!Program.Settings.TestCommandsEnabled)
                return;

            Rectangle v = new Rectangle(Clipboard.GetText());
            InnerMessage(new SetPosition(this, v));
        }

        /// <summary>
        /// Lists exits from current locality.
        /// </summary>
        protected void ListExits()
            => InnerMessage(new ListExits(this));

        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneBegan(CutsceneBegan message)
        {
            base.OnCutsceneBegan(message);

            if (message.CutsceneName == "cs7" || message.CutsceneName == "cs10")
                _messagingEnabled = false;
        }

        /// <summary>
        /// Processes the KeyUp message.
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnKeyUp(KeyReleased message)
        {
            if (_cutsceneInProgress)
                return;

            HashSet<KeyShortcut> walkCommands = new HashSet<KeyShortcut>()
            {

                new KeyShortcut(Keys.LShiftKey),
                new KeyShortcut(Keys.RShiftKey),
                new KeyShortcut(Keys.Left),
                new KeyShortcut(Keys.Right),
                new KeyShortcut(Keys.Up),
                new KeyShortcut(Keys.Down),
                new KeyShortcut(false, true, false, Keys.Left),
                new KeyShortcut(false, true, false, Keys.Right)
            };

            if (walkCommands.Contains(message.Shortcut))
                StopWalk();
        }

        /// <summary>
        /// Reports list of all exits from current locality.
        /// </summary>
        protected void SayExits() => InnerMessage(new SayExits(this));

        /// <summary>
        /// Reports current orientation setting of the Chipotle NPC.
        /// </summary>
        protected void SayOrientation()
            => InnerMessage(new SayOrientation(this));

        /// <summary>
        /// Allows the player to use a nearby object or door.
        /// </summary>
        private void Interact()
=> InnerMessage(new Interact(this));

        /// <summary>
        /// Moves the NPC one step back.
        /// </summary>
        private void GoBack()
            => InnerMessage(new StartWalk(this, TurnType.Around));

        /// <summary>
        /// Starts Moving the NPC forth.
        /// </summary>
        private void GoForward()
            => InnerMessage(new StartWalk(this, TurnType.None));

        /// <summary>
        /// Starts moving the NPC to the left.
        /// </summary>
        private void GoLeft()
             => InnerMessage(new StartWalk(this, TurnType.SharplyLeft));

        /// <summary>
        /// Moves the NPC one step to the right perpendicullar to current orientation.
        /// </summary>
        private void GoRight()
            => InnerMessage(new StartWalk(this, TurnType.SharplyRight));

        /// <summary>
        /// Announces the public name of the locality where the NPC is currently located using a
        /// screen reader or a voice synthesizer.
        /// </summary>
        private void SayLocalityName()
            => InnerMessage(new SayLocalityName(this));

        /// <summary>
        /// Reports the nearest objects around the NPC using a screen reader or voice synthesizer.
        /// </summary>
        private void SayObjects()
=> InnerMessage(new SayObjects(this));

        /// <summary>
        /// Stops the currently playing cutscene.
        /// </summary>
        private void StopCutscene()
        {
            if (_cutsceneInProgress)
            {
                _cutsceneInProgress = false;
                World.StopCutscene(Owner);
            }
        }

        /// <summary>
        /// Tells the physics to stop the Chipotle NPC.
        /// </summary>
        private void StopWalk()
            => InnerMessage(new StopWalk(this));

        /// <summary>
        /// Reports the terrain on which the NPC is standing.
        /// </summary>
        private void TerrainInfo()
            => InnerMessage(new SayTerrain(this));

        /// <summary>
        /// Rotates the NPC around Z axis.
        /// </summary>
        private void TurnAround()
            => InnerMessage(new ChangeOrientation(this, TurnType.Around));

        /// <summary>
        /// Rotates the NPC around Z axis 45 degrees to the left.
        /// </summary>
        private void TurnLeft()
=> InnerMessage(new ChangeOrientation(this, TurnType.SlightlyLeft));

        /// <summary>
        /// Rotates the NPC around Z axis 45 degrees to the right.
        /// </summary>
        private void TurnRight()
=> InnerMessage(new ChangeOrientation(this, TurnType.SlightlyRight));

        /// <summary>
        /// Rotates the NPC around Z axis 90 degrees to the left.
        /// </summary>
        private void TurnSharplyLeft()
                        => InnerMessage(new ChangeOrientation(this, TurnType.SharplyLeft));

        /// <summary>
        /// Rotates the NPC around Z axis 90 degrees to the right.
        /// </summary>
        private void TurnSharplyRight()
=> InnerMessage(new ChangeOrientation(this, TurnType.SharplyRight));

        /// <summary>
        /// Reports if the player have already visited the current locality.
        /// </summary>
        private void SayVisitedRegion()
            => InnerMessage(new SayVisitedRegion(this));

        /// <summary>
        /// Processes the KeyDown message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnKeyDown(KeyPressed message)
        {
            if (_cutsceneInProgress && message.Shortcut != new KeyShortcut(Keys.Space))
                return;

            base.OnKeyDown(message);
        }
    }
}