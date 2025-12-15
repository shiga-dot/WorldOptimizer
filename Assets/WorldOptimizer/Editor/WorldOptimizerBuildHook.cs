using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace WorldOptimizer
{
    [InitializeOnLoad]
    public class WorldOptimizerBuildHook
    {
        private static bool requestRevert = false;

        static WorldOptimizerBuildHook()
        {
            // Hook PlayMode change (Common for VRChat Uploads)
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!WorldOptimizerWindow.EnableAutoOptimizeOnUpload) return;

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Entering Play Mode (Potential Upload)
                // We should check if this is an Upload or just Play.
                // VRChat Upload usually calls methods in VRCSDK.
                // It is hard to distinguish without SDK reference.
                // For 'Beta', let's optimize on ANY Play Mode entry if enabled, 
                // but maybe we can ask? Or just do it.
                // "Upload" in toggle text might be misleading if it runs on Play.
                // Let's add a Dialog? "Auto Optimize running..."
                
                // However, modifying assets during ExitingEditMode might cause reload issues?
                // Unity locks AssetDatabase during PlayMode transition sometimes.
                // But ExitingEditMode is before the lock?
                
                // Use a flag to avoid loops
                if (WorldOptimizerWindow.EnableAutoOptimizeOnUpload)
                {
                    Debug.Log("[WorldOptimizer] Auto-Optimizing for Play/Upload...");
                    // Create window instance to run optimization
                    var window = ScriptableObject.CreateInstance<WorldOptimizerWindow>();
                    window.RunAllOptimizationsPublic();
                    ScriptableObject.DestroyImmediate(window);
                    
                    requestRevert = true;
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Returned from Play/Upload
                if (requestRevert)
                {
                    Debug.Log("[WorldOptimizer] Reverting Optimization...");
                    if (BackupManager.HasBackup())
                    {
                        BackupManager.RestoreAssets();
                    }
                    requestRevert = false;
                }
            }
        }
    }
}
