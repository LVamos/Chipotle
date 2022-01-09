using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using DavyKager;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Allows the player to scroll the entity using the keyboard.
    /// </summary>
    [Serializable]
    public class ChipotleInputComponent : InputComponent
    {
        /// <summary>
        /// Determines how quickly the game reacts to movement commands. The speed is in milliseconds.
        /// </summary>
        private const int _keyboardSpeed = 10;

        /// <summary>
        /// Constructor
        /// </summary>
        public ChipotleInputComponent() : base()
        {
        }

        /// <summary>
        /// Lists navigable objects.
        /// </summary>
        protected void ListNavigableObjects() => Owner.ReceiveMessage(new ListNavigableObjects(this));

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<Messaging.GameMessage>>
                {
                    [typeof(KeyReleased)] = (message) => OnKeyUp((KeyReleased)message)
                }
                );

            RegisterShortcuts(
            new Dictionary<KeyShortcut, Action>()
            {
                // Test commands
                [new KeyShortcut(Keys.F11)] = SaveStartPosition,
                [new KeyShortcut(false, true, false, Keys.C)] = SayAbsoluteCoords,
                [new KeyShortcut(Keys.F12)] = GoToClipboardCoords,

                // Other commands
                [new KeyShortcut(false, true, false, Keys.V)] = ListExits,
                [new KeyShortcut(false, true, false, Keys.O)] = ListNavigableObjects,
                [new KeyShortcut(Keys.S)] = SayOrientation,
                [new KeyShortcut(Keys.V)] = SayExits,
                [new KeyShortcut(Keys.Space)] = StopCutscene,
                [new KeyShortcut(Keys.T)] = TerrainInfo,
                [new KeyShortcut(Keys.N)] = VisitedLocality,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Left)] = MoveLeft,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Right)] = MoveRight,

                // Other shortcuts
                [new KeyShortcut(Keys.O)] = SaySurroundingObjects,
                [new KeyShortcut(Keys.L)] = SayLocality,
                [new KeyShortcut(Keys.Up)] = MoveForward,
                [new KeyShortcut(Keys.Down)] = MoveBack,
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
        /// A test method that saves current position as start position.
        /// </summary>
        private void SaveStartPosition()
        {
            if (!Program.TestMode)
                return;

            string pos = Owner.Area.Center.X.ToString() + ", " + Owner.Area.Center.Y.ToString();
            File.WriteAllText("initpos.txt", pos);
            Tolk.Speak("Startovní pozice uložena");
        }

        private void SayAbsoluteCoords()
        {
            if (!Program.TestMode)
                return;

            string coords = Owner.Area.Center.ToString();
            Clipboard.SetText(coords);
            Tolk.Speak(coords);
        }


        /// <summary>
        /// Test method that moves Chipotle to coords taken from clipboard
        /// </summary>
        private void GoToClipboardCoords()
        {
            if (!Program.TestMode)
                return;

            Plane v = new Plane(Clipboard.GetText());
            Owner.ReceiveMessage(new SetPosition(this, v));
        }

        /// <summary>
        /// Lists exits from current locality.
        /// </summary>
        protected void ListExits()
            => Owner.ReceiveMessage(new ListExits(this));

        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneBegan(CutsceneBegan message)
        {
            base.OnCutsceneBegan(message);

            if(message.CutsceneName == "cs7" || message.CutsceneName == "cs10")
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
        protected void SayExits() => Owner.ReceiveMessage(new SayExits(this));

        /// <summary>
        /// Reports current orientation setting of the Chipotle NPC.
        /// </summary>
        protected void SayOrientation() => Owner.ReceiveMessage(new SayOrientation(this));

        /// <summary>
        /// Allows the player to use a nearby object or door.
        /// </summary>
        private void Interact()
=> Owner.ReceiveMessage(new UseObject(this, OpenTK.Vector2.Zero));

        /// <summary>
        /// Moves the NPC one step back.
        /// </summary>
        private void MoveBack()
            => Owner.ReceiveMessage(new StartWalk(this, TurnType.Around));

        /// <summary>
        /// Moves the NPC one step forth.
        /// </summary>
        private void MoveForward()
            => Owner.ReceiveMessage(new StartWalk(this, TurnType.None));

        /// <summary>
        /// Moves the NPC one step to the left perpendicullar to current orientation.
        /// </summary>
        private void MoveLeft()
             => Owner.ReceiveMessage(new StartWalk(this, TurnType.SharplyLeft));

        /// <summary>
        /// Moves the NPC one step to the right perpendicullar to current orientation.
        /// </summary>
        private void MoveRight()
            => Owner.ReceiveMessage(new StartWalk(this, TurnType.SharplyRight));

        /// <summary>
        /// Announces the public name of the locality where the NPC is currently located using a
        /// screen reader or a voice synthesizer.
        /// </summary>
        private void SayLocality()
            => Owner.ReceiveMessage(new SayLocality(this));

        /// <summary>
        /// Reports the nearest objects around the NPC using a screen reader or voice synthesizer.
        /// </summary>
        private void SaySurroundingObjects()
=> Owner.ReceiveMessage(new SayNavigableObjects(this));

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
            => Owner.ReceiveMessage(new StopWalk(this));

        /// <summary>
        /// Reports the terrain on which the NPC is standing.
        /// </summary>
        private void TerrainInfo()
            => Owner.ReceiveMessage(new SayTerrain(this));

        /// <summary>
        /// Rotates the NPC around Z axis.
        /// </summary>
        private void TurnAround()
            => Owner.ReceiveMessage(new TurnEntity(this, TurnType.Around));

        /// <summary>
        /// Rotates the NPC around Z axis 45 degrees to the left.
        /// </summary>
        private void TurnLeft()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SlightlyLeft));

        /// <summary>
        /// Rotates the NPC around Z axis 45 degrees to the right.
        /// </summary>
        private void TurnRight()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SlightlyRight));

        /// <summary>
        /// Rotates the NPC around Z axis 90 degrees to the left.
        /// </summary>
        private void TurnSharplyLeft()
                        => Owner.ReceiveMessage(new TurnEntity(this, TurnType.SharplyLeft));

        /// <summary>
        /// Rotates the NPC around Z axis 90 degrees to the right.
        /// </summary>
        private void TurnSharplyRight()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SharplyRight));

        /// <summary>
        /// Reports if the player have already visited the current locality.
        /// </summary>
        private void VisitedLocality()
            => Owner.ReceiveMessage(new SayVisitedLocality(this));

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