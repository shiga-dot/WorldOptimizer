# 実装計画: LOD生成機能の追加

## ゴール
「LOD機能が動かない」という問題を解決するため、現在は存在しない **LODメッシュの自動生成機能** を実装する。
`WorldOptimizer` に、高ポリゴンメッシュから自動的に簡易モデル（LOD1, LOD2）を生成し、`LODGroup` コンポーネントを設定する機能を追加する。

## ユーザーレビューが必要な事項
- **メッシュ削減アルゴリズム**: 外部ライブラリ（UnityMeshSimplifier等）への依存を避けるため、簡易的な「Vertex Clustering（グリッドベース結合）」アルゴリズムを独自実装する。
  - メリット: 追加パッケージ不要、高速、堅牢。
  - デメリット: 市販の削減ツールより品質は劣る場合がある（形状がブロック状になりやすい）。

## 変更内容

### WorldOptimizer
#### [NEW] [LODGenerator.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/LODGenerator.cs)
- メッシュ削減とLODGroup設定を行うクラス。
- `GenerateLODs(GameObject target)`: 対象オブジェクトに対してLODsを生成。
- `SimplifyMesh(Mesh source, float quality)`: メッシュの頂点を間引く。

#### [MODIFY] [WorldOptimizerWindow.cs](file:///c:/Users/miiku/.gemini/antigravity/playground/stellar-ionosphere/Assets/WorldOptimizer/Editor/WorldOptimizerWindow.cs)
- `DrawAdvancedTab` 内のLOD設定セクションを拡張。
- "LOD Bias" 設定だけでなく、**"Generate LODs (LOD生成)"** ボタンを追加。
- 実行時に `LODGenerator` を呼び出す処理を追加。

## 検証計画

### 手動検証
1. **Scene Viewでの確認**:
   - 高ポリゴン（例: Sphere 5000 tris）を配置。
   - ツールから「Generate LODs」を実行。
   - Inspectorで `LODGroup` コンポーネントが追加されているか確認。
   - カメラを遠ざけた際に、LOD1, LOD2（削減されたメッシュ）に切り替わるか確認。
   - `Stats` ウィンドウで三角形数が減っているか確認。
