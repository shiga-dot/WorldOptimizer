# Implementation Plan - No-Clone (In-Place) Optimization

The user requested to avoid creating Clones. To achieve "Non-destructive" behavior without Clones, we must:
1.  **Record** the original settings of assets before modifying them.
2.  **Save** this record to a persistent file (JSON) to survive Unity restarts/crashes.
3.  **Modify** the original assets directly.
4.  **Restore** the assets using the persistent record upon request.

## User Review Required

> [!IMPORTANT]
> - This method **modifies original assets on disk**.
> - Safety relies entirely on the `WorldOptimizer_Backup.json` file.
> - **Risk**: If this JSON file is deleted while assets are optimized, the original settings are lost (though the assets themselves remain valid, just compressed).

## Proposed Changes

### Assets/WorldOptimizer/Editor

#### [MODIFY] [BackupManager.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/BackupManager.cs)
- Introduce serializable classes: `AssetBackupStore`, `TextureBackupData`, `AudioBackupData`.
- Implement `SaveBackup(string path, data)` and `LoadBackup()`.
- Store backup data in `ProjectSettings/WorldOptimizer_Backup.json` (or similar hidden path).
- `RecordTextureSettings(importer)` checks if path is already backed up. If not, adds to store and saves JSON.
- `RestoreAssets()` reads JSON, applies settings back to Importers, and clears JSON.

#### [MODIFY] [WorldOptimizerWindow.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/WorldOptimizerWindow.cs)
- **Remove** calls to `AssetCloner`.
- **Revert** Logic:
    - `OptimizeTextures`:
        1. Call `BackupManager.RecordTextureSettings(importer)` for target.
        2. Modify `importer` directly.
    - `OptimizeAudio`:
        1. Call `BackupManager.RecordAudioSettings(importer)`.
        2. Modify `importer` directly.
- **UI**:
    - "Revert" button calls `BackupManager.RestoreAssets()`.
- **New Feature**:
    - Add static bool `EnableAutoOptimizeOnUpload` (persisted via EditorPrefs).
    - Add Toggle in UI to switch this.

#### [NEW] [WorldOptimizerBuildHook.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/WorldOptimizerBuildHook.cs)
- Implement `IVRCSDKBuildRequestedCallback`:
    - If `EnableAutoOptimizeOnUpload` is true:
        - Call `WorldOptimizerWindow.RunAllOptimizations()`.
- Implement `IVRCSDKPostprocessBuildCallback` (or `OnPostprocessScene`? No, post-build):
    - Wait, VRChat SDK sometimes doesn't have a reliable "Post Build" for Worlds that fires *after* upload but *not* scene cleanup.
    - **Strategy**: We will hook into `EditorApplication.playModeStateChanged`.
        - VRC upload often goes into Play Mode.
        - If we optimized, we set a flag `optimizedForUpload = true`.
        - When exiting Playmode (Upload finished/cancelled) -> Revert.
    - **Alternative**: Using `IVRCSDKBuildEndedCallback` (if it exists).
    - **Fallback**: Just manual Revert if automatic fails, but we try to automate.
    - **Best Bet**: `IVRCSDKBuildRequestedCallback` returns a bool. If we return true, build proceeds.
    - We will assume `IVRCSDKOnPackagingCallback` (VRC SDK3) might be better.
    - **Target**: `IVRCSDKBuildRequestedCallback` for Start. And we need to ensure Revert happens.
    - If VRChat Build Process crashes or user cancels, assets stay optimized. The "Revert" button in Window is the safety net.
    - We will implement `IVRCSDKPostprocessBuildCallback` if available.

#### [DELETE] `AssetCloner.cs` (Optional, or just unused)
- It's no longer needed for this strategy.

## Verification Plan

### Manual Verification
1.  **Setup**: Select a specific Texture (e.g. Max Size 2048, No Crunch).
2.  **Optimize**: Run Optimization (Target: Crunch 50).
3.  **Verify**: Texture is modified (Size changed, Crunch checked). No Clone created.
4.  **Persistence**: Restart Unity.
5.  **Revert**: Click Revert.
6.  **Verify**: Texture restored to Max Size 2048, No Crunch.
