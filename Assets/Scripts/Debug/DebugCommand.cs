using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Debug
{
	public enum DebugCommand
	{
		OpenEditorOnPoint,
		StopMacroRecording,
		StartMacroRecording,
		PlayMacroPrompt,
		OpenSettings,
		OpenLog,
		SayItemSize,
		SayRelativeCoordinates,
		SayTuttlesPosition,
		SaveStartPosition,
		ResetGame,
		RestoreStartPosition,
		JumpToZoneMenu,
		GoToClipboardCoords
	}
}