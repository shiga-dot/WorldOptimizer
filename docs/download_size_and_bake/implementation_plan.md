# Implementation Plan - Download Size & Light Bake

To address the user's request for reducing download size and baking lights.

## Proposed Changes

### [WorldOptimizerWindow.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/WorldOptimizerWindow.cs)

#### 1. Light Bake Settings (Lighting Optimization) (Add to Advanced Tab)
- **New Method**: `SetLightsToBaked()`
    - Find all `Light` components in scene.
    - Set `lightmapBakeType` to `Baked`.
    - Exclude Realtime-only lights if necessary (but user asked for "Make Baked").
    - **Logic**:
        ```csharp
        foreach(var light in lights) {
            Undo.RecordObject(light, "Set Light to Baked");
            light.lightmapBakeType = LightmapBakeType.Baked;
        }
        ```
- **UI**: Add button "すべてのライトをBakedにする (Set All Lights to Baked)" in Lighting section.

#### 2. Download Size Reduction (Add to Advanced Tab)
- **New Method**: `OptimizeForDownloadSize()`
    - **Textures**: Force Max Size 1024 (or 512 optional), Enable Crunch (Quality 50).
    - **Audio**: Force Mono, Vorbis, Quality 0.5.
    - **Mesh**: (Optional) Aggressive compression? (Maybe too risky).
- **UI**: Add "Download Size Optimization" section.
    - Button: "ダウンロードサイズ優先設定を適用 (Apply Download Size Optimization)"
    - HelpBox: "テクスチャサイズを制限(1024px)し、圧縮を強力にかけます。画質は低下しますが、容量を大幅に削減できます。"

## Verification Plan
1.  **Light Bake**:
    - Create Scene with Realtime/Mixed lights.
    - Click Button.
    - Verify all lights are `Baked` in Inspector.
2.  **Download Size**:
    - Import large 4k texture.
    - Run Optimization.
    - Verify Import Settings: Max 1024, Crunched.
3.  **Undo/Revert**:
    - Verify `Undo` works for component changes.
    - Verify `BackupManager` handles these changes if routed through `RunIndividual` or standard logic.
    - Since `OptimizeForDownloadSize` will reuse `OptimizeTextures` logic with overrides, existing Backup work should cover it.
    - `SetLightsToBaked` is a component change, `Undo` main system handles it. BackupManager focuses on Asset Importers. Light components are part of Scene/Prefab, so `Undo` is sufficient for Scene objects.

> [!NOTE]
> BackupManager currently stores Texture/Audio Importer settings. `OptimizeForDownloadSize` modifies Importers, so it MUST use `BackupManager`.
