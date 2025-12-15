# World Optimizer Development Task List

-[x] Initial Planning
    -[x] Create documentation directory and files `docs/world_optimizer`
    -[x] Define features in implementation_plan.md
    -[x] User review of plan
-[x] Implementation
    -[x] Create `Assets/WorldOptimizer/Editor` directory
    -[x] Create `WorldOptimizerWindow.cs` skeleton
    -[x] Implement "Remove Missing Scripts" logic
    -[x] Implement "Enable GPU Instancing" logic
    -[x] Implement "Texture Settings" logic (Max size, Compression)
    -[x] Implement "Static Flags" logic
    -[x] Implement "Audio Settings" logic
    -[x] Connect all logic to the Editor Window UI
    -[x] Localize UI and Messages to Japanese
-[x] Verification
    -[x] Verify compilation (Code Review)
    -[x] Manual verification (User to test in Unity)

-[x] Phase 2: Advanced Optimizations
    -[x] Update implementation plan
    -[x] Implement Mesh Analysis (Polygon count scan)
    -[x] Implement Simple Mesh Combiner (Combine static meshes by material)
    -[x] Implement Lighting Settings Optimizer (Lightmap resolution, Bounces)
    -[x] Integrate advanced features into `WorldOptimizerWindow`
    -[x] Verify advanced features

-[x] Phase 3: Quality Profiles
    -[x] Update implementation plan
    -[x] Implement Profile Selection (Balanced / High Quality) in `WorldOptimizerWindow`
    -[x] Adjust Texture logic for High Quality (Max 4096, High Comp Quality)
    -[x] Adjust Lighting logic for High Quality (Higher Res, Bounces)
    -[x] Verify High Quality mode

-[x] Phase 4: Backup, Speed & Localization
    -[x] Update implementation plan
    -[x] Implement `BackupManager` (Scene copy + Asset settings snapshot)
    -[x] Optimize Asset operations (Process Scene Dependencies only instead of All Assets)
    -[x] Translate all code comments to Japanese
    -[x] Add "Create Backup" and "Restore" UI
    -[x] Verify Backup/Restore flow

-[x] Phase 5: Bug Fixes & Robustness
    -[x] Fix GetSceneDependencies type casting bug
    -[x] Add comprehensive error handling (try-catch)
    -[x] Add null reference checks throughout
    -[x] Improve BackupManager type safety
    -[x] Enhance MeshCombiner robustness

-[x] Phase 6: SDK Troubleshooting
    -[x] Add "Clear Blueprint ID" feature to fix "world we do not own" error
    -[x] Verify fix resolves the log warning

-[x] Phase 7: Packaging
    -[x] Create `Assets/WorldOptimizer/package.json`
    -[x] Create `Assets/WorldOptimizer/README.md`
    -[x] Create `Assets/WorldOptimizer/Editor/WorldOptimizer.Editor.asmdef`

-[x] Phase 8: Reporting
    -[x] Implement OptimizationStats class
    -[x] Implement RuntimeMemorySize measurement for textures
    -[x] Display results in dialog and console logs

-[x] Phase 9: Non-Destructive Optimization & Backface Culling
    -[x] Create `AssetCloner` helper class
    -[x] Implement `GetOrCreateClone<T>` logic
    -[x] Update `WorldOptimizerWindow` to use Cloning for Materials (Instancing/Culling)
    -[x] Update `WorldOptimizerWindow` to use Cloning for Audio
    -[x] Update `WorldOptimizerWindow` to use Cloning for Textures (Shader property scanning)
    -[x] Implement Backface Culling logic (Force Single Sided) using Cloned Materials

-[x] Phase 10: Compilation & Stability Fixes
    -[x] Restore missing `OptimizationStats` class and `RunAllOptimizations` method
    -[x] Restore missing `RemoveMissingScripts` method
    -[x] Fix invalid API calls (Material.supportsInstancing)
    -[x] Fix obsolete API warnings (LightmapEditorSettings)
    -[x] Verify full file integrity
    -[x] Verify full file integrity

-[x] Phase 11: LOD & Crunch Compression Features
    -[x] Add "Texture Crunch Settings" (Toggle & Quality Slider) to UI
    -[x] Implement specific Crunch Compression logic (with safeguards)
    -[x] Add "Level Of Detail (LOD) Bias" slider to UI
    -[x] Implement LOD Bias setting logic
    -[x] Verify new features

-[x] Phase 12: Occlusion Culling Support
    -[x] Add Occlusion Culling UI (Bake/Clear buttons) to Advanced Tab
    -[x] Implement parameter configuration (Smallest Occluder, etc.) logic
-[x] Integrate with StaticOcclusionCulling API

-[x] Phase 13: Error Logging
    -[x] Implement `LogError` helper to write to `WorldOptimizer_Error.log`
    -[x] Replace console error logging with file logging in all try-catch blocks
    -[x] Add "Open Log File" UI to Help tab

-[x] Phase 14: Stability & Crash Protection
    -[x] Wrap main execution loop in `try-catch` to prevent Editor crashes
    -[x] Extend logging to `BackupManager` to capture backup failures
    -[x] Ensure all helper classes propagate errors to `WorldOptimizer_Error.log`

-[x] Phase 15: Individual Execution & Texture Control
    -[x] Add "Run" buttons for individual optimization steps in Basic Tab
    -[x] Add "Max Texture Size" dropdown to Basic Tab
    -[x] Link Texture Size dropdown to optimization logic
    -[x] Ensure Profile selection updates Texture Size defaults
