using Luky;
using Game.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Game.Entities
{
  public abstract class PhysicsComponent: EntityComponent
    {
        protected int _walkSpeed;

        private void Appear(Plane target)
        {
            target.GetTiles().Foreach(t => t.Register(Owner));
        }

        private void DisAppear()
        {
            Area.GetTiles().Foreach(t => t.UnregisterObject());
        }


        protected void SetPosition(Plane target)
        {
            if(_area!=null)
            DisAppear();
            Appear(target);
            _area = new Plane(target);
        }


        //todo PhisicsComponent

        protected Orientation2D _orientation;
        public Orientation2D Orientation        {            get => new Orientation2D(_orientation);        }

        public Plane Area { get=>new Plane(_area); }
        protected Plane _area;



    }
}
