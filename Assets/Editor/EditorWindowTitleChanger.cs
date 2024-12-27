using UnityEditor;
using UnityEditor.Callbacks;
using System;

[InitializeOnLoad]
public class EditorWindowTitleChanger
{
    private static string originalTitle;
    private static bool isTitleSet = false;

    static EditorWindowTitleChanger()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.quitting += ResetTitle;
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => ResetTitle();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SetRunningTitle();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ResetTitle();
        }
    }

    private static void SetRunningTitle()
    {
        if (!isTitleSet)
        {
            originalTitle = UnityEditor.EditorWindow.focusedWindow?.titleContent.text;
            if (originalTitle != null)
            {
                UnityEditor.EditorWindow.focusedWindow.titleContent.text = "Running";
                isTitleSet = true;
            }
        }
    }

    private static void ResetTitle()
    {
        if (isTitleSet && originalTitle != null)
        {
            UnityEditor.EditorWindow.focusedWindow.titleContent.text = originalTitle;
            isTitleSet = false;
        }
    }
}
