        using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Game.Terrain;
using DavyKager;
using Luky;
using Game.UI;

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
                [new KeyShortcut(Keys.Space)] = Test,
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

		private void SayNearestObject()
=> Owner.ReceiveMessage(new NearestObjectAnnouncement(this));
		private void SayLocality()
            => Owner.ReceiveMessage(new LocalityAnnouncement(this));


        public ChipotleInputComponent():base()
        {
        }





        private void Test()
        {
            if (!DebugSO.TestModeEnabled) return;
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream("agojstereo"), role: null, looping: false, PositionType.Absolute, Owner.Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

        }

        private void SayOrientation()
        {
            if (!DebugSO.TestModeEnabled) return;
            Tolk.Speak($"Columbo orientation: {Owner.Orientation.UnitVector.ToString()}, listener facing: {World.Sound.ListenerOrientationFacing.ToString()}, listener up: {World.Sound.ListenerOrientationUp.ToString()}, listener position: {World.Sound.ListenerPosition.ToString()}.");
        }


        private void TerrainInfo()
            => Owner.ReceiveMessage(new TerrainInfo(this));

        private void MoveLeft()
             => Owner.ReceiveMessage(new Movement(this, TurnType.SharplyLeft));

        private void MoveRight()
            => Owner.ReceiveMessage(new Movement(this, TurnType.SharplyRight));

        private void TurnSharplyLeft()
=> Owner.ReceiveMessage(new Turnover(this, TurnType.SharplyLeft));

        private void Interact()
=> Owner.ReceiveMessage(new InteractionStartMessage(this));

        private void TurnSharplyRight()
=> Owner.ReceiveMessage(new Turnover(this, TurnType.SharplyRight));

        private void TurnRight()
=> Owner.ReceiveMessage(new Turnover(this, TurnType.SlightlyRight));

        private void TurnLeft()
=> Owner.ReceiveMessage(new Turnover(this, TurnType.SlightlyLeft));

        private void TurnAround()
            => Owner.ReceiveMessage(new Turnover(this, TurnType.Around));

        private void MoveBack()
            => Owner.ReceiveMessage(new Movement(this, TurnType.Around));

        private void MoveForward()
            => Owner.ReceiveMessage(new Movement(this, TurnType.None));

    }
}
