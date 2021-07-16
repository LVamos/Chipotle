using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DavyKager;

using Luky;

using Game.Terrain;

namespace Game.Entities
{
    public class ChipotleSoundComponent : SoundComponent
    {
        public override void Update()
        {
            base.Update();
            UpdateListener();
        }



        public ChipotleSoundComponent():base()
        {


        }

        protected void SayOrientation()
            => Tolk.Speak(Owner.Orientation.Angle.GetCardinalDirection().GetDescription());

        private void OnTurnoverDone(TurnEntityResult  message)
        {
            SayOrientation();
        }

        public override void Start()
        {
            _sound.ListenerOrientationUp = new Vector3(0, -1, 0);
            base.Start();

            RegisterMessageHandlers(
            new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(TurnEntityResult )] =(m)=> OnTurnoverDone((TurnEntityResult )m),
                [typeof(EntityMoved )] =(m)=> OnMovementDone((EntityMoved )m),
                [typeof(ObjectsCollided)] = (m)=> OnCollision((ObjectsCollided)m),
                [typeof(TerrainCollided )] = (m)=> OnInpermeableTerrainCollision((TerrainCollided )m)
            }
            );

        }



        private void OnInpermeableTerrainCollision(GameMessage message)
        {
            Tolk.Speak("penetráces");
        }

        private void OnCollision(ObjectsCollided message)
        {
            var collidingObject = message.Tile.Object;

            // Announce
            Timer t = new Timer();
            t.Interval = 500;
            t.Tick += (object s, EventArgs e) => { Say(collidingObject); t.Stop(); };
            t.Start();
            _sound.Play(_sound.GetRandomSoundStream("movhitwall"), null, looping: false, PositionType.Absolute, Owner.Area.Center.AsOpenALVector(), true);
        }

        protected void UpdateListener()
        {
            Vector2 position = Owner.Area.Center;
            Vector2 orientation = Owner.Orientation.UnitVector;

            if (_sound.ListenerPosition.X != position.X || _sound.ListenerPosition.Z != position.Y)
                _sound.ListenerPosition = position.AsOpenALVector();

            if (_sound.ListenerOrientationFacing.X != orientation.X || _sound.ListenerOrientationFacing.Z != orientation.Y)
                _sound.ListenerOrientationFacing = orientation.AsOpenALVector();
        }

        private void OnMovementDone(EntityMoved  message)
        {
            PlayTerrain(message.Target);
        }

        private void PlayTerrain(Tile tile)
        {
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role:  null, looping:  false, PositionType.Relative, new Vector3(0, -1.7f, 0), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}
