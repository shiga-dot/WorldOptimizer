using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WorldOptimizer
{
    public class WorldOptimizerWindow : EditorWindow
    {
        public enum OptimizationProfile
        {
            Standard, // 軽量化優先
            HighQuality, // クオリティ優先
            Custom // カスタム
        }

        [MenuItem("WorldOptimizer/Open Optimizer")]
        public static void ShowWindow()
        {
            GetWindow<WorldOptimizerWindow>("ワールド最適化ツール (ベータ)");
        }

        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private string[] tabs = { "基本設定", "詳細設定 (Advanced)", "ヘルプ (Help)" };

        // 設定 (Settings)
        private OptimizationProfile selectedProfile = OptimizationProfile.Standard;
        private bool enableRemoveMissingScripts = true;
        private bool enableGpuInstancing = true;
        private bool enableTextureCompression = true;
        private int maxTextureSize = 2048; // プロファイルによって更新される
        private bool enableAudioOptimization = true;
        private bool enableStaticFlags = true;
        
        // Auto Upload Settings
        public static bool EnableAutoOptimizeOnUpload
        {
            get { return EditorPrefs.GetBool("WorldOptimizer_AutoOptimizeOnUpload", false); }
            set { EditorPrefs.SetBool("WorldOptimizer_AutoOptimizeOnUpload", value); }
        }

        public static bool EnableFileBackup
        {
            get { return EditorPrefs.GetBool("WorldOptimizer_EnableFileBackup", false); }
            set { EditorPrefs.SetBool("WorldOptimizer_EnableFileBackup", value); }
        }

        // Analysis Data
        private List<KeyValuePair<GameObject, int>> heavyMeshes = new List<KeyValuePair<GameObject, int>>();

        // New Detailed Settings
        private float lodBias = 1.0f;
        private bool forceCrunch = false;
        private int crunchQuality = 50;

        // Occlusion Culling Settings
        private float smallestOccluder = 5.0f;
        private float smallestHole = 0.25f;
        private float backfaceThreshold = 100.0f;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("ワールド最適化ツール (ベータ)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("このツールはVRChat向けにシーンを最適化します。\n現在のシーンで使用されているアセットのみを対象に処理します。", MessageType.Info);

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space();

            if (selectedTab == 0)
            {
                DrawBasicTab();
            }
            else if (selectedTab == 1)
            {
                DrawAdvancedTab();
            }
            else
            {
                DrawHelpTab();
            }

            EditorGUILayout.EndScrollView();
        }
        
        private void DrawBasicTab()
        {
            EditorGUILayout.LabelField("基本最適化設定", EditorStyles.boldLabel);
            
            // File Backup Toggle
            bool fileBackup = EnableFileBackup;
            bool newFileBackup = EditorGUILayout.ToggleLeft("最適化前にファイルをバックアップする (Create File Backup)", fileBackup);
            if (newFileBackup != fileBackup) EnableFileBackup = newFileBackup;
            if (newFileBackup)
            {
                EditorGUILayout.HelpBox("アセットを変更する前に 'Assets/WorldOptimizer/Backups' にコピーを作成します。\nディスク容量を消費するため注意してください。", MessageType.None);
            }
            EditorGUILayout.Space();
            
            // DrawOptimizationRow helper logic inline or method
            DrawOptimizationRow("Missing Scriptの削除", ref enableRemoveMissingScripts, () => RunIndividual(() => RemoveMissingScripts(), "Missing Script Removal"));
            DrawOptimizationRow("GPU Instancingの有効化", ref enableGpuInstancing, () => RunIndividual(() => EnableGpuInstancing(), "GPU Instancing"));
            
            // Texture Optimization Row with Dropdown
            EditorGUILayout.BeginHorizontal();
            enableTextureCompression = EditorGUILayout.Toggle("テクスチャ圧縮の最適化", enableTextureCompression);
            if (GUILayout.Button("実行", GUILayout.Width(60))) RunIndividual(() => OptimizeTextures(), "Texture Optimization");
            EditorGUILayout.EndHorizontal();

            if (enableTextureCompression)
            {
                EditorGUI.indentLevel++;
                int[] sizes = { 512, 1024, 2048, 4096, 8192 };
                string[] sizeStrings = sizes.Select(x => x.ToString()).ToArray();
                int currentIndex = Array.IndexOf(sizes, maxTextureSize);
                if (currentIndex < 0) currentIndex = 2; // Default 2048

                int newIndex = EditorGUILayout.Popup("最大サイズ (Max Size)", currentIndex, sizeStrings);
                if (newIndex != currentIndex)
                {
                    maxTextureSize = sizes[newIndex];
                    selectedProfile = OptimizationProfile.Custom; // Switch to Custom if changed
                }
                EditorGUI.indentLevel--;
            }

            DrawOptimizationRow("オーディオ設定の最適化", ref enableAudioOptimization, () => RunIndividual(() => OptimizeAudio(), "Audio Optimization"));
            DrawOptimizationRow("Staticフラグの自動設定", ref enableStaticFlags, () => RunIndividual(() => SetStaticFlags(), "Static Flags"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("プロファイル設定", EditorStyles.boldLabel);
            var newProfile = (OptimizationProfile)EditorGUILayout.EnumPopup("最適化プロファイル", selectedProfile);
            if (newProfile != selectedProfile)
            {
                selectedProfile = newProfile;
                OnProfileChanged();
            }
            
            EditorGUILayout.Space();
            
            // Revert Button
            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
            if (GUILayout.Button("最適化を元に戻す (Revert to Original)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("確認", "最適化されたアセットを元の設定に戻しますか？\n(事前バックアップから設定を復元します)", "実行", "キャンセル"))
                {
                    BackupManager.RestoreAssets(LogErrorToFile);
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("一括最適化を実行 (Run All)", GUILayout.Height(30)))
            {
                // ... (existing run logic)
                try
                {
                    RunAllOptimizations();
                }
                catch (Exception e)
                {
                    LogErrorToFile($"[WorldOptimizer] Critical Error in RunAllOptimizations: {e}");
                    EditorUtility.DisplayDialog("Error", $"最適化中に予期せぬエラーが発生しました:\n{e.Message}", "OK");
                }
            }

            EditorGUILayout.Space();
            
            // Auto Upload Toggle
            bool autoOpt = EnableAutoOptimizeOnUpload;
            bool newAutoOpt = EditorGUILayout.ToggleLeft("アップロード時に自動で最適化を実行する (Beta)", autoOpt);
            if (newAutoOpt != autoOpt)
            {
                EnableAutoOptimizeOnUpload = newAutoOpt;
            }
            if (newAutoOpt)
            {
                EditorGUILayout.HelpBox("VRChatへのアップロード開始時に自動的に最適化を行い、終了時に元に戻します。", MessageType.Info);
            }
        }
        
        // Make this public so BuildHook can call it
        public void RunAllOptimizationsPublic(bool silent = false)
        {
            RunAllOptimizations(silent);
        }
        
        private void DrawAdvancedTab()
        {
            // ... (Mesh Combiner UI) ...
            EditorGUILayout.LabelField("メッシュ結合 (Mesh Combiner)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("選択中のオブジェクト（その子要素含む）のマテリアルごとにメッシュを結合します。元のオブジェクトは非表示になります。", MessageType.Info);
             
            if (GUILayout.Button("選択オブジェクトを結合"))
            {
                if (Selection.activeGameObject != null)
                {
                    try
                    {
                        MeshCombiner.CombineMeshesByMaterial(Selection.activeGameObject, LogErrorToFile);
                        EditorUtility.DisplayDialog("Mesh Combiner", "結合が完了しました。", "OK");
                    }
                    catch (Exception e)
                    {
                        EditorUtility.DisplayDialog("エラー", $"メッシュ結合中にエラーが発生しました:\n{e.Message}", "OK");
                        LogErrorToFile($"[WorldOptimizer] メッシュ結合エラー: {e}");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("エラー", "結合する親オブジェクトを選択してください。", "OK");
                }
            }

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            // LOD Settings
            EditorGUILayout.LabelField("LOD設定 (Level Of Detail)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("LOD Biasを調整して、遠くのオブジェクトの詳細度を制御します (小さい値ほど早く簡易表示になります)。", MessageType.None);
            
            lodBias = EditorGUILayout.Slider("LOD Bias", lodBias, 0.1f, 5.0f);
            if (GUILayout.Button("LOD設定を適用"))
            {
                QualitySettings.lodBias = lodBias;
                Debug.Log($"[WorldOptimizer] LOD Bias set to {lodBias}");
                EditorUtility.DisplayDialog("完了", $"LOD Biasを {lodBias} に設定しました。", "OK");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("LOD生成 (Experimental)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("選択中のオブジェクトに対して、簡易メッシュを自動生成しLODGroupコンポーネントを設定します。\n※Vertex Clusteringアルゴリズムによる簡易削減を行います。", MessageType.Info);
            
            if (GUILayout.Button("Generate LODs (選択オブジェクト)"))
            {
                if (Selection.activeGameObject != null)
                {
                    LODGenerator.GenerateLODs(Selection.activeGameObject, LogErrorToFile);
                    EditorUtility.DisplayDialog("完了", "LODの生成が完了しました。", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("エラー", "LODを生成するオブジェクトを選択してください。", "OK");
                }
            }

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            // Crunch Compression Settings
            EditorGUILayout.LabelField("強力な圧縮 (Crunch Compression)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("アセットをCrunch形式で圧縮し、ファイルサイズを劇的に削減します。\n※Normal MapやGUIは画質劣化を防ぐため除外されます。", MessageType.None);
            
            forceCrunch = EditorGUILayout.Toggle("Crunch圧縮を強制", forceCrunch);
            if (forceCrunch)
            {
                crunchQuality = EditorGUILayout.IntSlider("圧縮品質 (Quality)", crunchQuality, 0, 100);
            }

            if (GUILayout.Button("テクスチャ設定を適用 (Crunch含む)"))
            {
                // Temporarily override flags for this run
                bool prevTex = enableTextureCompression;
                enableTextureCompression = true;
                OptimizeTextures(); // Will use forceCrunch logic if updated
                enableTextureCompression = prevTex;
            }

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();
            
            // Download Size Optimization
            EditorGUILayout.LabelField("ダウンロードサイズ削減 (Download Size)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("テクスチャサイズを制限(1024px)し、強力な圧縮を適用して容量を削減します。\n画質は低下しますが、ダウンロード時間を短縮できます。", MessageType.Info);
            
            if (GUILayout.Button("ダウンロードサイズ優先設定を適用"))
            {
                 if (EditorUtility.DisplayDialog("確認", "テクスチャ解像度を1024pxに制限し、オーディオ品質を下げて容量を削減しますか？\n(画質・音質が低下します)", "実行", "キャンセル"))
                 {
                     OptimizeForDownloadSize();
                 }
            }

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            // Occlusion Culling Settings
            EditorGUILayout.LabelField("オクルージョンカリング (Occlusion Culling)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("シーン内の静的オブジェクトに対してオクルージョンデータを生成し、描画負荷を軽減します。", MessageType.None);
            
            smallestOccluder = EditorGUILayout.FloatField("Smallest Occluder", smallestOccluder);
            smallestHole = EditorGUILayout.FloatField("Smallest Hole", smallestHole);
            backfaceThreshold = EditorGUILayout.Slider("Backface Threshold", backfaceThreshold, 10f, 100f);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bake (生成)"))
            {
                 StaticOcclusionCulling.smallestOccluder = smallestOccluder;
                 StaticOcclusionCulling.smallestHole = smallestHole;
                 StaticOcclusionCulling.backfaceThreshold = backfaceThreshold;
                 StaticOcclusionCulling.Compute();
            }
            if (GUILayout.Button("Cancel (キャンセル)"))
            {
                StaticOcclusionCulling.Cancel();
            }
            if (GUILayout.Button("Clear (クリア)"))
            {
                StaticOcclusionCulling.Clear();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            // ... (Rest of Backface Culling, Lighting, Analysis) ...
            EditorGUILayout.LabelField("裏面カリングの強制 (Backface Culling)", EditorStyles.boldLabel);
            // ... (existing content)
            EditorGUILayout.HelpBox("全てのマテリアルを「片面描画 (Back Culling)」に強制設定します。\n" +
                                    "※注意: 葉っぱやガラスなどが透明に見える可能性があります。\n" +
                                    "非破壊モード時はマテリアルを複製して適用します。", MessageType.Warning);

            if (GUILayout.Button("すべてのマテリアルを片面描画にする (Beta)"))
            {
                if (EditorUtility.DisplayDialog("確認", "マテリアルのCull設定を強制的にBack(2)に変更します。\n見た目が変わる可能性がありますがよろしいですか？", "実行", "キャンセル"))
                {
                    OptimizeCulling();
                }
            }

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("ライティング最適化 (Lighting)", EditorStyles.boldLabel);
            if (GUILayout.Button("推奨ライトマップ設定を適用"))
            {
                OptimizeLighting();
            }
            if (selectedProfile == OptimizationProfile.Standard)
                EditorGUILayout.HelpBox("設定: Resolution 40, Bounces 1, Padding 2 (軽量)", MessageType.None);
            else
                EditorGUILayout.HelpBox("設定: Resolution 80, Bounces 2, Padding 2 (高品質)", MessageType.None);

            if (GUILayout.Button("すべてのライトをBakedにする (Set All Lights to Baked)"))
            {
                if (EditorUtility.DisplayDialog("確認", "シーン内のすべてのライトを「Baked」モードに変更しますか？", "実行", "キャンセル"))
                {
                    SetAllLightsToBaked();
                }
            }

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("メッシュ分析 (Analysis)", EditorStyles.boldLabel);
            if (GUILayout.Button("シーン内の高ポリゴンメッシュを検索"))
            {
                AnalyzeMeshes();
            }

            if (heavyMeshes.Count > 0)
            {
                EditorGUILayout.LabelField("ポリゴン数 Top 10:", EditorStyles.boldLabel);
                foreach (var item in heavyMeshes)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{item.Value} tris: {item.Key.name}");
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = item.Key;
                        EditorGUIUtility.PingObject(item.Key);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawHelpTab()
        {
            EditorGUILayout.LabelField("ヘルプ (Help)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("このツールに関するサポートやバグ報告は作者までご連絡ください。", MessageType.Info);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("トラブルシューティング", EditorStyles.boldLabel);
            if (GUILayout.Button("エラーログファイルを開く"))
            {
                string logPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "WorldOptimizer_Error.log");
                if (System.IO.File.Exists(logPath))
                {
                    EditorUtility.RevealInFinder(logPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("ログ", "エラーログファイルは見つかりませんでした。\n(まだエラーが発生していない可能性があります)", "OK");
                }
            }
            EditorGUILayout.HelpBox($"ログ保存場所:\n{System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "WorldOptimizer_Error.log")}", MessageType.None);
        }

        private void RemoveMissingScripts(bool silent = false)
        {
            try
            {
                // 非アクティブ含むシーンオブジェクトを全検索
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(go => go != null && go.transform != null && go.transform.root != null &&
                                 !EditorUtility.IsPersistent(go.transform.root.gameObject) && 
                                 !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                    .ToArray();

                int totalCleaned = 0;
                foreach (GameObject go in allObjects)
                {
                    if (go == null) continue;
                    
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    if (count > 0)
                    {
                        Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                        totalCleaned += count;
                    }
                }
                currentStats.missingScriptsCount += totalCleaned;
                if (!silent) Debug.Log($"[WorldOptimizer] {totalCleaned} 個のMissing Scriptを削除しました。");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] Missing Script削除エラー: {e.Message}");
            }
        }

        private void OptimizeCulling()
        {
            try
            {
                // シーン内のレンダラーを取得
                Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>()
                    .Where(r => r != null && r.gameObject != null && r.gameObject.scene.isLoaded)
                    .ToArray();

                int count = 0;
                foreach (var renderer in renderers)
                {
                    Material[] sharedMaterials = renderer.sharedMaterials;
                    bool materialsChanged = false;

                    for (int i = 0; i < sharedMaterials.Length; i++)
                    {
                        Material mat = sharedMaterials[i];
                        if (mat == null) continue;

                        if (mat.HasProperty("_Cull"))
                        {
                            int currentCull = mat.GetInt("_Cull");
                            // 0: Off (Double Sided), 1: Front, 2: Back
                            if (currentCull == 0) // Double Sided
                            {
                                BackupManager.RecordMaterialSettings(mat);
                                Undo.RecordObject(mat, "Change Cull Mode");
                                mat.SetInt("_Cull", 2);
                                EditorUtility.SetDirty(mat); // Make sure it saves
                                count++;
                            }
                        }
                    }

                    if (materialsChanged)
                    {
                        Undo.RecordObject(renderer, "Assign Optimized Materials");
                        renderer.sharedMaterials = sharedMaterials;
                        count++;
                    }
                }
                
                Debug.Log($"[WorldOptimizer] {count} 個のマテリアル（またはレンダラー）のカリング設定をBackに強制しました。");
                EditorUtility.DisplayDialog("完了", $"{count} 個の箇所のカリング設定を最適化しました。", "OK");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] Culling最適化エラー: {e.Message}");
            }
        }

        private void EnableGpuInstancing(bool silent = false)
        {
            try
            {
                // シーン内の全Rendererを取得 (Referencing objects directly enables swapping materials)
                Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>()
                    .Where(r => r != null && r.gameObject != null && r.gameObject.scene.isLoaded)
                    .ToArray();

                int count = 0;

                foreach (var renderer in renderers)
                {
                    Material[] sharedMaterials = renderer.sharedMaterials;

                    for (int i = 0; i < sharedMaterials.Length; i++)
                    {
                        Material mat = sharedMaterials[i];
                        if (mat == null) continue;

                        if (!mat.enableInstancing)
                        {
                            BackupManager.RecordMaterialSettings(mat);
                            Undo.RecordObject(mat, "Enable GPU Instancing");
                            mat.enableInstancing = true;
                            // EditorUtility.SetDirty(mat) is implied by RecordObject but good to be safe if strictly properties
                            count++;
                        }
                    }
                    // No need to assign back to renderer if we modified the sharedMaterial directly
                }

                currentStats.materialCount += count;
                if (!silent) Debug.Log($"[WorldOptimizer] {count} 箇所でGPU Instancingを有効化しました。");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] GPU Instancing有効化エラー: {e.Message}");
            }
        }

        private void OptimizeTextures(bool silent = false)
        {
            try
            {
                // 高速化: シーン依存のみを対象とする
                // 非破壊の場合、アセットパスではなくシーン内の参照から辿る必要があるが、
                // 依存関係リストからテクスチャを特定 -> それを使うマテリアルを特定 -> そのマテリアルを使うレンダラーを特定...
                // これはコストが高い。
                // 簡易実装: 
                // 1. Scene Dependenciesからテクスチャを特定し、Cloneを作成 & 最適化
                // 2. シーン内の全マテリアルをスキャンし、元のテクスチャを参照しているプロパティがあれば、Cloneに差し替えたマテリアルのCloneを作成して割り当てる

                List<string> assetPaths = GetSceneDependencies();
                int count = 0;
                long totalSizeDiff = 0;
                int targetCompression = selectedProfile == OptimizationProfile.Standard ? 50 : 85; 

                foreach (string path in assetPaths)
                {
                    try
                    {
                         TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                         if (importer == null) continue;

                        // Backup Logic
                        BackupManager.RecordTextureSettings(importer);
                        if (EnableFileBackup) BackupManager.CreateFileBackup(path);

                        Texture texBefore = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        long sizeBefore = texBefore != null ? UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texBefore) : 0;

                         bool dirty = ApplyTextureSettings(importer, targetCompression, maxTextureSize);
                         if (dirty)
                         {
                             importer.SaveAndReimport();
                             count++;
                             
                            Texture texAfter = AssetDatabase.LoadAssetAtPath<Texture>(path);
                            if (texAfter != null)
                            {
                                long sizeAfter = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texAfter);
                                totalSizeDiff += (sizeBefore - sizeAfter);
                            }
                         }
                    }
                    catch (Exception e) { Debug.LogWarning($"[WorldOptimizer] Texture Process Error: {e.Message}"); }
                }
                
                currentStats.textureCount += count;
                currentStats.textureMemorySaved += totalSizeDiff;
                if (!silent) Debug.Log($"[WorldOptimizer] {count} 枚のテクスチャを最適化しました。");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] テクスチャ最適化エラー: {e.Message}");
            }
        }

        private bool ApplyTextureSettings(TextureImporter importer, int compression, int maxSize)
        {
            bool dirty = false;
            
            // Texture Type Check
            bool isNormalMap = importer.textureType == TextureImporterType.NormalMap;
            bool isGUI = importer.textureType == TextureImporterType.GUI || importer.textureType == TextureImporterType.Sprite;
            
            // Streaming Mipmaps (Skip for UI settings)
            if (!isGUI && !importer.streamingMipmaps) 
            { 
                importer.streamingMipmaps = true; 
                dirty = true; 
            }

            // Compression Settings
            // If Force Crunch is enabled, we apply it (except for GUI/Sprite as it might behave unexpectedly)
            bool applyCrunch = forceCrunch;
            int qualityToUse = forceCrunch ? crunchQuality : compression;

            // Safeguard: Unless forced, don't crunch NormalMap
            if (!forceCrunch && isNormalMap) applyCrunch = false;
            
            // Safeguard: Never crunch GUI/Sprite automatically as it often breaks alpha/9-slice
            if (isGUI) applyCrunch = false; 

            if (applyCrunch)
            {
                 if (!importer.crunchedCompression) 
                 { 
                     importer.crunchedCompression = true; 
                     importer.compressionQuality = qualityToUse; 
                     dirty = true; 
                 }
                 else if (importer.compressionQuality != qualityToUse) 
                 { 
                     importer.compressionQuality = qualityToUse; 
                     dirty = true; 
                 }
            }
            else if (!isNormalMap && !isGUI) // Default behavior for others if not crunched
            {
                 // Ensure compression is enabled but maybe not crunched if we went back?
                 // Current logic only enables crunch. It doesn't disable it if it was already on.
                 // This matches previous behavior.
                 // If we want to support "Texture Optimization" toggle (crunchedCompression = true), we do it.
                 // Wait, "OptimizeTextures" generally meant "Enable Crunch" in original code.
                 // So "applyCrunch" should be true if `enableTextureCompression` is true OR `forceCrunch` is true.
                 // But `ApplyTextureSettings` is called FROM `OptimizeTextures`.
                 // So we can assume we WANT to optimize.
                 // Let's stick to: "If not Normal/GUI, Crunch."
                 // If ForceCrunch, "Even if Normal, Crunch (but not GUI)".
            }
            
            // Re-evaluating logic to match original intent + Force feature
            
            // Original logic (Restored + Modified):
            // if (!isNormalMap && !isGUI) -> Crunch = true.
            
            // New Logic:
            bool shouldCrunch = (!isNormalMap && !isGUI); // Default target
            if (forceCrunch && !isGUI) shouldCrunch = true; // Force includes NormalMap

            if (shouldCrunch)
            {
                if (!importer.crunchedCompression) 
                { 
                    importer.crunchedCompression = true; 
                    importer.compressionQuality = qualityToUse; 
                    dirty = true; 
                }
                else if (importer.compressionQuality != qualityToUse) 
                { 
                    importer.compressionQuality = qualityToUse; 
                    dirty = true; 
                }
            }

            // Max Size Limit (Apply to all)
            if (importer.maxTextureSize > maxSize) 
            { 
                importer.maxTextureSize = maxSize; 
                dirty = true; 
            }
            
            return dirty;
        }

        private void OptimizeAudio(bool silent = false)
        {
            try
            {
                // AudioSources in scene
                AudioSource[] sources = Resources.FindObjectsOfTypeAll<AudioSource>()
                    .Where(s => s != null && s.gameObject != null && s.gameObject.scene.isLoaded)
                    .ToArray();
                
                int count = 0;

                // 非破壊: AudioSourceが参照しているClipをCloneして、それを最適化し、参照を差し替える
                // 破壊的: DependenciesからClipを取得してImporterを変更（全AudioSourceに影響）

                foreach (var source in sources)
                {
                    if (source.clip == null) continue;
                    
                    string path = AssetDatabase.GetAssetPath(source.clip);
                    if (string.IsNullOrEmpty(path)) continue;

                    AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                    if (importer != null)
                    {
                        BackupManager.RecordAudioSettings(importer);
                        if (EnableFileBackup) BackupManager.CreateFileBackup(path);
                        if (ApplyAudioSettings(importer))
                        {
                            importer.SaveAndReimport();
                            count++;
                        }
                    }
                }
                
                currentStats.audioCount += count;
                if (!silent) Debug.Log($"[WorldOptimizer] {count} 個のオーディオクリップを最適化しました。");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] オーディオ最適化エラー: {e.Message}");
            }
        }

        private bool ApplyAudioSettings(AudioImporter importer)
        {
            bool dirty = false;
            AudioImporterSampleSettings defaultSettings = importer.defaultSampleSettings;
            
            if (!importer.forceToMono) { importer.forceToMono = true; dirty = true; }

            if (defaultSettings.loadType == AudioClipLoadType.DecompressOnLoad && 
                defaultSettings.compressionFormat == AudioCompressionFormat.PCM)
            {
                defaultSettings.loadType = AudioClipLoadType.CompressedInMemory;
                defaultSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                defaultSettings.quality = 0.7f;
                dirty = true;
            }

            if (dirty) importer.defaultSampleSettings = defaultSettings;
            return dirty;
        }

        private void SetStaticFlags(bool silent = false)
        {
            try
            {
                // シーンオブジェクトのみ
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(go => go != null && go.transform != null && go.transform.root != null &&
                                 !EditorUtility.IsPersistent(go.transform.root.gameObject) && 
                                 !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                    .ToArray();
                int count = 0;

                foreach (GameObject go in allObjects)
                {
                    if (go == null) continue;
                    
                    if (go.GetComponent<Rigidbody>() == null && go.GetComponent<Collider>() != null)
                    {
                        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                        StaticEditorFlags desired = StaticEditorFlags.BatchingStatic | 
                                                    StaticEditorFlags.OccludeeStatic | 
                                                    StaticEditorFlags.OccluderStatic | 
                                                    StaticEditorFlags.ReflectionProbeStatic;
                        
                        if ((flags & desired) != desired)
                        {
                            Undo.RecordObject(go, "Set Static Flags");
                            GameObjectUtility.SetStaticEditorFlags(go, flags | desired);
                            count++;
                        }
                    }
                }
                
                currentStats.staticFlagsCount += count;
                if (!silent) Debug.Log($"[WorldOptimizer] {count} 個のオブジェクトにStaticフラグを設定しました。");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] Staticフラグ設定エラー: {e.Message}");
            }
        }
        private void RunAllOptimizations(bool silent = false)
        {
            currentStats = new OptimizationStats();
            
            AssetDatabase.StartAssetEditing();
            BackupManager.BeginTransaction();
            
            try
            {
                if (enableRemoveMissingScripts) RemoveMissingScripts(silent);
                if (enableGpuInstancing) EnableGpuInstancing(silent);
                if (enableTextureCompression) OptimizeTextures(silent);
                if (enableAudioOptimization) OptimizeAudio(silent);
                if (enableStaticFlags) SetStaticFlags(silent);
            }
            finally
            {
                BackupManager.CommitTransaction();
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Show Report
            string report = "最適化が完了しました。\n\n" +
                            $"削除されたMissing Scripts: {currentStats.missingScriptsCount}\n" +
                            $"GPU Instancing有効化: {currentStats.materialCount} マテリアル/レンダラー\n" +
                            $"最適化されたテクスチャ: {currentStats.textureCount} 枚\n" +
                            $"  - 削減サイズ (概算): {EditorUtility.FormatBytes(currentStats.textureMemorySaved)}\n" +
                            $"最適化されたオーディオ: {currentStats.audioCount} 個\n" +
                            $"Staticフラグ設定: {currentStats.staticFlagsCount} オブジェクト";
            
            if (!silent)
            {
                Debug.Log($"[WorldOptimizer] Report:\n{report}");
                EditorUtility.DisplayDialog("完了", report, "OK");
            }
            else
            {
                Debug.Log($"[WorldOptimizer] Auto-Optimization Report:\n{report}");
            }
        }

        private List<string> GetSceneDependencies()
        {
            // 現在のシーンの依存関係のみを取得
            string[] scenePaths = new string[] { SceneManager.GetActiveScene().path };
            return AssetDatabase.GetDependencies(scenePaths, true).ToList();
        }

        private void OptimizeLighting()
        {
             // Simple lighting settings
             Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
             
             // Create or load LightingSettings
             LightingSettings settings = Lightmapping.GetLightingSettingsForScene(SceneManager.GetActiveScene());
             if (settings == null) settings = new LightingSettings();

             if (selectedProfile == OptimizationProfile.Standard)
             {
                 settings.lightmapCompression = LightmapCompression.NormalQuality;
                 settings.lightmapMaxSize = 2048;
                 settings.directSampleCount = 32;
             }
             else
             {
                 settings.lightmapMaxSize = 4096;
                 settings.directSampleCount = 64;
             }
             
             Lightmapping.SetLightingSettingsForScene(SceneManager.GetActiveScene(), settings);
             Debug.Log("[WorldOptimizer] ライトマップ設定を調整しました。");
        }

        private void SetAllLightsToBaked()
        {
            try
            {
                Light[] lights = Resources.FindObjectsOfTypeAll<Light>()
                    .Where(l => l != null && l.gameObject != null && l.gameObject.scene.isLoaded)
                    .ToArray();
                
                int count = 0;
                foreach (var light in lights)
                {
                    if (light.lightmapBakeType != LightmapBakeType.Baked)
                    {
                        Undo.RecordObject(light, "Set Light to Baked");
                        light.lightmapBakeType = LightmapBakeType.Baked;
                        count++;
                    }
                }
                Debug.Log($"[WorldOptimizer] {count} 個のライトをBakedモードに変更しました。");
                EditorUtility.DisplayDialog("完了", $"{count} 個のライトをBakedモードに変更しました。", "OK");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] Light Bake Change Error: {e.Message}");
            }
        }


        private void OptimizeForDownloadSize()
        {
            AssetDatabase.StartAssetEditing();
            BackupManager.BeginTransaction();
            try
            {
                // Similar to OptimizeTextures but enforcing aggressive settings
                List<string> assetPaths = GetSceneDependencies();
                int count = 0;
                
                // Aggressive Settings
                int targetMax = 1024;
                int agressiveQuality = 50;
                
                foreach (string path in assetPaths)
                {
                    // Textures
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(Texture2D))
                    {
                         TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                         if (importer != null)
                         {
                            // Backup
                            BackupManager.RecordTextureSettings(importer);
                            if (EnableFileBackup) BackupManager.CreateFileBackup(path);
                            
                            // Logic similar to ApplyTextureSettings but FORCE overrides
                            bool dirty = false;
                            
                            // Force Max Size 1024
                            if (importer.maxTextureSize > targetMax) { importer.maxTextureSize = targetMax; dirty = true; }
                            
                            // Force Crunch
                            if (!importer.textureType.ToString().Contains("GUI") && importer.textureType != TextureImporterType.Sprite && importer.textureType != TextureImporterType.NormalMap)
                            {
                                if (!importer.crunchedCompression) { importer.crunchedCompression = true; dirty = true; }
                                if (importer.compressionQuality != agressiveQuality) { importer.compressionQuality = agressiveQuality; dirty = true; }
                            }
                            
                            if (dirty)
                            {
                                importer.SaveAndReimport();
                                count++;
                            }
                         }
                    }
                    // Audio
                    else if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(AudioClip))
                    {
                         AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                         if (importer != null)
                         {
                            BackupManager.RecordAudioSettings(importer);
                            if (EnableFileBackup) BackupManager.CreateFileBackup(path);
                            
                            bool dirty = false;
                            if (!importer.forceToMono) { importer.forceToMono = true; dirty = true; }
                            
                            var settings = importer.defaultSampleSettings;
                            if (settings.loadType != AudioClipLoadType.CompressedInMemory) { settings.loadType = AudioClipLoadType.CompressedInMemory; dirty = true; }
                            if (settings.compressionFormat != AudioCompressionFormat.Vorbis) { settings.compressionFormat = AudioCompressionFormat.Vorbis; dirty = true; }
                            if (settings.quality > 0.5f) { settings.quality = 0.5f; dirty = true; } // Low quality
                            
                            if (dirty)
                            {
                                importer.defaultSampleSettings = settings;
                                importer.SaveAndReimport();
                                count++;
                            }
                         }
                    }
                }
                
                Debug.Log($"[WorldOptimizer] ダウンロードサイズ削減のための最適化が完了しました。対象アセット: {count}");
                EditorUtility.DisplayDialog("完了", $"ダウンロードサイズ優先設定を適用しました。\n対象アセット数: {count}", "OK");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] Download Size Optimization Error: {e.Message}");
            }
            finally
            {
                BackupManager.CommitTransaction();
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        private void LogErrorToFile(string message)
        {
            try
            {
                string logPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "WorldOptimizer_Error.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string content = $"[{timestamp}] {message}\n";
                System.IO.File.AppendAllText(logPath, content);
            }
            catch (Exception)
            {
                // Fallback if logging fails
            }
            // Always show in console
            Debug.LogError(message);
        }

        private void AnalyzeMeshes()
        {
            heavyMeshes.Clear();
            Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>()
                    .Where(r => r != null && r.gameObject != null && r.gameObject.scene.isLoaded)
                    .ToArray();

            foreach(var r in renderers)
            {
                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    int tris = mf.sharedMesh.triangles.Length / 3;
                    if (tris > 1000) // Filter small ones
                    {
                        heavyMeshes.Add(new KeyValuePair<GameObject, int>(r.gameObject, tris));
                    }
                }
            }
            // Sort
            heavyMeshes.Sort((a, b) => b.Value.CompareTo(a.Value));
            if (heavyMeshes.Count > 10) heavyMeshes = heavyMeshes.GetRange(0, 10);
        }

        // Stats Class
        private class OptimizationStats
        {
            public int missingScriptsCount = 0;
            public int materialCount = 0;
            public int textureCount = 0;
            public long textureMemorySaved = 0;
            public int audioCount = 0;
            public int staticFlagsCount = 0;
        }
        
        private OptimizationStats currentStats = new OptimizationStats();

        // Helpers
        private void DrawOptimizationRow(string label, ref bool toggle, Action onRun)
        {
            EditorGUILayout.BeginHorizontal();
            toggle = EditorGUILayout.Toggle(label, toggle);
            if (GUILayout.Button("実行", GUILayout.Width(60)))
            {
                onRun?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RunIndividual(Action action, string name)
        {
            try
            {
                currentStats = new OptimizationStats(); // Reset stats for this run
                
                action?.Invoke();
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                string msg = $"{name} が完了しました。";
                Debug.Log($"[WorldOptimizer] {msg}");
                EditorUtility.DisplayDialog("完了", msg, "OK");
            }
            catch (Exception e)
            {
                LogErrorToFile($"[WorldOptimizer] Error in {name}: {e}");
                EditorUtility.DisplayDialog("Error", $"エラーが発生しました:\n{e.Message}", "OK");
            }
        }

        private void OnProfileChanged()
        {
            if (selectedProfile == OptimizationProfile.Standard)
            {
                maxTextureSize = 2048;
            }
            else if (selectedProfile == OptimizationProfile.HighQuality)
            {
                maxTextureSize = 4096;
            }
            // Custom: do nothing
        }
    }
}

