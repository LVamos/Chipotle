using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

namespace Game.Entities
{
    /// <summary>
    /// Allows the player to scroll the entity using the keyboard.
    /// </summary>
    public class ChipotleInputComponent : InputComponent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ChipotleInputComponent() : base()
        {
        }

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterShortcuts(
            new Dictionary<KeyShortcut, Action>()
            {
                // Shortcuts for testing purposes

                [new KeyShortcut(Keys.Space)] = StopCutscene,
                [new KeyShortcut(Keys.T)] = TerrainInfo,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Left)] = MoveLeft,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Right)] = MoveRight,

                // Other shortcuts
                [new KeyShortcut(Keys.O)] = SayNearestObject,
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
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneBegan(CutsceneBegan message)
        {
            base.OnCutsceneBegan(message);

            switch (message.CutsceneName)
            {
                case "cs7": case "cs10": _messagingEnabled = false; break;
            }
        }

        /// <summary>
        /// Allows the player to use a nearby object or door.
        /// </summary>
        private void Interact()
=> Owner.ReceiveMessage(new UseObject(this));

        /// <summary>
        /// Moves the NPC one step back.
        /// </summary>
        private void MoveBack()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.Around));

        /// <summary>
        /// Moves the NPC one step forth.
        /// </summary>
        private void MoveForward()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.None));

        /// <summary>
        /// Moves the NPC one step to the left perpendicullar to current orientation.
        /// </summary>
        private void MoveLeft()
             => Owner.ReceiveMessage(new MakeStep(this, TurnType.SharplyLeft));

        /// <summary>
        /// Moves the NPC one step to the right perpendicullar to current orientation.
        /// </summary>
        private void MoveRight()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.SharplyRight));

        /// <summary>
        /// Announces the public name of the locality where the NPC is currently located using a
        /// screen reader or a voice synthesizer.
        /// </summary>
        private void SayLocality()
            => Owner.ReceiveMessage(new SayLocality(this));

        /// <summary>
        /// Reports the nearest objects around the NPC using a screen reader or voice synthesizer.
        /// </summary>
        private void SayNearestObject()
=> Owner.ReceiveMessage(new SayNearestObject(this));

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
    }
}