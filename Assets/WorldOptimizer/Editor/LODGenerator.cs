using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace WorldOptimizer
{
    public static class LODGenerator
    {
        private class VertexInfo
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public int oldIndex;
        }

        public static void GenerateLODs(GameObject target, System.Action<string> onError = null)
        {
            if (target == null) return;

            // Check if LODGroup already exists
            LODGroup lodGroup = target.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                if (!EditorUtility.DisplayDialog("確認", 
                    $"ターゲット '{target.name}' には既にLODGroupが存在します。\n上書きして再生成しますか？\n(既存のLODメッシュは削除されませんが、設定は上書きされます)", 
                    "上書き (Overwrite)", "キャンセル"))
                {
                    return;
                }
            }
            else
            {
                lodGroup = Undo.AddComponent<LODGroup>(target);
            }

            // Get original renderer/mesh
            MeshRenderer originalRenderer = target.GetComponent<MeshRenderer>();
            MeshFilter originalFilter = target.GetComponent<MeshFilter>();
            SkinnedMeshRenderer unusedSkinned = target.GetComponent<SkinnedMeshRenderer>();

            if (unusedSkinned != null)
            {
                string msg = "SkinnedMeshRenderer (アバター等) へのLOD生成は現在サポートされていません。";
                Debug.LogWarning($"[WorldOptimizer] {msg}");
                onError?.Invoke(msg);
                return;
            }

            if (originalRenderer == null || originalFilter == null || originalFilter.sharedMesh == null)
            {
                string msg = "有効なMeshRendererまたはMeshFilterが見つかりません。";
                Debug.LogWarning($"[WorldOptimizer] {msg}");
                onError?.Invoke(msg);
                return;
            }

            Mesh sourceMesh = originalFilter.sharedMesh;
            Material[] mats = originalRenderer.sharedMaterials;

            // --- Generate LOD Meshes ---
            // LOD0: Original
            // LOD1: 50% reduction (approx)
            // LOD2: 20% reduction (approx)

            List<LOD> lods = new List<LOD>();
            
            // LOD 0 (Original)
            lods.Add(new LOD(0.6f, new Renderer[] { originalRenderer }));

            try
            {
                // Create LOD 1
                GameObject lod1GO = CreateLODObject(target, "LOD1", sourceMesh, mats, 0.5f); // 50% quality? No, grid size.
                // Simple clustering doesn't use percentage exactly. Pass cell size relative to bounds.
                // Or pass "Quality" where 1.0 is min cell size, 0.0 is max.
                
                // Let's implement SimplifyMesh which takes a grid size based on object bounds.
                // 0.01 relative to bounds size might be subtle, 0.1 might be blocky.
                
                if (lod1GO != null) 
                    lods.Add(new LOD(0.3f, new Renderer[] { lod1GO.GetComponent<Renderer>() }));

                // Create LOD 2
                GameObject lod2GO = CreateLODObject(target, "LOD2", sourceMesh, mats, 0.2f);
                if (lod2GO != null)
                    lods.Add(new LOD(0.1f, new Renderer[] { lod2GO.GetComponent<Renderer>() }));
                
                // Set LODs
                lodGroup.SetLODs(lods.ToArray());
                lodGroup.RecalculateBounds();
                
                Debug.Log($"[WorldOptimizer] Generated LODs for {target.name}");
            }
            catch (System.Exception e)
            {
                string msg = $"LOD生成中にエラーが発生しました: {e.Message}";
                Debug.LogError($"[WorldOptimizer] {msg}\n{e.StackTrace}");
                onError?.Invoke(msg);
            }
        }

        private static GameObject CreateLODObject(GameObject parent, string suffix, Mesh sourceMesh, Material[] mats, float qualityRatio)
        {
            // Vertex Clustering Simplification
            // QualityRatio: 1.0 = High, 0.0 = Low. 
            // We map this to Grid Size.
            // Bounds Size / (Resolution)
            // Resolution of 50 means 50 cells across the object.
            
            int resolution = Mathf.Max(5, Mathf.RoundToInt(50 * qualityRatio)); 
            
            Mesh simplifiedMesh = SimplifyMeshVertexClustering(sourceMesh, resolution);
            if (simplifiedMesh == null) return null;
            
            simplifiedMesh.name = $"{sourceMesh.name}_{suffix}";

            GameObject go = new GameObject($"{parent.name}_{suffix}");
            go.transform.parent = parent.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = simplifiedMesh;
            
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = mats;
            
            Undo.RegisterCreatedObjectUndo(go, "Create LOD Object");
            
            return go;
        }

        private static Mesh SimplifyMeshVertexClustering(Mesh source, int gridResolution)
        {
            Vector3[] vertices = source.vertices;
            int[] triangles = source.triangles;
            Vector3[] normals = source.normals;
            Vector2[] uvs = source.uv;
            
            if (vertices.Length == 0) return null;

            Bounds bounds = source.bounds;
            float maxDim = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (maxDim <= 0) maxDim = 1.0f; // Safety
            
            float cellSize = maxDim / gridResolution;
            
            // Grid Key -> Averaged Vertex
            Dictionary<Vector3Int, List<int>> grid = new Dictionary<Vector3Int, List<int>>();
            int[] oldToNewIndex = new int[vertices.Length];
            
            Vector3 relativeOrigin = bounds.min;

            // 1. Cluster Vertices
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 p = vertices[i];
                Vector3Int key = new Vector3Int(
                    Mathf.FloorToInt((p.x - relativeOrigin.x) / cellSize),
                    Mathf.FloorToInt((p.y - relativeOrigin.y) / cellSize),
                    Mathf.FloorToInt((p.z - relativeOrigin.z) / cellSize)
                );

                if (!grid.ContainsKey(key))
                {
                    grid[key] = new List<int>();
                }
                grid[key].Add(i);
            }

            // 2. Create Representative Vertices
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            List<Vector2> newUVs = new List<Vector2>();

            // We iterate grid values but we need a deterministic order or mapping so triangles use correct indices.
            // Actually, we can just build the new list and store the index mapping.
            // Wait, we need to know WHICH new index corresponds to which OLD index.
            
            // Map: GridKey -> NewIndex
            Dictionary<Vector3Int, int> keyToNewIndex = new Dictionary<Vector3Int, int>();
            
            int currentNewIndex = 0;
            foreach (var kvp in grid)
            {
                List<int> indices = kvp.Value;
                Vector3 avgPos = Vector3.zero;
                Vector3 avgNorm = Vector3.zero;
                Vector2 avgUV = Vector2.zero;

                foreach (int idx in indices)
                {
                    avgPos += vertices[idx];
                    if (normals != null && normals.Length > idx) avgNorm += normals[idx];
                    if (uvs != null && uvs.Length > idx) avgUV += uvs[idx];
                }

                avgPos /= indices.Count;
                avgNorm = avgNorm.normalized;
                avgUV /= indices.Count;

                newVertices.Add(avgPos);
                newNormals.Add(avgNorm);
                newUVs.Add(avgUV);

                keyToNewIndex[kvp.Key] = currentNewIndex;
                
                // Map all old indices to this new index
                foreach (int idx in indices)
                {
                    oldToNewIndex[idx] = currentNewIndex;
                }
                
                currentNewIndex++;
            }

            // 3. Rebuild Triangles
            List<int> newTriangles = new List<int>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int newIdx0 = oldToNewIndex[triangles[i]];
                int newIdx1 = oldToNewIndex[triangles[i+1]];
                int newIdx2 = oldToNewIndex[triangles[i+2]];

                // Degenerate triangle check (if any 2 indices are same, it's a line or point)
                if (newIdx0 != newIdx1 && newIdx1 != newIdx2 && newIdx2 != newIdx0)
                {
                    newTriangles.Add(newIdx0);
                    newTriangles.Add(newIdx1);
                    newTriangles.Add(newIdx2);
                }
            }
            
            if (newTriangles.Count == 0) return null; // Simplified to nothing

            Mesh newMesh = new Mesh();
            // Check 16bit vs 32bit support
            if (newVertices.Count > 65535) newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            
            newMesh.SetVertices(newVertices);
            newMesh.SetNormals(newNormals);
            newMesh.SetUVs(0, newUVs);
            newMesh.SetTriangles(newTriangles, 0);
            
            newMesh.RecalculateBounds();
            // newMesh.RecalculateNormals(); // Optional, but averaging original normals usually looks better for LODs
            
            return newMesh;
        }
    }
}
