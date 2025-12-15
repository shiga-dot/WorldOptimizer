using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace WorldOptimizer
{
    /// <summary>
    /// 最適化前のバックアップと復元を管理するクラス
    /// </summary>
    public static class BackupManager
    {
        private const string BACKUP_FILE_PATH = "ProjectSettings/WorldOptimizer_Backup.json";
        private const string BACKUP_DIR_ROOT = "Assets/WorldOptimizer/Backups";

        [Serializable]
        public class AssetBackupStore
        {
            public List<TextureBackupData> textures = new List<TextureBackupData>();
            public List<AudioBackupData> audios = new List<AudioBackupData>();
            public List<MaterialBackupData> materials = new List<MaterialBackupData>();
        }

        [Serializable]
        public class MaterialBackupData
        {
            public string path;
            public bool enableInstancing;
            public int cullMode; // -1 if not exists or not backed up
        }

        [Serializable]
        public class TextureBackupData
        {
            public string path;
            public int maxTextureSize;
            public bool crunchedCompression;
            public int compressionQuality;
            public bool streamingMipmaps;
            public TextureImporterFormat textureFormat; // Platform default or specific
        }

        [Serializable]
        public class AudioBackupData
        {
            public string path;
            public bool forceToMono;
            public AudioClipLoadType loadType;
            public AudioCompressionFormat compressionFormat;
            public float quality;
        }

        private static AssetBackupStore currentStore = new AssetBackupStore();
        private static bool isTransactionActive = false;

        static BackupManager()
        {
            LoadBackup();
        }

        private static void LoadBackup()
        {
            if (File.Exists(BACKUP_FILE_PATH))
            {
                try
                {
                    string json = File.ReadAllText(BACKUP_FILE_PATH);
                    currentStore = JsonUtility.FromJson<AssetBackupStore>(json) ?? new AssetBackupStore();
                }
                catch { currentStore = new AssetBackupStore(); }
            }
        }

        private static void SaveBackup()
        {
            // If transaction is active, defer saving until CommitTransaction
            if (isTransactionActive) return;

            try
            {
                string json = JsonUtility.ToJson(currentStore, true);
                File.WriteAllText(BACKUP_FILE_PATH, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WorldOptimizer] Failed to save backup: {e.Message}");
            }
        }
        
        public static void BeginTransaction()
        {
            isTransactionActive = true;
        }

        public static void CommitTransaction()
        {
            isTransactionActive = false;
            SaveBackup();
        }

        public static void RecordTextureSettings(TextureImporter importer)
        {
            if (importer == null) return;
            // 既にバックアップ済みなら上書きしない（最初の状態を保持するため）
            if (currentStore.textures.Exists(x => x.path == importer.assetPath)) return;

            var data = new TextureBackupData
            {
                path = importer.assetPath,
                maxTextureSize = importer.maxTextureSize,
                crunchedCompression = importer.crunchedCompression,
                compressionQuality = importer.compressionQuality,
                streamingMipmaps = importer.streamingMipmaps,
            };
            currentStore.textures.Add(data);
            SaveBackup();
        }

        public static void RecordAudioSettings(AudioImporter importer)
        {
            if (importer == null) return;
            if (currentStore.audios.Exists(x => x.path == importer.assetPath)) return;

            var data = new AudioBackupData
            {
                path = importer.assetPath,
                forceToMono = importer.forceToMono,
                loadType = importer.defaultSampleSettings.loadType,
                compressionFormat = importer.defaultSampleSettings.compressionFormat,
                quality = importer.defaultSampleSettings.quality
            };
            currentStore.audios.Add(data);
            SaveBackup();
        }

        public static void RecordMaterialSettings(Material mat)
        {
            if (mat == null) return;
            string path = AssetDatabase.GetAssetPath(mat);
            if (string.IsNullOrEmpty(path)) return;
            
            if (currentStore.materials.Exists(x => x.path == path)) return;

            var data = new MaterialBackupData
            {
                path = path,
                enableInstancing = mat.enableInstancing,
                cullMode = mat.HasProperty("_Cull") ? mat.GetInt("_Cull") : -1
            };
            currentStore.materials.Add(data);
            SaveBackup();
        }

        public static void RestoreAssets(Action<string> onError = null)
        {
            int count = 0;
            LoadBackup(); // Ensure latest

            // Restore Textures
            foreach (var data in currentStore.textures)
            {
                try
                {
                    TextureImporter importer = AssetImporter.GetAtPath(data.path) as TextureImporter;
                    if (importer != null)
                    {
                        importer.maxTextureSize = data.maxTextureSize;
                        importer.crunchedCompression = data.crunchedCompression;
                        importer.compressionQuality = data.compressionQuality;
                        importer.streamingMipmaps = data.streamingMipmaps;
                        importer.SaveAndReimport();
                        count++;
                    }
                }
                catch (Exception e) { onError?.Invoke($"Restore Error ({data.path}): {e.Message}"); }
            }

            // Restore Audio
            foreach (var data in currentStore.audios)
            {
                try
                {
                    AudioImporter importer = AssetImporter.GetAtPath(data.path) as AudioImporter;
                    if (importer != null)
                    {
                        importer.forceToMono = data.forceToMono;
                        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                        settings.loadType = data.loadType;
                        settings.compressionFormat = data.compressionFormat;
                        settings.quality = data.quality;
                        importer.defaultSampleSettings = settings;
                        importer.SaveAndReimport();
                        count++;
                    }
                }
                catch (Exception e) { onError?.Invoke($"Restore Error ({data.path}): {e.Message}"); }
            }

            // Restore Materials
            foreach (var data in currentStore.materials)
            {
                try
                {
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(data.path);
                    if (mat != null)
                    {
                        mat.enableInstancing = data.enableInstancing;
                        if (data.cullMode != -1 && mat.HasProperty("_Cull"))
                        {
                            mat.SetInt("_Cull", data.cullMode);
                        }
                        EditorUtility.SetDirty(mat); // Not Import, just Dirty
                        count++;
                    }
                }
                catch (Exception e) { onError?.Invoke($"Restore Error ({data.path}): {e.Message}"); }
            }
            
            // Save Assets for Materials
            AssetDatabase.SaveAssets();

            // Clear and Save
            currentStore = new AssetBackupStore();
            SaveBackup();
            
            Debug.Log($"[WorldOptimizer] {count} assets restored from backup.");
            EditorUtility.DisplayDialog("Revert Complete", $"{count} assets have been restored to their original settings.", "OK");
        }

        public static void CreateFileBackup(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            
            try
            {
                if (!assetPath.StartsWith("Assets/")) return;

                string relativePath = assetPath.Substring("Assets/".Length); 
                string destPath = Path.Combine(BACKUP_DIR_ROOT, relativePath).Replace("\\", "/");
                string destDir = Path.GetDirectoryName(destPath);

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                
                if (File.Exists(destPath)) return; // Avoid overwrite

                AssetDatabase.CopyAsset(assetPath, destPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WorldOptimizer] File Backup Failed for {assetPath}: {e.Message}");
            }
        }

        public static bool HasBackup()
        {
            return currentStore.textures.Count > 0 || currentStore.audios.Count > 0 || currentStore.materials.Count > 0;
        }
    }
}
