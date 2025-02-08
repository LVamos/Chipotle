namespace Game.UI
{
		/// <summary>
		/// Defines possible actions
		/// </summary>
		public enum InventoryAction
		{
			/// <summary>
			/// No action selected
			/// </summary>
			None = 0,

			/// <summary>
			/// Puts an object on the ground.
			/// </summary>
			Place,

			/// <summary>
			/// Uses the selected item.
			/// </summary>
			Use,

			/// <summary>
			/// Applies the selected item to another item.
			/// </summary>
			ApplyToTarget
		}
	}
