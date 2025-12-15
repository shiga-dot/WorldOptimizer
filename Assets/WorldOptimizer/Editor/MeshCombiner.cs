using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace WorldOptimizer
{
    /// <summary>
    /// メッシュ結合ユーティリティ
    /// </summary>
    public static class MeshCombiner
    {
        /// <summary>
        /// 指定されたGameObjectとその子を、マテリアルごとにメッシュ結合する
        /// </summary>
        public static void CombineMeshesByMaterial(GameObject root, System.Action<string> onError = null)
        {
            if (root == null)
            {
                string msg = "[WorldOptimizer] 結合対象のGameObjectがnullです。";
                Debug.LogError(msg);
                onError?.Invoke(msg);
                return;
            }

            try
            {
                // すべてのMeshFilterを取得（子オブジェクト含む）
                MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
                
                if (meshFilters == null || meshFilters.Length == 0)
                {
                    Debug.LogWarning("[WorldOptimizer] 結合可能なメッシュが見つかりませんでした。");
                    EditorUtility.DisplayDialog("警告", "結合可能なメッシュが見つかりませんでした。", "OK");
                    return;
                }

                // マテリアルごとにCombineInstanceをグループ化
                Dictionary<Material, List<CombineInstance>> materialToCombine = new Dictionary<Material, List<CombineInstance>>();
                List<MeshRenderer> processedRenderers = new List<MeshRenderer>();

                foreach (MeshFilter mf in meshFilters)
                {
                    if (mf == null || mf.sharedMesh == null) continue;
                    
                    MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                    if (mr == null || mr.sharedMaterial == null) continue;

                    Material mat = mr.sharedMaterial;
                    
                    if (!materialToCombine.ContainsKey(mat))
                    {
                        materialToCombine[mat] = new List<CombineInstance>();
                    }

                    CombineInstance ci = new CombineInstance
                    {
                        mesh = mf.sharedMesh,
                        // Bake into Root's local space to avoid double transform when parenting
                        transform = root.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix
                    };
                    
                    materialToCombine[mat].Add(ci);
                    processedRenderers.Add(mr);
                }

                if (materialToCombine.Count == 0)
                {
                    Debug.LogWarning("[WorldOptimizer] 結合可能なメッシュが見つかりませんでした（マテリアル不足）。");
                    return;
                }

                // 結合メッシュを作成
                int combinedCount = 0;
                foreach (var kvp in materialToCombine)
                {
                    Material mat = kvp.Key;
                    List<CombineInstance> combines = kvp.Value;

                    // 1つしかない場合は結合しない
                    if (combines.Count < 2) continue;

                    try
                    {
                        GameObject combinedGo = new GameObject($"Combined_{(mat != null ? mat.name : "Unknown")}");
                        combinedGo.transform.parent = root.transform;
                        combinedGo.transform.localPosition = Vector3.zero;
                        combinedGo.transform.localRotation = Quaternion.identity;
                        combinedGo.transform.localScale = Vector3.one;

                        MeshFilter newMf = combinedGo.AddComponent<MeshFilter>();
                        newMf.mesh = new Mesh();
                        newMf.mesh.name = $"CombinedMesh_{mat.name}";

                        // 頂点数をチェックして32bitインデックスバッファが必要か判定
                        long totalVertexCount = combines.Sum(c => c.mesh != null ? c.mesh.vertexCount : 0);
                        
                        if (totalVertexCount > 65535)
                        {
                            newMf.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                            Debug.Log($"[WorldOptimizer] {mat.name}: 32bitインデックスバッファを使用します（頂点数: {totalVertexCount}）");
                        }

                        // メッシュを結合
                        newMf.mesh.CombineMeshes(combines.ToArray(), true, true);
                        
                        // マテリアルを設定
                        MeshRenderer newMr = combinedGo.AddComponent<MeshRenderer>();
                        newMr.sharedMaterial = mat;

                        // Undoに登録
                        Undo.RegisterCreatedObjectUndo(combinedGo, "Combine Meshes");
                        
                        combinedCount++;
                    }
                    catch (System.Exception e)
                    {
                        string msg = $"[WorldOptimizer] メッシュ結合エラー (マテリアル: {mat.name}): {e.Message}";
                        Debug.LogError(msg);
                        onError?.Invoke(msg);
                    }
                }

                // 元のレンダラーを無効化（削除はしない、安全のため）
                foreach (MeshRenderer mr in processedRenderers)
                {
                    if (mr != null)
                    {
                        Undo.RecordObject(mr.gameObject, "Disable Original Renderer");
                        mr.enabled = false;
                    }
                }

                Debug.Log($"[WorldOptimizer] {combinedCount} 個の結合メッシュを作成しました。元のレンダラーは無効化されました。");
                
                if (combinedCount > 0)
                {
                    EditorUtility.DisplayDialog("メッシュ結合", 
                        $"{combinedCount} 個の結合メッシュを作成しました。\n元のオブジェクトは無効化されています。", 
                        "OK");
                }
            }
            catch (System.Exception e)
            {
                string msg = $"[WorldOptimizer] メッシュ結合処理エラー: {e.Message}\n{e.StackTrace}";
                Debug.LogError(msg);
                onError?.Invoke(msg);
                throw;
            }
        }
    }
}
