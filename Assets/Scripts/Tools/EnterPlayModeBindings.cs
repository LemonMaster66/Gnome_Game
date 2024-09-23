#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.ShortcutManagement;

    [InitializeOnLoad]
    public static class EnterPlayModeBindings
    {
        static EnterPlayModeBindings()
        {
            #if UNITY_EDITOR
                EditorApplication.playModeStateChanged += ModeChanged;
                EditorApplication.quitting += Quitting;
            #endif
        }
    
        static void ModeChanged(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.EnteredPlayMode)
                ShortcutManager.instance.activeProfileId = "PlayMode";
            else if (playModeState == PlayModeStateChange.EnteredEditMode)
                ShortcutManager.instance.activeProfileId = "Default Better";
        }
    
        static void Quitting()
        {
            ShortcutManager.instance.activeProfileId = "Default Better";
        }
    }
#endif
