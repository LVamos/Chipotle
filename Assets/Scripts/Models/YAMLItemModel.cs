namespace Game.Models
{
	public class YamlItemModel
	{
		public string Type { get; set; }
		public string ClassName { get; set; }
		public string CollisionSound { get; set; }
		public string ActionSound { get; set; }
		public string LoopSound { get; set; }
		public string Cutscene { get; set; }
		public bool UsableOnce { get; set; }
		public bool AudibleOverWalls { get; set; }
		public float Volume { get; set; }
		public bool StopWhenPlayerMoves { get; set; }
		public bool QuickActionsAllowed { get; set; }
		public string PickingSound { get; set; }
		public string PlacingSound { get; set; }
	}
}