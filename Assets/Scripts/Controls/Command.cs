using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Controls
{
	public enum Command
	{
		GameSayAbsoluteCoordinates,
		GameLoadPredefinedSave,
		GameCreatePredefinedSave,
		GameSayCharacters,
		GameListCharacters,
		GameSayNavigatedObjectLocation,
		GameExploreItem,
		GameSayZoneDescription,
		GameInventoryMenu,
		GamePickUpItem,
		GameMenu,
		GameSayZoneSize,
		GameListExits,
		GameListItems,
		GameSayOrientation,
		GameSayExits,
		GameStopCutscene,
		GameTerrainInfo,
		GameSayVisitedRegion,
		GameGoLeft,
		GameGoRight,
		GameSayItems,
		GameSayZoneName,
		GameGoForward,
		GameGoBack,
		GameTurnLeft,
		GameTurnRight,
		GameTurnSharplyLeft,
		GameTurnSharplyRight,
		GameTurnAround,
		GameInteract,
		GameQuit
	}
}