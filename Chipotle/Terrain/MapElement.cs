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


		protected bool _editMode;

		public MapElement(Name name, Plane area, bool editMode=false):base()
		{
			_editMode = editMode;
			_name = name ?? throw new ArgumentException(nameof(name));
			_area = area;// ?? throw new ArgumentException(nameof(area));
		}

		public Name Name
		{
			get => _name;
			set =>_name = _editMode ? value : throw new InvalidOperationException("Forbidden in game mode");
		}









		protected Name _name;
		protected Plane _area;

		public Plane Area 
		{ 
			get =>_area==null ?null :  new Plane(_area); 
			set => _area= _editMode ? value : throw new InvalidOperationException("Forbidden in game mode");
		}

		public override string ToString()
=> Name.Friendly;
	}
}
