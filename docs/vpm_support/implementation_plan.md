# Implementation Plan - VPM Support

Update `package.json` to be fully compatible with VRChat Package Manager (VCC).

## Changes

### 1. [package.json](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/package.json)
- **Add `vpmDependencies`**:
    - `"com.vrchat.worlds": "^3.0.0"` (Ensures it's treated as a World tool)
    - `"com.vrchat.base": "^3.0.0"` (Implicit usually, but good to be explicit)
- **Add `legacyFolders`**:
    - `"Assets/WorldOptimizer": ""` 
    - This allows VCC to detect and migrate existing UnityPackage installations.
- **Update Version**: Bump to `1.1.0` (reflecting Beta/Backup features).

## Verification
- Inspect JSON content.
- (Manual) User can verify by adding the folder to VCC (if they move it to a repo root) or packaging it.
