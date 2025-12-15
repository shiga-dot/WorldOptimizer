# Individual Optimization & Texture Resolution Support

## Goal Description
Allow users to run optimization tasks individually without unchecking other options, and provide manual control over the maximum texture resolution.

## User Review Required
- None

## Proposed Changes

### WorldOptimizer
#### [MODIFY] [WorldOptimizerWindow.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/WorldOptimizerWindow.cs)
- **UI Changes in `DrawBasicTab`**:
    - Change the layout of optimization items. Instead of just a checkbox, provide a row with: `[Checkbox] Label [Run Button]`.
    - Add a `Max Texture Size` dropdown (1024, 2048, 4096, 8192) enabled when `Texture Optimization` is checked or globally available.
    - Decouple `maxTextureSize` from `OptimizationProfile` slightly (Profile sets a default, but user can override).

- **Logic Changes**:
    - Ensure `OptimizeTextures` uses the user-selected `maxTextureSize`.
    - Ensure individual buttons call the respective optimization methods (`RemoveMissingScripts`, `EnableGpuInstancing`, etc.) wrapped in error handling/undo logic similar to `RunAll`.

## Verification Plan
### Manual Verification
- **Individual Execution**: Click "Run" next to "Remove Missing Scripts" and verify it runs only that logic without affecting others.
- **Texture Resolution**: Change dropdown to 1024, run Texture Optimization, and check if textures are resized to max 1024.
- **Profile Switching**: Verify switching Profile (Standard/High) updates the Texture Size dropdown to defaults (2048/4096), but user can still manually change it afterwards.
