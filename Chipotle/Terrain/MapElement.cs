using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Luky;

namespace Game.Terrain
{
public abstract  class MapElement: MessagingObject
	{
		public virtual void Destroy()
=> Disappear();

		protected  virtual void Appear() { }
		protected virtual void Disappear() 
		{
		}



		public MapElement(Name name, Plane area):base()
		{
			Name = name ?? throw new ArgumentException(nameof(name));
			Area = area;// ?? throw new ArgumentException(nameof(area));
		}

		public readonly  Name Name;

		public  Plane Area { get; protected set; }

		public override string ToString()
=> Name.Friendly;
	}
}
