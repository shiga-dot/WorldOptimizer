# LOD機能の修正

- [x] 現状のコード解析と原因特定
    - [x] WorldOptimizerWindow.csの解析: LOD機能は `QualitySettings.lodBias` の変更のみ
    - [x] MeshCombiner.csの解析: 結合のみで削減機能なし
    - [x] プロジェクト内の既存ライブラリ検索: Simplifierは見つからず
- [ ] 修正方針の策定 (Implementation Plan)
- [ ] 修正の実装
- [ ] 動作確認 (Walkthrough)
