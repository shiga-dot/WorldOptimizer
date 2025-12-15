# Walkthrough - In-Place Optimization & Revert

I have updated the WorldOptimizer to perform **In-Place Optimization** (directly modifying original asset settings) instead of creating clones. To ensure safety, a JSON-based backup system has been implemented.

## Changes

### 1. In-Place Modification
- The tool now modifies the **Original Assets** directly (e.g., changing Texture Import Settings, Audio Settings).
- **No Clones** are created. The `Assets/WorldOptimizer/Generated` folder is no longer used.

### 2. Persistent Backup System (`BackupManager.cs`)
- Before modifying any asset, its settings are saved to `ProjectSettings/WorldOptimizer_Backup.json`.
- This file persists even if you close Unity.
- **Supported Backups**:
    - **Textures**: Max Size, Compression, Mipmaps.
    - **Audio**: Mono, Load Type, Compression, Quality.
    - **Materials**: GPU Instancing, Cull Mode.

### 3. Revert Functionality
- Clicking **"Revert to Original"** reads the JSON backup and restores the settings to the assets.
- **Texture Resolution**: Yes, reverting will restore the original high resolution. This works because the tool only changes the "Import Settings" (how Unity reads the file), not the source file itself.

## Verification

### Optimize -> Revert Workflow
1.  **Backup**: Check `ProjectSettings/WorldOptimizer_Backup.json` after running optimization. It should contain entries for optimized assets.
2.  **Modify**: Assets in the Project view will show changed settings (e.g., lower Max Size).
3.  **Revert**: Clicking "Revert to Original" restores the settings. Use the Inspector to verify Max Size returns to original.

> [!IMPORTANT]
> Do not delete `ProjectSettings/WorldOptimizer_Backup.json` if you intend to revert changes later.

### 4. Auto Optimize on Upload (Beta)
- **Checkbox**: "アップロード時に自動で最適化を実行する" (Run Auto Optimization on Upload)
- **Function**:
    1. When you start the Upload (enters Play Mode), the tool automatically runs "Run All".
    2. Assets are modified in-place.
    3. When Upload finishes (exits Play Mode), the tool continuously checks and reverts changes automatically.
- **Note**: This assumes the upload process triggers Play Mode (standard for VRChat).

### 5. Advanced Features
- **Set All Lights to Baked**:
    - Converts all Realtime/Mixed lights in the scene to **Baked**.
    - Useful for reducing realtime lighting cost.
- **Download Size Optimization**:
    - Aggressively compresses textures (limit 1024px, Crunch enabled) and audio (Mono, Vorbis, Low Quality).
    - Drastically reduces file size for easier transfer/download.
    - **Note**: Modifies Import Settings, so **Revert** works for this too.

### 6. Performance
- **Batch Processing**: The tool now groups asset operations and disk writes.
    - **Faster Execution**: "Run All" and "Auto-Optimize" are significantly faster on large projects.
    - **Safe**: Even if the process is interrupted, critical data is handled safely.

### 7. File Backup (New)
- **Create File Backup**: Option in basic settings.
- When enabled, creates a physical copy of assets in `Assets/WorldOptimizer/Backups/` before modifying them.
- Provides an extra layer of safety beyond the Import Settings restore.
