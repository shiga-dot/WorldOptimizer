# Protocol: Add File Backup Option

- [x] **Plan**: Design Backup Folder Structure and UI <!-- id: 0 -->
- [x] **Feature**: Implement `BackupManager.CreateFileBackup(path)` <!-- id: 1 -->
    - Copy target asset to `Assets/WorldOptimizer/Backups/...`
    - Maintain folder structure to avoid collisions.
- [x] **UI**: Add "最適化前にファイルをバックアップする (Create File Backup)" toggle <!-- id: 2 -->
- [x] **Integration**: Call `CreateFileBackup` before modifying Textures/Audio <!-- id: 3 -->
- [x] **Verify**: Check if files are copied correctly <!-- id: 4 -->
