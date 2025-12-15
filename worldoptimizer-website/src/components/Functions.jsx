import React from 'react';

const Functions = () => {
    const functions = [
        {
            id: 1,
            title: 'テクスチャ最適化',
            description: '適切な圧縮形式への変換、解像度調整、不要なテクスチャの削除'
        },
        {
            id: 2,
            title: 'メッシュ最適化',
            description: 'ポリゴン数削減、LOD生成、重複頂点の削除'
        },
        {
            id: 3,
            title: 'オーディオ最適化',
            description: '音声ファイルの圧縮、ビットレート調整、不要なトラック削除'
        },
        {
            id: 4,
            title: 'Missing Scripts除去',
            description: '欠損スクリプトの自動検出と削除'
        },
        {
            id: 5,
            title: 'GPU Instancing有効化',
            description: '同一メッシュの描画を最適化し、ドローコールを削減'
        },
        {
            id: 6,
            title: 'ライトマップ圧縮',
            description: 'ライトマップテクスチャを圧縮し、メモリ使用量を削減'
        }
    ];

    return (
        <section className="functions">
            <div className="container">
                <h2 className="section-title">機能一覧</h2>
                <div className="functions-grid">
                    {functions.map(func => (
                        <div key={func.id} className="function-card">
                            <h3 className="function-title">{func.title}</h3>
                            <p className="function-description">{func.description}</p>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
};

export default Functions;
