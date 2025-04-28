using Game.Terrain;

using System;

using UnityEngine;

namespace Assets.Scripts.Models
{
	public class PreparedPortalAmbientModel
	{
		public Passage Passage;
		public Vector3 Position;
		public bool DoubleAttenuation;

		public PreparedPortalAmbientModel(Passage passage, Vector3 position, bool doubleAttenuation)
		{
			Passage = passage ?? throw new ArgumentNullException(nameof(passage));
			Position = position;
			DoubleAttenuation = doubleAttenuation;
		}
	}
}
