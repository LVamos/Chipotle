namespace Game.Controls
{
    public enum CommandId
    {
        // Menu commands
        MenuPreviousItem,
        MenuNextItem,
        MenuActivateItem,
        MenuFirstItem,
        MenuLastItem,
        MenuQuit,

        // Game commands
        GameToggleSonar,
        GamePlaceItem,
        GameApplyItemToItem,
        GameSendFeedback,
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