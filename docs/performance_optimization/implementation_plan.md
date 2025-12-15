# Implementation Plan - Execution Speed Optimization

The current implementation suffers from performance issues due to frequent disk I/O and asset re-imports.

## Bottlenecks
1.  **BackupManager**: Saves JSON to disk (`File.WriteAllText`) *every time* an asset is backed up. For 100 textures, this is 100 writes.
2.  **AssetImporter**: `SaveAndReimport()` triggers a synchronous reimport immediately. Doing this in a loop causes Unity to process assets one by one.
3.  **AssetDatabase**: `Refresh()` and `SaveAssets()` might be called redundantly.

## Proposed Changes

### 1. [BackupManager.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/BackupManager.cs)
- **Introduce Batching**: Add `BeginTransaction()` and `CommitTransaction()` methods.
- **Modifications**:
    - `Record...` methods will only update the in-memory list.
    - `CommitTransaction()` will write to disk once.
    - `AutoSave` flag to support legacy single calls if needed.

### 2. [WorldOptimizerWindow.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/WorldOptimizerWindow.cs)
- **AssetDatabase.StartAssetEditing()**:
    - Wrap the `RunAllOptimizations` loop in `StartAssetEditing()` and `StopAssetEditing()`.
    - This tells Unity to hold off on importing until `StopAssetEditing()` is called, allowing bulk processing.
- **Refactor Optimizers**:
    - Ensure `OptimizeTextures`, `OptimizeAudio`, etc., don't call `SaveAssets` or `Refresh` internally if running in "Batch Mode".
    - Pass a `BatchContext` or simply rely on the outer scope handling the final save.
- **Texture/Audio Importers**:
    - Continue using `SaveAndReimport()`, but because `StartAssetEditing()` is active, Unity *should* defer the heavy lifting? 
    - *Correction*: `SaveAndReimport` might still force it. 
    - **Better Approach**: Set properties on the Importer, but *do not* call `SaveAndReimport` immediately if possible? 
    - Actually, `AssetDatabase.StartAssetEditing()` effectively batches the *Import* process. When `StopAssetEditing()` is called, imports happen.
    - However, `importer.SaveAndReimport()` is explicit. 
    - **Optimization**: For `TextureImporter`, usage of `StartAssetEditing()` is the standard way to batch changes.

### 3. Strategy
```csharp
void RunAllOptimizations(bool silent) {
    AssetDatabase.StartAssetEditing(); // Stop auto-imports
    BackupManager.BeginTransaction();  // Stop disk writes
    
    try {
        // ... Run all individual optimizers ...
        // They call importer.SaveAndReimport(), but Unity defers the actual import work due to StartAssetEditing
        
        BackupManager.CommitTransaction(); // Write JSON once
    } 
    finally {
        AssetDatabase.StopAssetEditing(); // Trigger all imports at once (Parallelized by Unity)
    }
    AssetDatabase.SaveAssets();
}
```

## Verification
1.  Measure time taken for "Run All" on a project with ~50-100 textures.
2.  Ensure `BackupManager` correctly saves all data despite batching.
