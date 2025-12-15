# GitHubアップロード計画

「プログラムのeditor内」および作成したウェブサイトの両方をGitHubで管理・公開するための計画です。
プロジェクト全体（Unityツール + ウェブサイト）を1つのリポジトリとして構成することを推奨します。

## 対象ファイル
1. **Unityツール (Editorコード)**
   - `Assets/WorldOptimizer/Editor/` 内のC#スクリプト群
2. **ウェブサイト**
   - `worldoptimizer-website/` 内のReact/Viteプロジェクト

## 手順

### 1. Gitの初期化 (Local)
プロジェクトのルートディレクトリ (`stellar-ionosphere`) でGitリポジトリを作成します。

### 2. .gitignore の作成
不要なファイル（Unityの一時ファイルや、ウェブサイトの `node_modules` など）を除外設定します。
- `Temp/`, `Library/` (Unity)
- `node_modules/`, `dist/` (Web)

### 3. コミット
ファイルをステージングし、最初のコミットを行います。

### 4. GitHubへのプッシュ
**ユーザー作業:** GitHubで新しいリポジトリを作成し、そのURLを教えていただく必要があります。
その後、リモートリポジトリとして登録し、プッシュします。

## 確認事項
「プログラムのeditor内」というご指示が、上記 `Assets/.../Editor` フォルダを指しているか、念のため確認しながら進めます。
