using Microsoft.VisualBasic;

using DavyKager;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

using Luky;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Game.Entities
{
    public class ChipotleInputComponent : InputComponent
    {

        public override void Start()
        {
            base.Start();

            RegisterShortcuts(
            new Dictionary<KeyShortcut, Action>()
            {
                // Shortcuts for testing purposes
                [new KeyShortcut(KeyShortcut.Modifiers.Alt, Keys.J)] = JumpToCoordsFromClipBoard,
                [new KeyShortcut(Keys.J)] = JumpToCoords,

                [new KeyShortcut(Keys.T)] = TerrainInfo,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Left)] = MoveLeft,
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.Right)] = MoveRight,
                [new KeyShortcut(Keys.O)] = SayOrientation,

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

        // Just for testing purpose
        private void JumpToCoords()
        {
            Vector2 coords = new Vector2(Interaction.InputBox("Zadej souřadnice", "Skok na souřadnice", ""));
            Owner.ReceiveMessage(new SetPosition(this, new Plane(coords)));
        }

        // For testing only
        private void JumpToCoordsFromClipBoard()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane(new Vector2(Clipboard.GetText()))));



        private void SayNearestObject()
=> Owner.ReceiveMessage(new SayNearestObject(this));
        private void SayLocality()
            => Owner.ReceiveMessage(new SayLocality(this));


        public ChipotleInputComponent() : base()
        {
        }






        private void SayOrientation()
        {
            if (!DebugSO.TestModeEnabled)
            {
                return;
            }

            Tolk.Speak($"Columbo orientation: {Owner.Orientation.UnitVector.ToString()}, listener facing: {World.Sound.ListenerOrientationFacing.ToString()}, listener up: {World.Sound.ListenerOrientationUp.ToString()}, listener position: {World.Sound.ListenerPosition.ToString()}.");
        }


        private void TerrainInfo()
            => Owner.ReceiveMessage(new SayTerrain(this));

        private void MoveLeft()
             => Owner.ReceiveMessage(new MakeStep(this, TurnType.SharplyLeft));

        private void MoveRight()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.SharplyRight));

        private void TurnSharplyLeft()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SharplyLeft));

        private void Interact()
=> Owner.ReceiveMessage(new UseObject(this));

        private void TurnSharplyRight()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SharplyRight));

        private void TurnRight()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SlightlyRight));

        private void TurnLeft()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SlightlyLeft));

        private void TurnAround()
            => Owner.ReceiveMessage(new TurnEntity(this, TurnType.Around));

        private void MoveBack()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.Around));

        private void MoveForward()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.None));

    }
}
