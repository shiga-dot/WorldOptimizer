# Implementation Plan - File Backup Option

Add a safety feature to physically copy assets before optimization.

## Changes

### 1. [BackupManager.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/BackupManager.cs)
- **New Method**: `CreateFileBackup(string assetPath)`
    - Target Dir: `Assets/WorldOptimizer/Backups/`
    - Logic:
        1. Calculate relative path (remove `Assets/`).
        2. Create subdirectories in Backup folder.
        3. `AssetDatabase.CopyAsset(assetPath, backupPath)`.
- **New Property**: `EnableFileBackup` (handled in Window, passed to Manager? Or Manager static?)

### 2. [WorldOptimizerWindow.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/WorldOptimizerWindow.cs)
- **UI**: Add Toggle `enableFileBackup` in Basic Tab (or Advanced?).
    - "バックアップ作成 (Create Backup Copy)"
- **Integration**:
    - In `OptimizeTextures`, `OptimizeAudio`:
    - Call `BackupManager.CreateFileBackup(path)` before modifying importer.

## Verification
- Run optimization with toggle ON.
- Verify `Assets/WorldOptimizer/Backups/` contains copies.
- Verify original files are modified.
- Verify copies are untouched (Original settings).
