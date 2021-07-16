using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Entities
{
	class TuttlePhysicsComponent: PhysicsComponent
	{
		public override void Start()
		{
			// set initial position.
			SetPosition(new Plane(new Vector2(1029, 1030)));
			_orientation = new Orientation2D(0, 1);
			_area.GetLocality().ReceiveMessage(new LocalityEntered(this, Owner));

			base.Start();
		}
	}
}
