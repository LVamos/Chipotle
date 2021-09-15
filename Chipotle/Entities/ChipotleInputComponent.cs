using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DavyKager;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

using Luky;

using Microsoft.VisualBasic;

namespace Game.Entities
{
    public class ChipotleInputComponent : InputComponent
    {
        public ChipotleInputComponent() : base()
        {
        }

        public override void Start()
        {
            base.Start();

            RegisterShortcuts(
            new Dictionary<KeyShortcut, Action>()
            {
                // Shortcuts for testing purposes
                [new KeyShortcut(Keys.J)] = JumpToCoords,

                [new KeyShortcut(Keys.Space)] = StopCutscene,
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

            if (!_testModeEnabled)
                return;

            Clipboard.Clear();
            Timer t = new Timer();
            t.Interval = 30;
            t.Tick += (s, e) =>
            {
                Plane p;

                try
                {
                    p = new Plane(Clipboard.GetText().ToVector2());
                }
                catch (Exception ex)
                {
                    return;
                }
                Owner.ReceiveMessage(new SetPosition(this, p));
                Clipboard.Clear();
            };
            t.Start();
        }

        protected override void OnCutsceneBegan(CutsceneBegan message)
        {
            base.OnCutsceneBegan(message);

            switch (message.CutsceneName)
            {
                case "cs7": case "cs10": _messagingEnabled = false; break;
            }
        }

        private void Interact()
=> Owner.ReceiveMessage(new UseObject(this));

        // Just for testing purpose
        private void JumpToCoords()
        {
            if (!_testModeEnabled)
                return;

            string text = Interaction.InputBox("Zadej souřadnice", "Skok na souřadnice", "");
            Plane target = new Plane(text.ToVector2());
            Owner.ReceiveMessage(new SetPosition(this, target));
        }

        private void MoveBack()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.Around));

        private void MoveForward()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.None));

        private void MoveLeft()
             => Owner.ReceiveMessage(new MakeStep(this, TurnType.SharplyLeft));

        private void MoveRight()
            => Owner.ReceiveMessage(new MakeStep(this, TurnType.SharplyRight));

        private void SayLocality()
            => Owner.ReceiveMessage(new SayLocality(this));

        private void SayNearestObject()
=> Owner.ReceiveMessage(new SayNearestObject(this));

        private void SayOrientation()
        {
            if (!DebugSO._testModeEnabled)
                return;

            Tolk.Speak($"Columbo orientation: {Owner.Orientation.UnitVector.ToString()}, listener facing: {World.Sound.ListenerOrientationFacing.ToString()}, listener up: {World.Sound.ListenerOrientationUp.ToString()}, listener position: {World.Sound.ListenerPosition.ToString()}.");
        }

        private void StopCutscene()
        {
            if (_cutsceneInProgress)
            {
                _cutsceneInProgress = false;
                World.StopCutscene(Owner);
            }
        }

        private void TerrainInfo()
            => Owner.ReceiveMessage(new SayTerrain(this));

        private void TurnAround()
            => Owner.ReceiveMessage(new TurnEntity(this, TurnType.Around));

        private void TurnLeft()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SlightlyLeft));

        private void TurnRight()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SlightlyRight));

        private void TurnSharplyLeft()
                        => Owner.ReceiveMessage(new TurnEntity(this, TurnType.SharplyLeft));

        private void TurnSharplyRight()
=> Owner.ReceiveMessage(new TurnEntity(this, TurnType.SharplyRight));
    }
}