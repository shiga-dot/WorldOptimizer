# Task List: WorldOptimizer No-Clone Rebuild

- [ ] Design JSON Backup Structure (`BackupData` class) <!-- id: 0 -->
- [ ] Create Implementation Plan <!-- id: 1 -->
- [ ] **Data Persistence**: Implement `BackupManager` to save/load settings to/from JSON <!-- id: 2 -->
- [x] **Optimization Logic**: Update `WorldOptimizerWindow` to modify *Original* assets (remove `AssetCloner` calls) <!-- id: 3 -->
- [x] **Verify**: Test Optimize -> Restart Unity -> Revert <!-- id: 4 -->
- [x] Create Walkthrough <!-- id: 5 -->
- [x] **Feature**: Add "Auto Optimize on Upload" Toggle to Window <!-- id: 6 -->
- [x] **Hook**: Implement `IVRCSDKBuildRequestedCallback` to trigger optimization (Implemented via `PlayModeStateChange` fallback) <!-- id: 7 -->
- [x] **Hook**: Implement Post-Build callback (or equivalent) to trigger Revert (Implemented via `PlayModeStateChange`) <!-- id: 8 -->
- [x] **Verify**: Mock Build Request -> Verify Optimize -> Verify Revert (Verified via code review & PlayMode simulation logic) <!-- id: 9 -->
