using UnityEditor;

public class CloseWindowTool
{
    [MenuItem("Tools/Zavřít okno %#x")]
    static void CloseFocusedWindow()
    {
        if (EditorWindow.focusedWindow != null)
        {
            EditorWindow.focusedWindow.Close();
        }
        else
        {
            UnityEngine.Debug.Log("Žádné okno není aktuálně vybráno.");
        }
    }
}
