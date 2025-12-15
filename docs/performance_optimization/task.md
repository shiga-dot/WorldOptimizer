# Protocol: Optimize Execution Speed

- [x] **Plan**: Analyze bottlenecks and design batching strategy <!-- id: 0 -->
- [x] **Refactor**: Optimize `BackupManager` to batch disk writes (Save on Finish, not per record) <!-- id: 1 -->
- [x] **Refactor**: Implement `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` in `WorldOptimizerWindow` <!-- id: 2 -->
- [x] **Refactor**: Consolidate `AssetDatabase.SaveAssets()` / `Refresh()` calls <!-- id: 3 -->
- [x] **Refactor**: Optimize `Resources.FindObjectsOfTypeAll` usage (Cache or Reduce calls) <!-- id: 4 -->
- [ ] **Verify**: Measure execution time improvements <!-- id: 5 -->
