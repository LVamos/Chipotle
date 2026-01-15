using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Controls
{
	public enum Command
	{
		SayAbsoluteCoordinates,
		LoadPredefinedSave,
		CreatePredefinedSave,
		SayCharacters,
		ListCharacters,
		SayNavigatedObjectLocation,
		ExploreItem,
		SayZoneDescription,
		RunInventoryMenu,
		PickUpItem,
		GameMenu,
		SayZoneSize,
		ListExits,
		ListItems,
		SayOrientation,
		SayExits,
		StopCutscene,
		TerrainInfo,
		SayVisitedRegion,
		GoLeft,
		GoRight,
		SayItems,
		SayZoneName,
		GoForward,
		GoBack,
		TurnLeft,
		TurnRight,
		TurnSharplyLeft,
		TurnSharplyRight,
		TurnAround,
		Interact,
		QuitGame
	}
}