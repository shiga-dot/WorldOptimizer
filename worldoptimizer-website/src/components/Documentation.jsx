import React from 'react';

const Documentation = () => {
    // 内部リンクでのハッシュ変更を防ぎ、スムーズスクロールを実行する関数
    const handleScroll = (e, id) => {
        e.preventDefault();
        const element = document.getElementById(id);
        if (element) {
            // ナビゲーションバーの分だけオフセットを考慮
            const headerOffset = 100;
            const elementPosition = element.getBoundingClientRect().top;
            const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

            window.scrollTo({
                top: offsetPosition,
                behavior: 'smooth'
            });
        }
    };

    return (
        <div className="page-container documentation">
            <div className="doc-sidebar">
                <h3>目次</h3>
                <ul>
                    <li>
                        <a href="#install" onClick={(e) => handleScroll(e, 'install')}>
                            インストール
                        </a>
                    </li>
                    <li>
                        <a href="#getting-started" onClick={(e) => handleScroll(e, 'getting-started')}>
                            はじめに
                        </a>
                    </li>
                    <li>
                        <a href="#features" onClick={(e) => handleScroll(e, 'features')}>
                            機能詳細
                        </a>
                    </li>
                    <li>
                        <a href="#api" onClick={(e) => handleScroll(e, 'api')}>
                            APIリファレンス
                        </a>
                    </li>
                </ul>
            </div>
            <div className="doc-content">
                <h1>ドキュメント</h1>

                <section id="install">
                    <h2>インストール方法</h2>
                    <p>Unity Package Managerを使用してインストールします。</p>
                    <div className="code-block">
                        <code>https://github.com/user/world-optimizer.git</code>
                    </div>
                </section>

                <section id="getting-started">
                    <h2>はじめに</h2>
                    <p>World Optimizerを導入すると、メニューバーに「Tools &gt; World Optimizer」が追加されます。</p>
                    <p>ウィンドウを開き、「Analyze Scene」ボタンをクリックして現在のシーンを分析してください。</p>
                </section>

                <section id="features">
                    <h2>機能詳細</h2>
                    <h3>テクスチャ最適化</h3>
                    <p>プロジェクト内の全テクスチャをスキャンし、プラットフォームに最適な圧縮形式に一括変換します。</p>

                    <h3>メッシュ結合</h3>
                    <p>Staticなメッシュを結合し、ドローコールを削減します。マテリアルごとに自動的にグループ化されます。</p>

                    <h3>オーディオ最適化</h3>
                    <p>長時間のBGMや短いSEを自動判別し、適切な圧縮設定（Vorbis/ADPCM）とLoad Typeを適用します。</p>
                </section>

                <section id="api">
                    <h2>APIリファレンス</h2>
                    <p>エディタ拡張から最適化機能を呼び出すことも可能です。</p>
                    <div className="code-block">
                        <code>WorldOptimizer.Core.RunOptimization();</code>
                    </div>
                </section>
            </div>
        </div>
    );
};

export default Documentation;
