using Game.Terrain;

using Luky;

namespace Game.Entities
{
    public abstract class PhysicsComponent : EntityComponent
    {
        protected int _walkSpeed;

        private void Appear(Plane target) => target.GetTiles().Foreach(t => t.Register(Owner));

        private void DisAppear() 
            => _area.GetTiles()
            .Foreach(t => t.UnregisterObject());

        protected void SetPosition(float x, float y)
            => SetPosition(new Vector2(x, y));

        protected void SetPosition(Vector2 coords)
            => SetPosition(new Plane(coords));

        protected void SetPosition(Plane target)
        {
            if (_area != null)
            {
                DisAppear();
            }

            Appear(target);
            _area = new Plane(target);
        }


        //todo PhisicsComponent

        protected Orientation2D _orientation;
        public Orientation2D Orientation => new Orientation2D(_orientation);

        public Plane Area => new Plane(_area);
        protected Plane _area;



    }
}
