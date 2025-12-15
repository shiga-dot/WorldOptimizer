# World Optimizer 公式ホームページ

## 概要
World OptimizerのReact製公式ホームページです。

## ディレクトリ構成
```
worldoptimizer-website/
├── index.html              # HTMLテンプレート
├── package.json            # 依存関係
├── vite.config.js          # Vite設定
├── src/
│   ├── main.jsx            # エントリーポイント
│   ├── App.jsx             # メインコンポーネント
│   ├── App.css             # メインスタイル
│   └── components/         # 各セクションコンポーネント
│       ├── Hero.jsx        # ヒーローセクション
│       ├── Features.jsx    # 特徴セクション
│       ├── Functions.jsx   # 機能一覧セクション
│       ├── HowItWorks.jsx  # 使い方セクション
│       ├── UseCases.jsx    # 利用シーンセクション
│       ├── FAQ.jsx         # FAQセクション
│       └── Footer.jsx      # フッター
```

## セットアップ

### 依存関係のインストール
```bash
npm install
```

### 開発サーバーの起動
```bash
npm run dev
```
ブラウザで `http://localhost:3000` が自動的に開きます。

### ビルド
```bash
npm run build
```
`dist/` ディレクトリに本番用ファイルが生成されます。

### プレビュー
```bash
npm run preview
```
ビルドしたファイルをプレビューできます。

## 技術スタック
- **React 18** - UIライブラリ
- **Vite** - ビルドツール
- **CSS** - スタイリング（外部ライブラリなし）

## 特徴
- ✅ レスポンシブデザイン（PC / タブレット / スマホ対応）
- ✅ モダンでシンプルなUI
- ✅ アクセシビリティ対応
- ✅ コンポーネント分割による保守性
- ✅ FAQアコーディオン（React状態管理）

## ページ構成
1. **Hero** - キャッチコピーとCTAボタン
2. **Features** - 5つの主な特徴
3. **Functions** - 6つの機能一覧
4. **HowItWorks** - 3ステップの使い方
5. **UseCases** - 3つの利用シーン
6. **FAQ** - よくある質問（アコーディオン）
7. **Footer** - リンクとコピーライト
